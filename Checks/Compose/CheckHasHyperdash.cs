using System;
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
            Difficulties = new [] { Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard },
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
                            "{0} {1} is an slider droplet/head/repeat hyper.",
                            "timestamp - ", "object")
                        .WithCause(
                            "Distance between the two objects is too high, triggering a hyperdash distance")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            foreach (var catchObject in catchObjects.Where(catchObject =>
                catchObject.MovementType == MovementType.HYPERDASH))
            {
                yield return new Issue(
                    GetTemplate("Hyperdash"),
                    beatmap,
                    TimestampHelper.Get(catchObject, catchObject.Target),
                    catchObject.GetNoteTypeName()
                ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);

                switch (catchObject.NoteType)
                {
                    case NoteType.HEAD:
                    case NoteType.REPEAT:
                    case NoteType.DROPLET:
                        yield return new Issue(
                            GetTemplate("HyperdashSliderPart"),
                            beatmap,
                            TimestampHelper.Get(catchObject, catchObject.Target),
                            catchObject.GetNoteTypeName()
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                        break;
                }
            }
        }
    }
}