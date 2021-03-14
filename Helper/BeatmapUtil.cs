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
        /// Check if the snapping between two objects is higher-snapped or basic-snapped
        ///
        /// Cup: No dashes or hyperdashes are allowed
        /// Salad: 125-249 ms dashes are higher-snapped, hyperdashes are not allowed
        /// Platter: 62-124 ms dashes are higher-snapped, 125-249 ms hyperdashes are higher-snapped
        /// Rain: 62-124 ms dashes/hyperdashes are higher-snapped
        /// Overdose: No allowed distance are specified so no basic-snapped and higher-snapped exist
        /// 
        /// </summary>
        /// <param name="difficulty">The guessed difficulty of the mapset</param>
        /// <param name="currentObject">The object that is currently checked</param>
        /// <param name="lastObject">The object before the current one</param>
        /// <returns>True if the current object is higher-snapped</returns>
        public static bool IsHigherSnapped(Beatmap.Difficulty difficulty, CatchHitObject currentObject, CatchHitObject lastObject)
        {
            var ms = currentObject.time - lastObject.time;

            return difficulty switch
            {
                Beatmap.Difficulty.Normal => (ms < 250),
                Beatmap.Difficulty.Hard => (ms < (lastObject.MovementType == MovementType.HYPERDASH ? 250 : 125)),
                Beatmap.Difficulty.Insane => (ms < (125)),
                _ => false
            };
        }
    }
}