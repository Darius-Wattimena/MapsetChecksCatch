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
        private const string CONSECUTIVE = "Consecutive";
        private const string CONSECUTIVE_RAIN_SNAP = "ConsecutiveRainSnap";
        private const string CONSECUTIVE_PLATTER_SNAP = "ConsecutivePlatterSnap";

        private const int THREASHOLD_H = 2;
        private const int THREASHOLD_I = 4;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Too many consecutive hyperdashes.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>()
                {
                    {
                        "Purpose",
                        @"
                        soon."
                    },
                    {
                        "Reasoning",
                        @"
                        soon."
                    }
                }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                {
                    CONSECUTIVE,
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} To many consecutive hyperdashes were used and should be at most {1}, currently {2}.",
                        "timestamp - ", "rule amount", "amount")
                    .WithCause("To many consecutive hyperdash are used.")
                },
                {
                    CONSECUTIVE_PLATTER_SNAP,
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} A highersnapped hyperdash followed by a different snapped hyperdash or dash.",
                        "timestamp - ")
                    .WithCause("Higher snapped hyperdash followed by a different snapped hyperdash.")
                },
                {
                    CONSECUTIVE_RAIN_SNAP,
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} A highersnapped hyperdash followed by a different snapped hyperdash.",
                        "timestamp - ")
                    .WithCause("Higher snapped hyperdash followed by a different snapped hyperdash.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            var catchObjectManager = new ObjectManager();

            List<CatchHitObject> catchObjects = catchObjectManager.GenerateCatchObjects(aBeatmap);
            catchObjectManager.CalculateJumps(catchObjects, aBeatmap);
            var count = 0;
            CatchHitObject firstHyperdash = null;
            CatchHitObject lastHyperdash = null;
            bool nextMustBeSameSnap = false;
            for (var i = 0; i < catchObjects.Count; i++)
            {
                CatchHitObject currentObject = catchObjects[i];

                if (nextMustBeSameSnap)
                {
                    if (lastHyperdash.Extras == null)
                    {
                        var originTime = Math.Abs(lastHyperdash.time - currentObject.time);
                        var time = Math.Abs(currentObject.GetPrevDeltaTime());

                        //TODO recoginize normal dashes
                        if ((originTime > time + 5 || originTime < time - 5) && currentObject.Origin.IsHyperDash)
                        {
                            yield return new Issue(
                                    GetTemplate(CONSECUTIVE_PLATTER_SNAP),
                                    aBeatmap,
                                    Timestamp.Get(currentObject.time)
                                ).ForDifficulties(Beatmap.Difficulty.Hard);

                            yield return new Issue(
                                    GetTemplate(CONSECUTIVE_RAIN_SNAP),
                                    aBeatmap,
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
                    if (currentObject.IsHigherSnapped(Beatmap.Difficulty.Hard) || currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane))
                    {
                        nextMustBeSameSnap = true;
                    }
                }

                if (currentObject.IsHyperDash)
                {
                    count++;
                    lastHyperdash = catchObjects[i];

                    if (firstHyperdash == null)
                    {
                        firstHyperdash = catchObjects[i];
                    }

                    continue;
                } 
                else
                {
                    // No more hdashes check
                    if (count > THREASHOLD_H)
                    {
                        yield return new Issue(
                            GetTemplate(CONSECUTIVE),
                            aBeatmap,
                            Timestamp.Get(lastHyperdash.time),
                            THREASHOLD_H,
                            count
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }

                    if (count > THREASHOLD_I)
                    {
                        yield return new Issue(
                            GetTemplate(CONSECUTIVE),
                            aBeatmap,
                            Timestamp.Get(lastHyperdash.time),
                            THREASHOLD_I,
                            count
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }

                    count = 0;
                    firstHyperdash = null;
                }
            }
        }
    }
}
