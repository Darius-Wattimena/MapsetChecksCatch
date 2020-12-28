using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MapsetChecksCatch.helper
{
    public class ObjectManagerSingleton
    {
        private static readonly Lazy<ObjectManagerSingleton> LazyObjectManager = new Lazy<ObjectManagerSingleton>(() => new ObjectManagerSingleton());
        public static ObjectManagerSingleton Instance => LazyObjectManager.Value;

        private readonly ConcurrentDictionary<string, ObjectManager> _dictionary = new ConcurrentDictionary<string, ObjectManager>();

        public ObjectManager GetBeatmapObjectManager(Beatmap beatmap)
        {
            var key = beatmap.metadataSettings.beatmapSetId + ":" + beatmap.metadataSettings.beatmapId;

            if (_dictionary.TryGetValue(key, out var manager))
            {
                return manager;
            }
            else
            {
                manager = new ObjectManager(beatmap);

                _dictionary.TryAdd(key, manager);

                return manager;
            }
        }
    }

    public class ObjectManager
    {
        public readonly List<CatchHitObject> Objects;

        public ObjectManager(Beatmap beatmap)
        {
            Objects = LoadBeatmap(beatmap);
            LoadOrigins(Objects);
            CalculateJumps(Objects, beatmap);
        }

        private List<CatchHitObject> LoadBeatmap(Beatmap beatmap)
        {
            var mapObjects = beatmap.hitObjects;
            var objects = new List<CatchHitObject>();

            foreach (var mapSliderObject in mapObjects.OfType<Slider>())
            {
                var objectExtras = new List<CatchHitObject>();
                var objectCode = mapSliderObject.code.Split(',');

                var sliderObject = new CatchHitObject(objectCode, beatmap, CatchType.CIRCLE);

                // Slider ticks
                objectExtras.AddRange(CreateObjectExtra(beatmap, mapSliderObject, mapSliderObject.sliderTickTimes, objectCode, CatchType.DROPLET));

                // Slider repeats and tail
                objectExtras.AddRange(CreateObjectExtra(beatmap, mapSliderObject, GetEdgeTimes(mapSliderObject), objectCode, CatchType.CIRCLE));

                objectExtras.Sort((h1, h2) => h1.time.CompareTo(h2.time));

                sliderObject.Extras = objectExtras; 
                objects.Add(sliderObject);
            }

            objects.AddRange(
                from mapObject in mapObjects
                where mapObject is Circle
                select new CatchHitObject(mapObject.code.Split(','), beatmap, CatchType.CIRCLE)
            );

            objects.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            return objects;
        }

        private void LoadOrigins(List<CatchHitObject> mapObjects)
        {
            CatchHitObject lastObject = null;

            // Set object origin before we return everything
            foreach (var currentObject in mapObjects)
            {
                if (lastObject == null)
                {
                    lastObject = currentObject;
                    continue;
                }

                currentObject.Origin = lastObject;

                lastObject = currentObject;

                foreach (var extraHitObject in currentObject.Extras)
                {
                    extraHitObject.Origin = lastObject;
                    lastObject = extraHitObject;
                }
            }
        }

        private static IEnumerable<double> GetEdgeTimes(Slider sObject)
        {
            for (var i = 0; i < sObject.edgeAmount; ++i)
                yield return sObject.time + sObject.GetCurveDuration() * (i + 1);
        }

        private static IEnumerable<CatchHitObject> CreateObjectExtra(Beatmap beatmap, Slider slider, IEnumerable<double> times, string[] objectCode, CatchType type)
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

        private void CalculateJumps(List<CatchHitObject> mapObjects, Beatmap beatmap)
        {
            var objectWithDroplets = new List<CatchHitObject>();

            foreach (var currentObject in mapObjects.Where(currentObject => currentObject.type != HitObject.Type.Spinner))
            {
                objectWithDroplets.Add(currentObject);

                // If object isn't Slider, just skip it
                if (currentObject.Extras == null)
                {
                    continue;
                }

                objectWithDroplets.AddRange(currentObject.Extras);
            }

            if (objectWithDroplets.Count < 2) return;

            objectWithDroplets.Sort((h1, h2) => h1.time.CompareTo(h2.time));

            // Taken from Modding Assistant as osu-lazer seems broken
            // https://github.com/rorre/decompiled-MA/blob/master/Modding%20assistant/osu/DiffCalc/BeatmapDifficultyCalculatorFruits.cs
            var adjustDiff = (beatmap.difficultySettings.circleSize - 5.0) / 5.0;
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
                var distanceToHyper = timeToNext - distanceToNext;

                if (distanceToHyper < 0)
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    currentObject.HyperDashTarget = nextObject;
                    currentObject.IsWalkable = false;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    currentObject.IsWalkable = true; // TODO: this is not true since it can also be a normal jump at this point
                    lastExcess = Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }

        // https://github.com/ppy/osuTK/blob/master/src/osuTK/Math/MathHelper.cs#L303
        private static double Clamp(double n, double min, double max)
        {
            return Math.Max(Math.Min(n, max), min);
        }
    }
}