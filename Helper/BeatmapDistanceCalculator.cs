using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using MapsetParser.objects;
using MapsetParser.objects.hitobjects;

namespace MapsetChecksCatch.Helper
{
    public static class BeatmapDistanceCalculator
    {
        /// <summary>
        /// The width of the catcher which can receive fruit. Equivalent to "catchMargin" in osu-stable.
        /// </summary>
        public const float ALLOWED_CATCH_RANGE = 0.8f;

        /// <summary>
        /// The size of the catcher at 1x scale.
        /// </summary>
        public const float BASE_SIZE = 106.75f;

        /// <summary>
        /// The relative space to cover in 1 millisecond. based on 1 game pixel per millisecond as in osu-stable.
        /// </summary>
        public const double BASE_SPEED = 1.0;

        /// <summary>
        /// Calculates the scale of the catcher based off the provided beatmap difficulty.
        /// </summary>
        private static Vector2 calculateScale(float circleSize) => new Vector2(1.0f - 0.7f * (circleSize - 5) / 5);

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="scale">The scale of the catcher.</param>
        public static float CalculateCatchWidth(Vector2 scale) => BASE_SIZE * Math.Abs(scale.X) * ALLOWED_CATCH_RANGE;

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="difficulty">The beatmap difficulty.</param>
        public static float CalculateCatchWidth(float circleSize) => CalculateCatchWidth(calculateScale(circleSize));

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
                    objectExtras.AddRange(CreateObjectExtra(beatmap, sliderObject, mapSliderObject, edgeTimes, objectCode, NoteType.TAIL));
                }
                else
                {
                    // We have a repeat so the slider end is the last object
                    var lastObjectArray = new[] { edgeTimes.Last() };
                    
                    objectExtras.AddRange(CreateObjectExtra(beatmap, sliderObject, mapSliderObject, lastObjectArray, objectCode, NoteType.TAIL));

                    // Remove the last object from the edgeTimes so we only have repeats
                    edgeTimes.RemoveAt(edgeTimes.Count - 1);

                    objectExtras.AddRange(CreateObjectExtra(beatmap, sliderObject, mapSliderObject, edgeTimes, objectCode, NoteType.REPEAT));
                }

                foreach (var sliderTick in mapSliderObject.sliderTickTimes.Where(tickTime => !objectExtras.Any(extra => IsSimilarTime(tickTime, extra.time))))
                {
                    // Only add a slider tick to the objects extra if not present yet
                    if (objectExtras.Any(x => x.time.Equals(sliderTick)))
                    {
                        continue;
                    }

                    objectExtras.AddRange(CreateObjectExtra(beatmap, sliderObject, mapSliderObject, new [] { sliderTick }, objectCode, NoteType.DROPLET));
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
            
            // Add all spinners so we can ignore then when calculating dashes or hyperdashes
            objects.AddRange(
                from mapObject in mapObjects
                where mapObject is Spinner
                select new CatchHitObject(mapObject.code.Split(','), beatmap, NoteType.SPINNER)
            );
            
            objects.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            return new HashSet<CatchHitObject>(objects, new TimeComparer()).ToList();
        }

        private static bool IsSimilarTime(double thisTime, double otherTime)
        {
            var range = Enumerable.Range(
                (int) (thisTime - 4.0), 
                (int) (thisTime + 4.0));

            return range.Contains((int) otherTime);
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

        // Get all edge times except of the slider head
        public static IEnumerable<double> GetEdgeTimes(Slider sObject)
        {
            for (var i = 0; i < sObject.edgeAmount; ++i)
                yield return sObject.time + sObject.GetCurveDuration() * (i + 1);
        }

        private static IEnumerable<CatchHitObject> CreateObjectExtra(Beatmap beatmap, CatchHitObject catchHitObject,
            Slider slider, IEnumerable<double> times, string[] objectCode, NoteType type)
        {
            foreach (var time in times)
            {
                objectCode[0] = Math.Round(slider.GetPathPosition(time).X)
                    .ToString(CultureInfo.InvariantCulture);
                objectCode[2] = time.ToString(CultureInfo.InvariantCulture);
                var line = string.Join(",", objectCode);
                var catchSliderHitObject = new CatchHitObject(line.Split(','), beatmap, type);
                catchSliderHitObject.SliderHead = catchHitObject;
                yield return catchSliderHitObject;
            }
        }

        private static void CalculateDistances(List<CatchHitObject> mapObjects, Beatmap beatmap)
        {
            var allObjects = new List<CatchHitObject>();

            foreach (var currentObject in mapObjects)
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

            // Using the way how lazer calculates hyperdashes as it seems to be in line with stable
            double halfCatcherWidth = CalculateCatchWidth(beatmap.difficultySettings.circleSize) / 2;

            halfCatcherWidth /= ALLOWED_CATCH_RANGE;
            double baseWalkRange = halfCatcherWidth / 3;

            var lastDirection = NoteDirection.NONE;
            double dashRange = halfCatcherWidth;

            // TODO Current walk range is referring to super strong edge walk, need to figure out to allow tap dashes for Cup/Salad rules
            double walkRange = baseWalkRange;

            // After a hyperdash we want to be more lenient with what the dash distance as player commonly overshoot
            double hyperdashLeniency = 0.9;

            var lastWasHyper = false;

            for (var i = 0; i < allObjects.Count - 1; i++)
            {
                var currentObject = allObjects[i];
                var nextObject = allObjects[i + 1];
                currentObject.Target = nextObject;

                if (currentObject.NoteType == NoteType.SPINNER || nextObject.NoteType == NoteType.SPINNER)
                {
                    // TODO This might not be true, a hyper can be created on the previous note if the spinner end is not catchable
                    currentObject.MovementType = MovementType.WALK;
                    
                    // For now reset everything when we have a spinner, ignore spinner hyperdashes
                    dashRange = halfCatcherWidth;
                    walkRange = baseWalkRange;
                    lastDirection = NoteDirection.NONE;
                    lastWasHyper = false;
                }
                else
                {
                    var objectMetadata = GenerateObjectMetadata(
                        currentObject, nextObject, lastDirection, dashRange, walkRange, 
                        halfCatcherWidth, baseWalkRange, hyperdashLeniency, lastWasHyper
                    );
                    currentObject.DistanceToHyper = objectMetadata.DistanceToHyper;
                    currentObject.DistanceToDash = objectMetadata.DistanceToDash;
                    currentObject.MovementType = objectMetadata.MovementType;
                
                    // Cast to an int since osu seems to be doing something similar
                    currentObject.TimeToTarget = (int) objectMetadata.TimeToNext;

                    if (objectMetadata.MovementType == MovementType.HYPERDASH)
                    {
                        dashRange = halfCatcherWidth;
                    }
                    else
                    {
                        dashRange = Clamp(objectMetadata.DistanceToHyper, 0, halfCatcherWidth);
                    }

                    if (objectMetadata.MovementType == MovementType.DASH)
                    {
                        walkRange = baseWalkRange;
                    }
                    else
                    {
                        walkRange = Clamp(objectMetadata.DistanceToDash, 0, baseWalkRange);
                    }
                
                    currentObject.NoteDirection = objectMetadata.Direction;
                    currentObject.IsEdgeMovement = IsEdgeMovement(beatmap, currentObject);

                    lastDirection = objectMetadata.Direction;

                    lastWasHyper = objectMetadata.MovementType == MovementType.HYPERDASH;
                }
            }
        }

        private static ObjectMetadata GenerateObjectMetadata(
            CatchHitObject current, 
            CatchHitObject next,
            NoteDirection lastDirection, 
            double dashRange, 
            double walkRange,
            double halfCatcherWidth,
            double baseWalkRange,
            double hyperdashLeniency,
            bool lastWasHyper
        ) {
            var metadata = new ObjectMetadata {
                Direction = next.X > current.X ? NoteDirection.LEFT : NoteDirection.RIGHT,
                // 1/4th of a frame of grace time, taken from osu-stable
                TimeToNext = next.time - current.time - 1000f / 60f / 4,
                DistanceInOsuCords = Math.Abs(next.X - current.X)
            };

            double actualWalkRange;

            if (lastWasHyper)
            {
                actualWalkRange = (lastDirection == metadata.Direction ? walkRange : baseWalkRange) *
                                  hyperdashLeniency;
            }
            else
            {
                actualWalkRange = lastDirection == metadata.Direction ? walkRange : baseWalkRange;
            }

            metadata.HyperDistanceToNext = metadata.DistanceInOsuCords - (lastDirection != NoteDirection.NONE || lastDirection == metadata.Direction ? dashRange : halfCatcherWidth);
            metadata.DashDistanceToNext = metadata.DistanceInOsuCords - (lastDirection != NoteDirection.NONE ? actualWalkRange : baseWalkRange);
            metadata.DistanceToHyper = (int) (metadata.TimeToNext - metadata.HyperDistanceToNext);
            metadata.DistanceToDash = (int) (metadata.TimeToNext - metadata.DashDistanceToNext);

            // Label the type of movement based on if the distance is dashable or walkable
            if (metadata.DistanceToHyper <= 0) {
                metadata.MovementType = MovementType.HYPERDASH;
            } else if (metadata.DistanceToDash <= 0) {
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
                    return hitObject.DistanceToHyper <= pixelsScale;
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
