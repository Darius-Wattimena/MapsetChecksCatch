using System.Collections.Generic;
using System.Linq;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose
{
    [Check]
    public class CheckHasHyperdash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Contains hyperdashes.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Hyperdashes are not allowed in Cups and Salads. "
                },
                {
                    "Reasoning",
                    @"
                    This is to ensure an easy starting experience to beginner players in Cups.
                    </br>
                    And to ensure a manageable step in difficulty for novice players in Salads."
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "Hyperdash",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} {1} is a hyper.",
                            "timestamp - ", "object")
                        .WithCause(
                            "Distance between the two objects is too high, triggering a hyperdash distance")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            foreach (var catchObject in catchObjects)
            {
                if (catchObject.MovementType is MovementType.HYPERDASH)
                {
                    yield return new Issue(
                        GetTemplate("Hyperdash"),
                        beatmap,
                        Timestamp.Get(catchObject, catchObject.Target),
                        catchObject.GetNoteTypeName()
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
                }

                foreach (var catchObjectExtra in catchObject.Extras
                    .Where(catchObjectExtra => catchObjectExtra.MovementType is MovementType.HYPERDASH))
                {
                    yield return new Issue(
                        GetTemplate("Hyperdash"),
                        beatmap,
                        Timestamp.Get(catchObjectExtra, catchObjectExtra.Target),
                        catchObjectExtra.GetNoteTypeName()
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
                }
            }
        }
    }
}