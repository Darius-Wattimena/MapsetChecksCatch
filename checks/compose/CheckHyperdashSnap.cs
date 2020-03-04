using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.checks.compose
{
    //[Check]
    public class CheckHyperdashSnap : BeatmapCheck
    {
        private const string HyperdashSnap = "HyperdashSnap";

        private const int ThresholdH = (int) AllowedHyperDash.DIFF_H;
        private const int ThresholdI = (int) AllowedHyperDash.DIFF_I;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Disallowed hyperdash snap.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new[] { Beatmap.Difficulty.Hard, Beatmap.Difficulty.Insane },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Hyperdashes may only be used when the snapping is above a certain threshold.
                    </br>
                    <ul>
                    <li>For Platters at least [i]125ms or higher[/i] must be between the ticks of the desired snapping.</li>
                    <li>For Rains at least [i]62ms or higher[/i] must be between the ticks of the desired snapping.</li>
                    </ul>"
                },
                {
                    "Reason",
                    @"
                    To ensure an increase of complexity in each level, hyperdashes can be really harsh on lower difficulties."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    HyperdashSnap,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} current snap is not allowed, only ticks with at least {1}ms are allowed, currently {2}ms.",
                            "timestamp - ", "allowed", "current")
                        .WithCause("The used snap is not allowed.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjectManager = new ObjectManager();
            var catchObjects = catchObjectManager.LoadBeatmap(beatmap);

            catchObjectManager.CalculateJumps(catchObjects, beatmap);

            CatchHitObject checkingObject = null;

            if (catchObjects.Count == 0)
            {
                yield break;
            }

            for (var i = 1; i < catchObjects.Count; i++)
            {
                var currentObject = catchObjects[i];

                if (checkingObject == null)
                {
                    checkingObject = catchObjects[i - 1];
                }

                if (checkingObject.IsHyperDash)
                {
                    var snap = currentObject.time - checkingObject.time;

                    if (snap < ThresholdH)
                    {
                        yield return new Issue(
                            GetTemplate(HyperdashSnap),
                            beatmap,
                            Timestamp.Get(currentObject.time)
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }

                    if (snap < ThresholdI)
                    {
                        yield return new Issue(
                            GetTemplate(HyperdashSnap),
                            beatmap,
                            Timestamp.Get(currentObject.time)
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }
                }

                checkingObject = currentObject;

                //Check snaps for slider parts
                foreach (var currentObjectExtra in currentObject.Extras)
                {
                    if (checkingObject.IsHyperDash)
                    {
                        var snap = currentObjectExtra.time - checkingObject.time;

                        if (snap < ThresholdI)
                        {
                            yield return new Issue(
                                GetTemplate(HyperdashSnap),
                                beatmap,
                                Timestamp.Get(currentObject.time)
                            ).ForDifficulties(Beatmap.Difficulty.Insane);
                        }
                    }

                    checkingObject = currentObject;
                }
            }
        }
    }
}
