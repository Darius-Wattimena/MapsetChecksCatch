using System;
using System.Linq;
using MapsetParser.objects;

namespace MapsetChecksCatch.Helper
{
    public static class BeatmapUtil
    {
        public static string GetBeatmapIdentifier(Beatmap beatmap)
        {
            var difficulty = beatmap?.metadataSettings?.version ?? "";
            var creator = beatmap?.metadataSettings?.creator ?? "";
            var artist = beatmap?.metadataSettings?.artist ?? "";
            var title = beatmap?.metadataSettings?.title ?? "";

            return $"d={difficulty}c={creator}a={artist}t={title}";
        }

        public static float GetCurrentTriggerDistance(this CatchHitObject currentObject)
        {
            return GetTriggerDistance(currentObject) / currentObject.DistanceToHyper;
        }

        public static float GetTriggerDistance(this CatchHitObject currentObject)
        {
            if (currentObject.MovementType != MovementType.HYPERDASH) return 0f;

            var xDistance = currentObject.NoteDirection switch
            {
                NoteDirection.LEFT => currentObject.X - currentObject.Target.X,
                NoteDirection.RIGHT => currentObject.Target.X - currentObject.X,
                _ => 0f
            };

            if (xDistance > 0f) return xDistance - Math.Abs(currentObject.DistanceToHyper);

            return 0f;
        }

        /// <summary>
        ///     Check if the current object is the same snap as the other object.
        ///     There is a snap margin of 4 ms since objects can be at most 2 ms off before they are detected by MV.
        /// </summary>
        /// <param name="currentObject">The current object which is being used for the range</param>
        /// <param name="otherObject">The other object which is checked if it is in the range of the current object</param>
        /// <returns>True if the current object and the other object are the same snap</returns>
        public static bool IsSameSnap(this CatchHitObject currentObject, CatchHitObject otherObject)
        {
            const double snapMargin = 4.0;
            var snapMin = currentObject.TimeToTarget - snapMargin;
            var snapMax = currentObject.TimeToTarget + snapMargin;

            return otherObject.TimeToTarget >= snapMin && otherObject.TimeToTarget <= snapMax;
        }

        public static bool IsBasicSnapped(this CatchHitObject currentObject, Beatmap.Difficulty difficulty)
        {
            var ms = currentObject.TimeToTarget;

            return difficulty switch
            {
                Beatmap.Difficulty.Normal => ms >= 250,
                Beatmap.Difficulty.Hard => ms >= (currentObject.MovementType == MovementType.HYPERDASH ? 250 : 125),
                Beatmap.Difficulty.Insane => ms >= 125,
                _ => false
            };
        }

        /// <summary>
        ///     Check if the snapping between two objects is higher-snapped or basic-snapped
        ///     Cup: No dashes or hyperdashes are allowed
        ///     Salad: 125-249 ms dashes are higher-snapped, hyperdashes are not allowed
        ///     Platter: 62-124 ms dashes are higher-snapped, 125-249 ms hyperdashes are higher-snapped
        ///     Rain: 62-124 ms dashes/hyperdashes are higher-snapped
        ///     Overdose: No allowed distance are specified so no basic-snapped and higher-snapped exist
        /// </summary>
        /// <param name="currentObject">The current object which is getting checked</param>
        /// <param name="difficulty">The difficulty of the mapset</param>
        /// <returns>True if the origin object is higher-snapped</returns>
        public static bool IsHigherSnapped(this CatchHitObject currentObject, Beatmap.Difficulty difficulty)
        {
            var ms = currentObject.TimeToTarget;

            return difficulty switch
            {
                Beatmap.Difficulty.Normal => ms < 250 && ms >= 125,
                Beatmap.Difficulty.Hard => ms < (currentObject.MovementType == MovementType.HYPERDASH ? 250 : 125) && ms >= (currentObject.MovementType == MovementType.HYPERDASH ? 125 : 62),
                Beatmap.Difficulty.Insane => ms < 125 && ms >= 62,
                _ => false
            };
        }
    }
}