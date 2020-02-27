using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System.Collections.Generic;
using System.Linq;

namespace MapsetChecksCatch.checks.compose
{
    [Check]
    public class CheckHasSpinner : BeatmapCheck
    {
        private const string HAS_SPINNER = "HasSpinner";

        private const int THREASHOLD_SPINNER = 1;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Missing spinner.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>()
                {
                    {
                        "Purpose",
                        @"
                        Every difficulty should have a spinner."
                    },
                    {
                        "Reasoning",
                        @"
                        Try to have at least one spinner in each difficulty to create variety in the map and fluctuation among scores. 
                        However, if a spinner just doesn't fit anywhere in the song, then there's no need to force one."
                    }
                }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                {
                    HAS_SPINNER,
                    new IssueTemplate(Issue.Level.Minor, "The difficulty has no spinner.")
                    .WithCause("Their is no spinner present.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            if (beatmap.hitObjects.Count(x => x is Spinner) < THREASHOLD_SPINNER)
            {
                yield return new Issue(GetTemplate(HAS_SPINNER), beatmap);
            }
        }
    }
}
