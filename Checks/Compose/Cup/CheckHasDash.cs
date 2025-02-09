using System.Collections.Generic;
using System.Linq;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Parser.Statics;
using MapsetVerifier.Framework.Objects;
using MapsetVerifier.Framework.Objects.Attributes;
using MapsetVerifier.Framework.Objects.Metadata;

namespace MapsetChecksCatch.Checks.Compose.Cup
{
    [Check]
    public class CheckHasDash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[C] Contains dashes.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new []
            {
                Beatmap.Difficulty.Easy
            },
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Dashes are not allowed in Cups. "
                },
                {
                    "Reasoning",
                    @"
                    This is to ensure beginners have an easy starting experience."
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "Dash",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} There is a dash between the {1} and {2} {3}.",
                            "timestamp - ", "object", "target object", "Debug info")
                        .WithCause(
                            "Distance between the two objects is too high, triggering a dash distance")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            foreach (var catchObject in catchObjects)
            {
                if (catchObject.MovementType == MovementType.DASH)
                {
                    yield return new Issue(
                        GetTemplate("Dash"),
                        beatmap,
                        TimestampHelper.Get(catchObject, catchObject.Target),
                        catchObject.GetNoteTypeName(),
                        catchObject.Target.GetNoteTypeName(),
                        catchObject.GetDebugInfo()
                    ).ForDifficulties(Beatmap.Difficulty.Easy);
                }
            }
        }
    }
}