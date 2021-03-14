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
    }
}