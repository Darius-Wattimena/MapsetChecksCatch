using System.Collections.Generic;
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
    public class CheckHyperdashSlider : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Disallowed slider part hyper.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new[] { Beatmap.Difficulty.Hard, Beatmap.Difficulty.Insane },
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
                { "SliderHyperPlatter",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} {1} hyperdashes are not allowed.",
                            "timestamp - ", "object")
                        .WithCause("Hyperdash is put on a droplet or slider repeat.")
                },
                { "SliderHyperRain",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} {1} hyperdashes should not be used.",
                            "timestamp - ", "object")
                        .WithCause("Hyperdash is put on a droplet or slider repeat.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            foreach (var catchObject in catchObjects)
            {
                if (catchObject.Target == null) continue;
                
                if (catchObject.MovementType == MovementType.HYPERDASH)
                {
                    if (catchObject.NoteType == NoteType.DROPLET || catchObject.NoteType == NoteType.REPEAT)
                    {
                        yield return new Issue(
                            GetTemplate("SliderHyperPlatter"),
                            beatmap,
                            TimestampHelper.Get(catchObject, catchObject.Target),
                            catchObject.GetNoteTypeName()
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                        
                        yield return new Issue(
                            GetTemplate("SliderHyperRain"),
                            beatmap,
                            TimestampHelper.Get(catchObject, catchObject.Target),
                            catchObject.GetNoteTypeName()
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }
                }
            }
        }
    }
}