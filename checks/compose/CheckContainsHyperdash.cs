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
    [Check]
    public class CheckContainsHyperdash : BeatmapCheck
    {
        private const string ContainsHyperdash = "ContainsHyperdash";

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Hyperdash.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new [] { Beatmap.Difficulty.Easy,Beatmap.Difficulty.Normal  },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Cup and Salad difficulties should not contain any hyperdashes."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    ContainsHyperdash,
                    new IssueTemplate(Issue.Level.Problem, 
                            "{0} Is a hyperdash.",
                            "timestamp - ")
                        .WithCause("This difficulty should not contain any hyperdashes.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjectManager = new ObjectManager();

            var catchObjects = catchObjectManager.GenerateCatchObjects(beatmap);
            catchObjectManager.CalculateJumps(catchObjects, beatmap);

            foreach (var currentObject in catchObjects
                .Where(currentObject => currentObject.type != HitObject.Type.Spinner && currentObject.IsHyperDash))
            {
                yield return new Issue(
                    GetTemplate(ContainsHyperdash),
                    beatmap,
                    Timestamp.Get(currentObject.time)
                ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
            }
        }
    }
}
