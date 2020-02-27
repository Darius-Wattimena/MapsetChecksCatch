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
        private const string SPINNER_AFTER = "SpinnerAfter";
        private const string SPINNER_BEFORE = "SpinnerBefore";

        private const int THREASHOLD_AFTER_ENH = (int) AllowedSpinnerGapEnd.DIFFS_ENH;
        private const int THREASHOLD_AFTER_IX = (int) AllowedSpinnerGapEnd.DIFFS_IX;

        private const int THREASHOLD_BEFORE_EN = (int) AllowedSpinnerGapStart.DIFFS_EN;
        private const int THREASHOLD_BEFORE_HI = (int) AllowedSpinnerGapStart.DIFFS_HI;
        private const int THREASHOLD_BEFORE_X = (int) AllowedSpinnerGapStart.DIFF_X;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Spinner gap.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>()
                {
                    {
                        "Purpose",
                        @"
                        To ensure readability."
                    }
                }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                {
                    SPINNER_BEFORE,
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} The time between the spinner and the previous active object must be at least {1} ms, currently {2} ms.",
                        "timestamp - ", "guideline duration", "current duration")
                    .WithCause("The spinner starts to early.")
                },
                {
                    SPINNER_AFTER,
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

                if (hitObject is Spinner)
                {
                    var timeBetweenBefore = -1.0;
                    var timeBetweenAfter = -1.0;

                    //Check if we have any objects before the spinner
                    if (i - 1 >= 0)
                    {
                        timeBetweenBefore = hitObject.time - beatmap.hitObjects[i - 1].time;
                    }

                    //Check if we have any objects after the spinner
                    if (i + 1 < beatmap.hitObjects.Count)
                    {
                        timeBetweenAfter = beatmap.hitObjects[i + 1].time - hitObject.GetEndTime();
                    }

                    if (timeBetweenAfter < THREASHOLD_AFTER_ENH && timeBetweenAfter != -1.0)
                    {
                        yield return new Issue(
                            GetTemplate(SPINNER_AFTER),
                            beatmap,
                            Timestamp.Get(hitObject),
                            THREASHOLD_AFTER_ENH,
                            timeBetweenAfter
                        ).ForDifficulties(
                            Beatmap.Difficulty.Easy,
                            Beatmap.Difficulty.Normal,
                            Beatmap.Difficulty.Hard
                        );
                    }

                    if (timeBetweenAfter < THREASHOLD_AFTER_IX && timeBetweenAfter != -1.0)
                    {
                        yield return new Issue(
                            GetTemplate(SPINNER_AFTER),
                            beatmap,
                            Timestamp.Get(hitObject),
                            THREASHOLD_AFTER_IX,
                            timeBetweenAfter
                        ).ForDifficulties(
                            Beatmap.Difficulty.Insane,
                            Beatmap.Difficulty.Expert,
                            Beatmap.Difficulty.Ultra
                        );
                    }

                    if (timeBetweenBefore < THREASHOLD_BEFORE_EN && timeBetweenBefore != -1.0)
                    {
                        yield return new Issue(
                            GetTemplate(SPINNER_BEFORE),
                            beatmap,
                            Timestamp.Get(hitObject),
                            THREASHOLD_BEFORE_EN,
                            timeBetweenBefore
                        ).ForDifficulties(
                            Beatmap.Difficulty.Easy,
                            Beatmap.Difficulty.Normal
                        );
                    }

                    if (timeBetweenBefore < THREASHOLD_BEFORE_HI && timeBetweenBefore != -1.0)
                    {
                        yield return new Issue(
                            GetTemplate(SPINNER_BEFORE),
                            beatmap,
                            Timestamp.Get(hitObject),
                            THREASHOLD_BEFORE_HI,
                            timeBetweenBefore
                        ).ForDifficulties(
                            Beatmap.Difficulty.Hard,
                            Beatmap.Difficulty.Insane
                        );
                    }

                    if (timeBetweenBefore < THREASHOLD_BEFORE_X && timeBetweenBefore != -1.0)
                    {
                        yield return new Issue(
                            GetTemplate(SPINNER_BEFORE),
                            beatmap,
                            Timestamp.Get(hitObject),
                            THREASHOLD_BEFORE_X,
                            timeBetweenBefore
                        ).ForDifficulties(
                            Beatmap.Difficulty.Expert,
                            Beatmap.Difficulty.Ultra
                        );
                    }
                }
            }
        }
    }
}
