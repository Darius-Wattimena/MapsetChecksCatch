using System.Collections.Generic;
using System.Linq;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose.Salad
{
    [Check]
    public class CheckAntiFlowDash : BeatmapCheck
    {
        private const int AllowedBasicSnappedDashes = 2;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[S] Higher-snapped dash followed with anti-flow.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new[] { Beatmap.Difficulty.Normal },
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    TODO"
                },
                {
                    "Reason",
                    @"
                    TODO"
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "Warning",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Is a higher-snapped dash followed by anti-flow movement.",
                            "timestamp - ")
                        .WithCause("The next movement after the higher-snapped dash is not to the same direction")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            foreach (var dashObject in catchObjects.Where(catchObject => catchObject.MovementType == MovementType.DASH))
            {
                if (dashObject.Target == null) continue;
                
                // only higher-snapped objects need to be going to the same direction so we can basic-dashes.
                if (!dashObject.IsHigherSnapped(Beatmap.Difficulty.Normal)) continue;
                
                if (dashObject.Target.NoteDirection != dashObject.NoteDirection)
                {
                    yield return new Issue(
                        GetTemplate("Warning"),
                        beatmap,
                        TimestampHelper.Get(dashObject, dashObject.Target)
                    ).ForDifficulties(Beatmap.Difficulty.Normal);
                }
            }
        }
    }
}