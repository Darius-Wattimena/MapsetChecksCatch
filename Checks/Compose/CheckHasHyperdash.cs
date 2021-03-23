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
                    Hyperdashes are not allowed in Cups and Salads.
                    </br>
                    And hyperdashes can't be used on drops and/or slider repetitions in Platters."
                },
                {
                    "Reasoning",
                    @"
                    This is to ensure an easy starting experience to beginner players in Cups.
                    </br>
                    This is to ensure a manageable step in difficulty for novice players in Salads.
                    </br>
                    For Platters the accuracy and control required is unreasonable and can create a situation where the player potentially fails to read the slider path."
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
                },
                { "HyperdashSliderPart",
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
                if (catchObject.MovementType == MovementType.HYPERDASH)
                {
                    yield return new Issue(
                        GetTemplate("Hyperdash"),
                        beatmap,
                        TimestampHelper.Get(catchObject, catchObject.Target),
                        catchObject.GetNoteTypeName()
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
                }

                foreach (var catchObjectExtra in catchObject.Extras)
                {
                    if (catchObjectExtra.MovementType != MovementType.HYPERDASH) continue;
                    
                    if (catchObjectExtra.NoteType != NoteType.TAIL)
                    {
                        yield return new Issue(
                            GetTemplate("HyperdashSliderPart"),
                            beatmap,
                            TimestampHelper.Get(catchObjectExtra, catchObjectExtra.Target),
                            catchObjectExtra.GetNoteTypeName()
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }
                    
                    yield return new Issue(
                        GetTemplate("Hyperdash"),
                        beatmap,
                        TimestampHelper.Get(catchObjectExtra, catchObjectExtra.Target),
                        catchObjectExtra.GetNoteTypeName()
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
                }
            }
        }
    }
}