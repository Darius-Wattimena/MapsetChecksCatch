using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MapsetParser.objects;
using MapsetParser.objects.hitobjects;

namespace MapsetChecksCatch.Helper
{
    public static class BeatmapDistanceCalculator
    {
        public static List<CatchHitObject> Calculate(Beatmap beatmap)
        {
            var hitObjects = GenerateCatchHitObjects(beatmap);
            CalculateDistances(hitObjects, beatmap);
            return hitObjects;
        }

        private static List<CatchHitObject> GenerateCatchHitObjects(Beatmap beatmap)
        {
            var mapObjects = beatmap.hitObjects;
            var objects = new List<CatchHitObject>();

            // Get all the catch objects from the spinners
            foreach (var mapSliderObject in mapObjects.OfType<Slider>())
            {
                var objectExtras = new List<CatchHitObject>();
                var objectCode = mapSliderObject.code.Split(',');

                // The first object of a slider is always its head
                var sliderObject = new CatchHitObject(objectCode, beatmap, NoteType.HEAD);

                var edgeTimes = GetEdgeTimes(mapSliderObject).ToList();

                if (edgeTimes.Count == 1)
                {
                    // We only have a slider end so can specify this as a "Tail"
                    objectExtras.AddRange(CreateObjectExtra(beatmap, mapSliderObject, edgeTimes, objectCode, NoteType.TAIL));
                }
                else
                {
                    // We have a repeat so the slider end is the last object
                    var lastObjectArray = new[] { edgeTimes.Last() };
                    
                    objectExtras.AddRange(CreateObjectExtra(beatmap, mapSliderObject, lastObjectArray, objectCode, NoteType.TAIL));

                    // Remove the last object from the edgeTimes so we only have repeats
                    edgeTimes.RemoveAt(edgeTimes.Count - 1);

                    objectExtras.AddRange(CreateObjectExtra(beatmap, mapSliderObject, edgeTimes, objectCode, NoteType.REPEAT));
                }

                foreach (var sliderTick in mapSliderObject.sliderTickTimes)
                {
                    // Only add a slider tick to the objects extra if not present yet
                    if (objectExtras.Any(x => x.time.Equals(sliderTick)))
                    {
                        continue;
                    }

                    objectExtras.AddRange(CreateObjectExtra(beatmap, mapSliderObject, new [] { sliderTick }, objectCode, NoteType.DROPLET));
                }

                objectExtras.Sort((h1, h2) => h1.time.CompareTo(h2.time));

                sliderObject.Extras = objectExtras;
                objects.Add(sliderObject);
            }

            // Add all circles
            objects.AddRange(
                from mapObject in mapObjects
                where mapObject is Circle
                select new CatchHitObject(mapObject.code.Split(','), beatmap, NoteType.CIRCLE)
            );

            

            objects.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            return new HashSet<CatchHitObject>(objects, new TimeComparer()).ToList();
        }

        private class TimeComparer : IEqualityComparer<CatchHitObject>
        {
            public bool Equals(CatchHitObject x, CatchHitObject y)
            {
                return x.time == y.time;
            }

            public int GetHashCode(CatchHitObject obj)
            {
                return (obj.time).GetHashCode();
            }
        }

        public static IEnumerable<double> GetEdgeTimes(Slider sObject)
        {
            for (var i = 0; i < sObject.edgeAmount; ++i)
                yield return sObject.time + sObject.GetCurveDuration() * (i + 1);
        }

        private static IEnumerable<CatchHitObject> CreateObjectExtra(Beatmap beatmap, Slider slider, IEnumerable<double> times, string[] objectCode, NoteType type)
        {
            foreach (var time in times)
            {
                objectCode[0] = Math.Round(slider.GetPathPosition(time).X)
                    .ToString(CultureInfo.InvariantCulture);
                objectCode[2] = time.ToString(CultureInfo.InvariantCulture);
                var line = string.Join(",", objectCode);
                yield return new CatchHitObject(line.Split(','), beatmap, type);
            }
        }

        private static void CalculateDistances(List<CatchHitObject> mapObjects, Beatmap beatmap)
        {
            var allObjects = new List<CatchHitObject>();

            foreach (var currentObject in mapObjects.Where(currentObject => currentObject.type != HitObject.Type.Spinner))
            {
                allObjects.Add(currentObject);

                // If object isn't Slider, just skip it
                if (currentObject.Extras == null)
                {
                    continue;
                }

                allObjects.AddRange(currentObject.Extras);
            }

            // No need to calculate anything when the map contains less then 2 objects
            if (allObjects.Count < 2) return;

            allObjects.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            // Constant values taken from Modding Assistant as osu-lazer seems broken
            // https://github.com/rorre/decompiled-MA/blob/master/Modding%20assistant/osu/DiffCalc/BeatmapDifficultyCalculatorFruits.cs
            var circleSize = beatmap.difficultySettings.circleSize;
            var catchDifficulty = (circleSize - 5.0) / 5.0;
            var fruitWidth = (float) (64.0 * (1.0 - 0.699999988079071 * catchDifficulty)) / 128f;
            var catcherWidth = 305f * fruitWidth * 0.7f;
            var halfCatcherWidth = catcherWidth / 2;
            var lastDirection = NoteDirection.NONE;
            double dashRange = halfCatcherWidth;
            double walkRange = halfCatcherWidth / 2;

            for (var i = 0; i < allObjects.Count - 1; i++)
            {
                var currentObject = allObjects[i];
                var nextObject = allObjects[i + 1];

                var objectMetadata = GenerateObjectMetadata(currentObject, nextObject, lastDirection, dashRange, walkRange, halfCatcherWidth);
                currentObject.Target = nextObject;
                currentObject.DistanceToHyperDash = objectMetadata.DistanceToHyper;
                currentObject.DistanceToDash = objectMetadata.DistanceToDash;
                currentObject.MovementType = objectMetadata.MovementType;
                
                // Cast to an int since osu seems to be doing something similar
                currentObject.TimeToTarget = (int) (nextObject.time - currentObject.time);

                if (objectMetadata.MovementType == MovementType.HYPERDASH)
                {
                    dashRange = halfCatcherWidth;
                }
                else
                {
                    dashRange = Clamp(objectMetadata.DistanceToHyper, 0, halfCatcherWidth);

                    if (objectMetadata.MovementType == MovementType.DASH)
                    {
                        walkRange = halfCatcherWidth / 2;
                    }
                    else
                    {
                        walkRange = Clamp(objectMetadata.DistanceToDash, 0, halfCatcherWidth / 2);
                    }
                }
                
                currentObject.NoteDirection = objectMetadata.Direction;
                currentObject.IsEdgeMovement = IsEdgeMovement(beatmap, currentObject);

                lastDirection = objectMetadata.Direction;
            }
        }

        private static ObjectMetadata GenerateObjectMetadata(
            CatchHitObject current, 
            CatchHitObject next,
            NoteDirection lastDirection, 
            double dashRange, 
            double walkRange,
            double halfCatcherWidth
        ) {
            var metadata = new ObjectMetadata {
                Direction = next.X > current.X ? NoteDirection.LEFT : NoteDirection.RIGHT,
                // 1/4th of a frame of grace time, taken from osu-stable
                TimeToNext = next.time - current.time - 1000f / 60f / 4,
                DistanceInOsuCords = Math.Abs(next.X - current.X)
            };
            metadata.HyperDistanceToNext = metadata.DistanceInOsuCords - (lastDirection == metadata.Direction ? dashRange : halfCatcherWidth);
            metadata.DashDistanceToNext = metadata.DistanceInOsuCords - (lastDirection == metadata.Direction ? walkRange : halfCatcherWidth / 2);
            metadata.DistanceToHyper = (int) (metadata.TimeToNext - metadata.HyperDistanceToNext);
            metadata.DistanceToDash = (int) (metadata.TimeToNext - metadata.DashDistanceToNext - (metadata.TimeToNext * 0.3));

            // Label the type of movement based on if the distance is dashable or walkable
            if (metadata.DistanceToHyper < 0) {
                metadata.MovementType = MovementType.HYPERDASH;
            } else if (metadata.DistanceToDash < 0) {
                metadata.MovementType = MovementType.DASH;
            } else {
                metadata.MovementType = MovementType.WALK;
            }

            return metadata;
        }
        
        private static double GetBeatsPerMinute(this TimingLine timingLine)
        {
            var msPerBeatString = timingLine.code.Split(",")[1];
            var msPerBeat = double.Parse(msPerBeatString, CultureInfo.InvariantCulture);
            
            return 60000 / msPerBeat;
        }

        private static bool IsEdgeMovement(Beatmap beatmap, CatchHitObject hitObject)
        {
            var timingLine = beatmap.GetTimingLine(hitObject.time);
            var bpm = GetBeatsPerMinute(timingLine);
            var pixelsScale = (int) (180 / bpm * 10);

            switch (hitObject.MovementType)
            {
                case MovementType.WALK:
                    return hitObject.DistanceToDash <= pixelsScale;
                case MovementType.DASH:
                    return hitObject.DistanceToHyperDash <= pixelsScale;
                default:
                    return false;
            }
        }

        private static double Clamp(double n, double min, double max)
        {
            return Math.Max(Math.Min(n, max), min);
        }
    }
}
