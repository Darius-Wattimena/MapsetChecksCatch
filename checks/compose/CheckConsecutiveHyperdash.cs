using System;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System.Collections.Generic;
using MapsetChecksCatch.helper;

namespace MapsetChecksCatch.checks.compose
{
    [Check]
    public class CheckConsecutiveHyperdash : BeatmapCheck
    {
        private const string Consecutive = "Consecutive";
        private const string ConsecutiveRainSnap = "ConsecutiveRainSnap";
        private const string ConsecutivePlatterSnap = "ConsecutivePlatterSnap";

        private const int ThresholdPlatter = (int) AllowedHyperDashConsecutive.DIFF_H;
        private const int ThresholdRain = (int) AllowedHyperDashConsecutive.DIFF_I;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Too many consecutive hyperdashes.",
            Modes = new[] {Beatmap.Mode.Catch},
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Note",
                    "So far only checks consecutive hyperdashes and highersnapped hyperdashes followed by a different snap hyperdash."
                },
                {
                    "Purpose",
                    @"
                    <b>Rain</b> : 
                    Basic hyperdashes must not be used more than four times between consecutive fruits. 
                    If higher-snapped hyperdashes are used, they must not be used in conjunction with other hyperdashes or higher-snapped dashes.
                    </br>
                    <b>Platter</b> : 
                    Basic hyperdashes must not be used more than two times between consecutive fruits. 
                    If higher-snapped hyperdashes are used, they must be used singularly (not in conjunction with other hyperdashes or dashes)."
                },
                {
                    "Reasoning",
                    @"
                    The amount of hyperdashes used in a difficulty should be increasing which each difficulty level.
                    In platters the maximum amount of hyperdashes is set to two because the difficulty is meant to be an introduction to hypers."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    Consecutive,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Too many consecutive hyperdashes were used and should be at most {1}, currently {2}.",
                            "timestamp - ", "rule amount", "amount")
                        .WithCause("To many consecutive hyperdash are used.")
                },
                {
                    ConsecutivePlatterSnap,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} A highersnapped hyperdash followed by a different snapped hyperdash.",
                            "timestamp - ")
                        .WithCause("Higher snapped hyperdash followed by a different snapped hyperdash.")
                },
                {
                    ConsecutiveRainSnap,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} A highersnapped hyperdash followed by a different snapped hyperdash.",
                            "timestamp - ")
                        .WithCause("Higher snapped hyperdash followed by a different snapped hyperdash.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjectManager = new ObjectManager();
            var catchObjects = catchObjectManager.LoadBeatmap(beatmap);

            catchObjectManager.CalculateJumps(catchObjects, beatmap);

            var count = 0;
            CatchHitObject firstHyperdash = null;
            CatchHitObject lastObject = null;
            var nextMustBeSameSnap = false;
            foreach (var currentObject in catchObjects)
            {
                if (nextMustBeSameSnap)
                {
                    if (lastObject.Extras == null)
                    {
                        var originTime = Math.Abs(lastObject.time - currentObject.time);
                        var time = Math.Abs(currentObject.GetPrevDeltaTime());

                        //TODO recognize normal dashes
                        if ((originTime > time + 5 || originTime < time - 5) && lastObject.IsHyperDash)
                        {
                            yield return new Issue(
                                GetTemplate(ConsecutivePlatterSnap),
                                beatmap,
                                Timestamp.Get(currentObject.time)
                            ).ForDifficulties(Beatmap.Difficulty.Hard);

                            yield return new Issue(
                                GetTemplate(ConsecutiveRainSnap),
                                beatmap,
                                Timestamp.Get(currentObject.time)
                            ).ForDifficulties(Beatmap.Difficulty.Insane);
                        }
                    }

                    nextMustBeSameSnap = false;
                }

                // Check if we came from a hyperdash
                if (firstHyperdash != null)
                {
                    // Check if it was highersnapped for platter/rain rule
                    if (currentObject.IsHigherSnapped(Beatmap.Difficulty.Hard, lastObject.IsHyperDash)
                        || currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane, lastObject.IsHyperDash))
                    {
                        nextMustBeSameSnap = true;
                    }
                }

                if (currentObject.IsHyperDash)
                {
                    count++;
                    lastObject = currentObject;

                    if (firstHyperdash == null)
                    {
                        firstHyperdash = currentObject;
                    }

                    continue;
                }

                // No more hdashes check
                if (count > ThresholdPlatter)
                {
                    yield return new Issue(
                        GetTemplate(Consecutive),
                        beatmap,
                        Timestamp.Get(lastObject.time),
                        ThresholdPlatter,
                        count
                    ).ForDifficulties(Beatmap.Difficulty.Hard);
                }

                if (count > ThresholdRain)
                {
                    yield return new Issue(
                        GetTemplate(Consecutive),
                        beatmap,
                        Timestamp.Get(lastObject.time),
                        ThresholdRain,
                        count
                    ).ForDifficulties(Beatmap.Difficulty.Insane);
                }

                count = 0;
                firstHyperdash = null;
            }
        }
    }
}