using System.Collections.Generic;
using System.Linq;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Framework.Objects;
using MapsetVerifier.Framework.Objects.Attributes;
using MapsetVerifier.Framework.Objects.Metadata;

namespace MapsetChecksCatch.Checks.Compose
{
    [Check]
    public class CheckHasEdgeWalk : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Too strong walks.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Too strong walks are quite harsh and are most of the time unintentionally placed on a lower difficulty."
                },
                {
                    "Reasoning",
                    @"
                    Too strong walks require fast reaction speed, newer players don't have this and will instead tap dash this distance."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "EdgeWalk",
                    new IssueTemplate(Issue.Level.Minor,
                            "{0} This object is a harsh walk and might be seen as ambiguous, consider reducing it.",
                            "timestamp - ")
                        .WithCause(
                            "A too strong walk is provided")
                }
            };
        }

        private static Issue EdgeWalkIssue(IssueTemplate template, Beatmap beatmap, CatchHitObject currentObject, params Beatmap.Difficulty[] difficulties)
        {
            return new Issue(
                template,
                beatmap,
                TimestampHelper.Get(currentObject, currentObject.Target)
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            var issueObjects = new List<CatchHitObject>();

            foreach (var currentObject in catchObjects
                .Where(currentObject => currentObject.type != HitObject.Types.Spinner && currentObject.MovementType == MovementType.WALK))
            {
                var dashDistance = currentObject.DistanceToDash;

                if (dashDistance <= 5)
                {
                    issueObjects.Add(currentObject);
                }
            }

            foreach (var issueObject in issueObjects)
            {
                if (issueObject.IsEdgeMovement)
                {
                    yield return EdgeWalkIssue(GetTemplate("EdgeWalk"), beatmap, issueObject,
                        Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);
                }
            }
        }
    }
}
