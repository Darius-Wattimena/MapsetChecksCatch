using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System.Collections.Generic;
using MapsetChecksCatch.helper;

namespace MapsetChecksCatch.checks.compose
{
    [Check]
    public class CheckSpinnerGap : BeatmapCheck
    {
        private const string SpinnerAfter = "SpinnerAfter";
        private const string SpinnerBefore = "SpinnerBefore";

        private const int ThresholdBeforeEN = (int) AllowedSpinnerGapStart.DIFFS_EN;
        private const int ThresholdBeforeHI = (int) AllowedSpinnerGapStart.DIFFS_HI;
        private const int ThresholdBeforeX = (int) AllowedSpinnerGapStart.DIFF_X;
        private const int ThresholdAfterENH = (int) AllowedSpinnerGapEnd.DIFFS_ENH;
        private const int ThresholdAfterIX = (int) AllowedSpinnerGapEnd.DIFFS_IX;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Spinner gap.",
            Modes = new[] {Beatmap.Mode.Catch},
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    There must be a gap between the start and end of a spinner to ensure readability."
                },
                {
                    "Reasoning",
                    @"
                    The start and end of a spinner can make it hard to read the next objects. 
                    On lower diffs a bigger gap is used to make it easier for the player to react on the new objects and to not fall in a spinner trap."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    SpinnerBefore,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} The time between the spinner and the previous active object must be at least {1} ms, currently {2} ms.",
                            "timestamp - ", "guideline duration", "current duration")
                        .WithCause("The spinner starts to early.")
                },
                {
                    SpinnerAfter,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} The time between the spinner and the next active object must be at least {1} ms, currently {2} ms.",
                            "timestamp - ", "guideline duration", "current duration")
                        .WithCause("The spinner ends to early.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            for (var i = 0; i < beatmap.hitObjects.Count; i++)
            {
                var hitObject = beatmap.hitObjects[i];

                //Ignore all objects that aren't spinners
                if (!(hitObject is Spinner)) continue;

                //Check if we have any objects before the spinner
                if (i - 1 >= 0)
                {
                    var timeBetweenBefore = hitObject.time - beatmap.hitObjects[i - 1].time;

                    if (timeBetweenBefore < ThresholdBeforeEN)
                    {
                        yield return new Issue(
                            GetTemplate(SpinnerBefore),
                            beatmap,
                            Timestamp.Get(hitObject),
                            ThresholdBeforeEN,
                            timeBetweenBefore
                        ).ForDifficulties(
                            Beatmap.Difficulty.Easy,
                            Beatmap.Difficulty.Normal
                        );
                    }

                    if (timeBetweenBefore < ThresholdBeforeHI)
                    {
                        yield return new Issue(
                            GetTemplate(SpinnerBefore),
                            beatmap,
                            Timestamp.Get(hitObject),
                            ThresholdBeforeHI,
                            timeBetweenBefore
                        ).ForDifficulties(
                            Beatmap.Difficulty.Hard,
                            Beatmap.Difficulty.Insane
                        );
                    }

                    if (timeBetweenBefore < ThresholdBeforeX)
                    {
                        yield return new Issue(
                            GetTemplate(SpinnerBefore),
                            beatmap,
                            Timestamp.Get(hitObject),
                            ThresholdBeforeX,
                            timeBetweenBefore
                        ).ForDifficulties(
                            Beatmap.Difficulty.Expert,
                            Beatmap.Difficulty.Ultra
                        );
                    }
                }

                //Break if we are on the end of the map
                if (i + 1 >= beatmap.hitObjects.Count) break;

                var timeBetweenAfter = beatmap.hitObjects[i + 1].time - hitObject.GetEndTime();

                if (timeBetweenAfter < ThresholdAfterENH)
                {
                    yield return new Issue(
                        GetTemplate(SpinnerAfter),
                        beatmap,
                        Timestamp.Get(hitObject),
                        ThresholdAfterENH,
                        timeBetweenAfter
                    ).ForDifficulties(
                        Beatmap.Difficulty.Easy,
                        Beatmap.Difficulty.Normal,
                        Beatmap.Difficulty.Hard
                    );
                }

                if (timeBetweenAfter < ThresholdAfterIX)
                {
                    yield return new Issue(
                        GetTemplate(SpinnerAfter),
                        beatmap,
                        Timestamp.Get(hitObject),
                        ThresholdAfterIX,
                        timeBetweenAfter
                    ).ForDifficulties(
                        Beatmap.Difficulty.Insane,
                        Beatmap.Difficulty.Expert,
                        Beatmap.Difficulty.Ultra
                    );
                }
            }
        }
    }
}