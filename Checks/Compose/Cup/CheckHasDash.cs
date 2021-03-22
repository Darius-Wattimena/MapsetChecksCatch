using System.Collections.Generic;
using System.Linq;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose.Cup
{
    [Check]
    public class CheckHasDash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Contains dashes.",
            Modes = new[] { Beatmap.Mode.Catch },
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
                    This is to ensure an easy starting experience to beginner players."
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "Dash",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} {1} is a dash.",
                            "timestamp - ", "object")
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
                if (catchObject.MovementType is MovementType.DASH)
                {
                    yield return new Issue(
                        GetTemplate("Dash"),
                        beatmap,
                        Timestamp.Get(catchObject, catchObject.Target),
                        catchObject.GetNoteTypeName()
                    ).ForDifficulties(Beatmap.Difficulty.Easy);
                }

                foreach (var catchObjectExtra in catchObject.Extras
                    .Where(catchObjectExtra => catchObjectExtra.MovementType is MovementType.DASH))
                {
                    yield return new Issue(
                        GetTemplate("Dash"),
                        beatmap,
                        Timestamp.Get(catchObjectExtra, catchObjectExtra.Target),
                        catchObjectExtra.GetNoteTypeName()
                    ).ForDifficulties(Beatmap.Difficulty.Easy);
                }
            }
        }
    }
}