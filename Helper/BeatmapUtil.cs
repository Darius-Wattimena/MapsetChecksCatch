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
            
            return $"difficulty={difficulty}creator={creator}artist={artist}title={title}";
        }

        /// <summary>
        /// Check if the current object is the same snap as the other object.
        /// There is a snap margin of 4 ms since objects can be at most 2 ms off before they are detected by MV.
        /// </summary>
        /// <param name="currentObject">The current object which is being used for the range</param>
        /// <param name="otherObject">The other object which is checked if it is in the range of the current object</param>
        /// <returns>True if the current object and the other object are the same snap</returns>
        public static bool IsSameSnap(this CatchHitObject currentObject, CatchHitObject otherObject)
        {
            const int snapMargin = 4;
            var range = Enumerable.Range(
                currentObject.TimeToTarget - snapMargin, 
                currentObject.TimeToTarget + snapMargin);

            return range.Contains(otherObject.TimeToTarget);
        }

        /// <summary>
        /// Extension function for easy access to the higher-snapped check.
        /// </summary>
        /// <param name="currentObject">The current object which is getting checked</param>
        /// <param name="difficulty">The difficulty of the mapset</param>
        /// <returns>True if the current object is higher-snapped</returns>
        public static bool IsHigherSnapped(this CatchHitObject currentObject, Beatmap.Difficulty difficulty)
        {
            return IsHigherSnapped(difficulty, currentObject.Target, currentObject);
        }
        
        /// <summary>
        /// Check if the snapping between two objects is higher-snapped or basic-snapped
        ///
        /// Cup: No dashes or hyperdashes are allowed
        /// Salad: 125-249 ms dashes are higher-snapped, hyperdashes are not allowed
        /// Platter: 62-124 ms dashes are higher-snapped, 125-249 ms hyperdashes are higher-snapped
        /// Rain: 62-124 ms dashes/hyperdashes are higher-snapped
        /// Overdose: No allowed distance are specified so no basic-snapped and higher-snapped exist
        /// 
        /// </summary>
        /// <param name="difficulty">The difficulty of the mapset</param>
        /// <param name="targetObject">The object that is the target of the movement</param>
        /// <param name="originObject">The object that is the starting point of the movement</param>
        /// <returns>True if the origin object is higher-snapped</returns>
        public static bool IsHigherSnapped(Beatmap.Difficulty difficulty, CatchHitObject targetObject, CatchHitObject originObject)
        {
            var ms = targetObject.time - originObject.time;

            return difficulty switch
            {
                Beatmap.Difficulty.Normal => (ms < 250),
                Beatmap.Difficulty.Hard => (ms < (originObject.MovementType == MovementType.HYPERDASH ? 250 : 125)),
                Beatmap.Difficulty.Insane => (ms < (125)),
                _ => false
            };
        }
    }
}