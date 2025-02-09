using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Parser.Objects.HitObjects;

namespace MapsetChecksCatch.Helper
{
    public static class BeatmapDistanceCalculator
    {
        /// <summary>
        /// The width of the catcher which can receive fruit. Equivalent to "catchMargin" in osu-stable.
        /// </summary>
        private const float ALLOWED_CATCH_RANGE = 0.8f;

        /// <summary>
        /// The size of the catcher at 1x scale.
        /// </summary>
        private const float BASE_SIZE = 106.75f;

        /// <summary>
        /// The speed of the catcher when the catcher is dashing.
        /// </summary>
        private const double BASE_DASH_SPEED = 1.0;

        /// <summary>
        /// The speed of the catcher when the catcher is not dashing.
        /// </summary>
        private const double BASE_WALK_SPEED = 0.5;

        // After a hyperdash we want to be more lenient with what the dash distance as player commonly overshoot
        private const double HyperdashLeniency = 0.95;

        private static float CalculateCatchWidth(float circleSize) => BASE_SIZE * Math.Abs(CalculateScale(circleSize).X) * ALLOWED_CATCH_RANGE;

        private static float CalculateScaleFromCircleSize(float circleSize)
        {
            return (float)(1.0f - 0.7f * DifficultyRange(circleSize)) / 2 * 1;
        }
        
        /// <summary>
        /// Calculates the scale of the catcher based off the provided beatmap difficulty.
        /// </summary>
        private static Vector2 CalculateScale(float circleSize) => new Vector2(CalculateScaleFromCircleSize(circleSize) * 2);
        
        static double DifficultyRange(double difficulty) => (difficulty - 5) / 5;
        
        public static List<CatchHitObject> Calculate(Beatmap beatmap)
        {
            var hitObjects = GenerateCatchHitObjects(beatmap);
            CalculateDistances(hitObjects, beatmap);
            return hitObjects;
        }

        private static List<CatchHitObject> GenerateCatchHitObjects(Beatmap beatmap)
        {
            var mapObjects = beatmap.HitObjects;
            var objects = new List<CatchHitObject>();

            // Get all the catch objects from the spinners
            foreach (var mapSliderObject in mapObjects.OfType<Slider>())
            {
                var objectExtras = new List<CatchHitObject>();
                var objectCode = mapSliderObject.code.Split(',');

                // The first object of a slider is always its head
                var sliderObject = new CatchHitObject(objectCode, beatmap, NoteType.HEAD, mapSliderObject);
                
                var edgeTimes = GetEdgeTimes(mapSliderObject).ToList();
                
                for (var i = 0; i < edgeTimes.Count; i++)
                {
                    objectExtras.Add(i + 1 == edgeTimes.Count
                        ? CreateObjectExtra(beatmap, sliderObject, mapSliderObject, edgeTimes[i], objectCode, NoteType.TAIL)
                        : CreateObjectExtra(beatmap, sliderObject, mapSliderObject, edgeTimes[i], objectCode, NoteType.REPEAT));
                }

                // TODO doesn't seem to work?
                var actualSliderTicks = mapSliderObject.SliderTickTimes.Where(tickTime =>
                    objectExtras.All(extra => !IsSimilarTime(tickTime, extra.time)));

                objectExtras.AddRange(mapSliderObject.SliderTickTimes.Select(sliderTick =>
                    CreateObjectExtra(beatmap, sliderObject, mapSliderObject, sliderTick, objectCode, NoteType.DROPLET)));

                objects.Add(sliderObject);
                objects.AddRange(objectExtras);
            }

            // Add all circles
            objects.AddRange(
                from mapObject in mapObjects
                where mapObject is Circle
                select new CatchHitObject(mapObject.code.Split(','), beatmap, NoteType.CIRCLE, mapObject)
            );
            
            // Add all spinners so we can ignore then when calculating dashes or hyperdashes
            objects.AddRange(
                from mapObject in mapObjects
                where mapObject is Spinner
                select new CatchHitObject(mapObject.code.Split(','), beatmap, NoteType.SPINNER, mapObject)
            );
            objects.Sort((h1, h2) => h1.time.CompareTo(h2.time));
            return objects;
        }

        private static bool IsSimilarTime(double thisTime, double otherTime)
        {
            return thisTime - 4.0 < otherTime || thisTime + 4.0 > otherTime;
        }

        // Get all edge times except of the slider head
        public static IEnumerable<double> GetEdgeTimes(Slider sObject)
        {
            for (var i = 0; i < sObject.EdgeAmount; ++i)
                yield return sObject.time + sObject.GetCurveDuration() * (i + 1);
        }

        private static CatchHitObject CreateObjectExtra(Beatmap beatmap, CatchHitObject catchHitObject,
            Slider slider, double time, string[] objectCode, NoteType type)
        {
            var objectCodeCopy = (string[]) objectCode.Clone();
            objectCodeCopy[0] = slider.GetPathPosition(time).X.ToString(CultureInfo.InvariantCulture);
            objectCodeCopy[2] = time.ToString(CultureInfo.InvariantCulture);
            var line = string.Join(",", objectCodeCopy);
            var catchSliderHitObject = new CatchHitObject(line.Split(','), beatmap, type, slider)
            {
                SliderHead = catchHitObject
            };
            return catchSliderHitObject;
        }

        private static void CalculateDistances(List<CatchHitObject> allObjects, Beatmap beatmap)
        {
            // No need to calculate anything when the map contains less then 2 objects
            if (allObjects.Count < 2) return;

            allObjects.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            // Using the way how lazer calculates hyperdashes as it seems to be in line with stable
            float catcherWidth = CalculateCatchWidth(beatmap.DifficultySettings.circleSize);
            float halfCatcherWidth = catcherWidth * 0.5f;

            halfCatcherWidth /= ALLOWED_CATCH_RANGE;
            double baseWalkRange = halfCatcherWidth * 0.95;

            var lastDirection = NoteDirection.NONE;
            double dashRange = halfCatcherWidth;

            // TODO Current walk range is referring to super strong edge walk, need to figure out to allow tap dashes for Cup/Salad rules
            double walkRange = baseWalkRange;

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
                    
                    // Reset everything when we have a spinner, ignore spinner hyperdashes
                    dashRange = halfCatcherWidth;
                    walkRange = baseWalkRange;
                    lastDirection = NoteDirection.NONE;
                    lastWasHyper = false;
                }
                else
                {
                    var objectMetadata = GenerateObjectMetadata(
                        currentObject, nextObject, lastDirection, dashRange, walkRange, 
                        halfCatcherWidth, baseWalkRange, lastWasHyper
                    );
                    currentObject.DistanceToHyper = objectMetadata.DistanceToHyper;
                    currentObject.DistanceToDash = objectMetadata.DistanceToDash;
                    currentObject.MovementType = objectMetadata.MovementType;
                
                    currentObject.TimeToTarget = objectMetadata.TimeToNext;

                    if (objectMetadata.MovementType == MovementType.HYPERDASH)
                    {
                        dashRange = halfCatcherWidth;
                    }
                    else
                    {
                        dashRange = Math.Clamp(objectMetadata.DistanceToHyper, 0, halfCatcherWidth);
                    }

                    if (objectMetadata.MovementType == MovementType.DASH)
                    {
                        walkRange = baseWalkRange;
                    }
                    else
                    {
                        walkRange = Math.Clamp(objectMetadata.DistanceToDash, 0, baseWalkRange);
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
            bool lastWasHyper
        ) {
            var metadata = new ObjectMetadata {
                Direction = next.X > current.X ? NoteDirection.LEFT : NoteDirection.RIGHT,
                // 1/4th of a frame of grace time, taken from osu-stable
                TimeToNext = (int) next.time - (int) current.time - 1000f / 60f / 4,
                DistanceInOsuCords = Math.Abs(next.X - current.X),
            };

            double actualWalkRange = lastDirection == metadata.Direction ? walkRange : baseWalkRange;

            if (lastWasHyper)
            {
                actualWalkRange *= HyperdashLeniency;
            }
    
            double dashDistanceToNext = metadata.DistanceInOsuCords - (lastDirection == metadata.Direction ? dashRange : halfCatcherWidth);
            metadata.DistanceToHyper = (float)(metadata.TimeToNext * BASE_DASH_SPEED - dashDistanceToNext);
            
            double walkDistanceToNext = metadata.DistanceInOsuCords - actualWalkRange;
            metadata.DistanceToDash = (float)(metadata.TimeToNext * BASE_WALK_SPEED - walkDistanceToNext);

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
        
        public static double GetBeatsPerMinute(this TimingLine timingLine)
        {
            var msPerBeatString = timingLine.Code.Split(",")[1];
            var msPerBeat = double.Parse(msPerBeatString, CultureInfo.InvariantCulture);
            
            return 60000 / msPerBeat;
        }

        public static double GetBpm(this Beatmap beatmap, CatchHitObject hitObject)
        {
            var timingLine = beatmap.GetTimingLine(hitObject.time);
            var bpm = GetBeatsPerMinute(timingLine);

            return bpm;
        }

        public static double GetBpmScale(this Beatmap beatmap, CatchHitObject hitObject)
        {
            return 180 / beatmap.GetBpm(hitObject);
        }

        private static bool IsEdgeMovement(Beatmap beatmap, CatchHitObject hitObject)
        {
            switch (hitObject.MovementType)
            {
                case MovementType.WALK:
                    // 1,44 * bpm
                    double comfyDash = 1.44 * beatmap.GetBpm(hitObject);
                    float xDistance = Math.Abs(hitObject.X - hitObject.Target.X);
                    return xDistance > comfyDash * beatmap.GetBpmScale(hitObject);
                case MovementType.DASH:
                    double pixelScale = 10.0 * beatmap.GetBpmScale(hitObject);
                    return hitObject.DistanceToHyper < pixelScale;
                case MovementType.HYPERDASH:
                default:
                    return false;
            }
        }
    }
}
