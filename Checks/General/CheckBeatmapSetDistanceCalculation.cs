using System.Collections.Concurrent;
using System.Collections.Generic;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.General
{
    [Check]
    public class CheckBeatmapSetDistanceCalculation : GeneralCheck
    {
        private static readonly ConcurrentDictionary<string, List<CatchHitObject>> CatchBeatmaps =
            new ConcurrentDictionary<string, List<CatchHitObject>>();

        public static List<CatchHitObject> GetBeatmapDistances(Beatmap beatmap)
        {
            var identifier = BeatmapUtil.GetBeatmapIdentifier(beatmap);
            CatchBeatmaps.TryGetValue(identifier, out var catchObjects);

            return catchObjects ?? new List<CatchHitObject>();
        }

        public override CheckMetadata GetMetadata()
        {
            return new CheckMetadata
            {
                Category = "Resources",
                Message = "Calculating dash and hyperdash distances.",
                Author = "Greaper",

                Documentation = new Dictionary<string, string>
                {
                    {
                        "Purpose",
                        @"
                    Calculate all the dash and hyperdash distances of this beatmap and cache it so we can use it for distance specific checks."
                    },
                    {
                        "Reasoning",
                        @"
                    Calculating the beatmap distances can be a heavy process and should only be done once to avoid long loading times with slower computers."
                    }
                }
            };
        }

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>();
        }

        public override IEnumerable<Issue> GetIssues(BeatmapSet beatmapSet)
        {
            beatmapSet.beatmaps.ForEach(beatmap =>
            {
                var calculatedBeatmap = BeatmapDistanceCalculator.Calculate(beatmap);
                var beatmapIdentifier = BeatmapUtil.GetBeatmapIdentifier(beatmap);

                CatchBeatmaps.TryRemove(beatmapIdentifier, out _);
                CatchBeatmaps.TryAdd(beatmapIdentifier, calculatedBeatmap);
            });

            yield break;
        }
    }
}