using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System.Collections.Generic;
using System.Linq;
using MapsetParser.statics;

namespace MapsetChecksCatch.checks.compose
{
    // TODO uncomment when added walk detection
    //[Check]
    public class CheckContainsDash : BeatmapCheck
    {
        private const string ContainsDash = "ContainsDash";

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Dash.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new [] { Beatmap.Difficulty.Easy },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Cup difficulties should not contain dashes."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    ContainsDash,
                    new IssueTemplate(Issue.Level.Problem, 
                            "{0} Is a dash.",
                            "timestamp - ")
                        .WithCause("This difficulty should not contain any dashes.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjectManager = new ObjectManager();
            var catchObjects = catchObjectManager.LoadBeatmap(beatmap);

            catchObjectManager.CalculateJumps(catchObjects, beatmap);

            foreach (var currentObject in catchObjects
                .Where(currentObject => currentObject.type != HitObject.Type.Spinner && !currentObject.IsWalkable && !currentObject.IsHyperDash))
            {
                yield return new Issue(
                    GetTemplate(ContainsDash),
                    beatmap,
                    Timestamp.Get(currentObject.time)
                ).ForDifficulties(Beatmap.Difficulty.Easy);
            }
        }
    }
}
