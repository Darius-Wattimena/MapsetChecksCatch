using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MapsetChecksCatch.helper
{
    public class ObjectManager
    {
        public List<CatchHitObject> GenerateCatchObjects(Beatmap beatmap)
        {
            var mapObjects = beatmap.hitObjects;
            var objects = new List<CatchHitObject>();

            foreach (var mapSliderObject in mapObjects.OfType<Slider>())
            {
                var objectExtras = new List<CatchHitObject>();
                var objectCode = mapSliderObject.code.Split(',');

                // Slider ticks
                foreach (var tickTimes in mapSliderObject.sliderTickTimes)
                {
                    objectCode[0] = Math.Round(mapSliderObject.GetPathPosition(tickTimes).X)
                        .ToString(CultureInfo.InvariantCulture);
                    objectCode[2] = tickTimes.ToString(CultureInfo.InvariantCulture);
                    var line = string.Join(",", objectCode);
                    var node = new CatchHitObject(line.Split(','), beatmap);
                    objectExtras.Add(node);
                }

                foreach (double ticktimes in GetEdgeTimes(mapSliderObject))
                {
                    // Slider repeats and tail
                    objectCode[0] = Math.Round(mapSliderObject.GetPathPosition(ticktimes).X)
                        .ToString(CultureInfo.InvariantCulture);
                    objectCode[2] = ticktimes.ToString(CultureInfo.InvariantCulture);
                    var line = string.Join(",", objectCode);
                    var node = new CatchHitObject(line.Split(','), beatmap);
                    objectExtras.Add(node);
                }

                var sliderObject = new CatchHitObject(mapSliderObject.code.Split(','), beatmap)
                {
                    Extras = objectExtras
                };
                objects.Add(sliderObject);
            }

            objects.AddRange(
                from mapObject in mapObjects
                where !(mapObject is Slider)
                select new CatchHitObject(mapObject.code.Split(','), beatmap)
            );

            objects.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            // Set object origin before we return everything
            for (var i = 0; i < objects.Count; i++)
            {
                if (i != 0)
                {
                    objects[i].Origin = objects[i - 1];
                }
            }

            return objects;
        }

        private static IEnumerable<double> GetEdgeTimes(Slider sObject)
        {
            for (var i = 0; i < sObject.edgeAmount; ++i)
                yield return sObject.time + sObject.GetCurveDuration() * (i + 1);
        }

        public void CalculateJumps(List<CatchHitObject> mapObjects, Beatmap aBeatmap)
        {
            var objectWithDroplets = new List<CatchHitObject>();

            foreach (var currentObject in mapObjects.Where(currentObject => currentObject.GetObjectType() != "Spinner"))
            {
                objectWithDroplets.Add(currentObject);

                // If object isn't Slider, just skip it
                if (currentObject.Extras == null)
                {
                    continue;
                }

                objectWithDroplets.AddRange(currentObject.Extras);
            }

            objectWithDroplets.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            // Taken from Modding Assistant as osu-lazer seems broken
            // https://github.com/rorre/decompiled-MA/blob/master/Modding%20assistant/osu/DiffCalc/BeatmapDifficultyCalculatorFruits.cs
            var adjustDiff = (aBeatmap.difficultySettings.circleSize - 5.0) / 5.0;
            var catcherWidth = (float) (64 * (1.0 - 0.7 * adjustDiff)) / 128f;
            var num2 = 305f * catcherWidth * 0.7f;
            var halfCatcherWidth = num2 / 2;
            var lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            // https://github.com/ppy/osu/blob/master/osu.Game.Rulesets.Catch/Beatmaps/CatchBeatmapProcessor.cs#L190
            // With modifications taken from Modding Assistant
            for (var i = 0; i < objectWithDroplets.Count - 1; i++)
            {
                var currentObject = objectWithDroplets[i];
                var nextObject = objectWithDroplets[i + 1];

                var thisDirection = nextObject.X > currentObject.X ? 1 : -1;
                var timeToNext =
                    nextObject.time - currentObject.time -
                    1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                var xDistance = Math.Abs(nextObject.X - currentObject.X);
                var distanceToNext = xDistance - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                var distanceToHyper = (float) (timeToNext - distanceToNext);

                if (distanceToHyper < 0)
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    var requiredAbsolute = distanceToHyper + distanceToNext +
                                           (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                    currentObject.PixelsToHyperDash = requiredAbsolute - distanceToNext;
                    lastExcess = Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }

        // https://github.com/ppy/osuTK/blob/master/src/osuTK/Math/MathHelper.cs#L303
        public static double Clamp(double n, double min, double max)
        {
            return Math.Max(Math.Min(n, max), min);
        }
    }
}