using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Framework.Objects;
using MapsetVerifier.Framework.Objects.Attributes;
using MapsetVerifier.Framework.Objects.Metadata;

namespace MapsetChecksCatch.Checks.Compose.Platter
{
    [Check]
    public class CheckHyperdashSnap : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[P] Disallowed hyperdash snap.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new[] { Beatmap.Difficulty.Hard },
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
                { "DifferentSnapHyperdashes",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} is an edge dash.",
                            "timestamp - ")
                        .WithCause("Object is almost a hyper making it an edge dash.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            foreach (var catchObject in catchObjects)
            {
                if (catchObject.Target == null) continue;
                
                if (catchObject.MovementType == MovementType.HYPERDASH && catchObject.Target.MovementType == MovementType.HYPERDASH)
                {
                    if (!catchObject.IsSameSnap(catchObject.Target))
                    {
                        yield return new Issue(
                            GetTemplate("DifferentSnapHyperdashes"),
                            beatmap,
                            TimestampHelper.Get(catchObject, catchObject.Target)
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }
                }
            }
        }
    }
}