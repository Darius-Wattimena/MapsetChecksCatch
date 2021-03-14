using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using static MapsetParser.objects.Beatmap.Mode;

namespace MapsetChecksCatch.Checks.Compose
{
    [Check]
    public class CheckHyperdashSnap : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Disallowed hyperdash snap.",
            Modes = new[] { Catch },
            Difficulties = new[] { Beatmap.Difficulty.Hard, Beatmap.Difficulty.Insane },
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Hyperdashes may only be used when the snapping is above or equal to the allowed threshold.
                    </br>
                    <ul>
                    <li>For Platters at least <i>125ms or higher</i> must be between the ticks of the desired snapping.</li>
                    <li>For Rains at least <i>62ms or higher</i> must be between the ticks of the desired snapping.</li>
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
                { "HyperdashSnap",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} current snap is not allowed, only ticks with at least {1}ms are allowed, currently {2}ms.",
                            "timestamp - ", "allowed", "current")
                        .WithCause("The used snap is not allowed.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            CatchHitObject lastCheckedObject = null;

            if (catchObjects == null || catchObjects.Count == 0)
            {
                yield break;
            }

            // We set i = 1 to skip the first object
            for (var i = 1; i < catchObjects.Count; i++)
            {
                var currentObject = catchObjects[i];

                if (lastCheckedObject == null)
                {
                    lastCheckedObject = catchObjects[i - 1];
                }

                if (lastCheckedObject.MovementType == MovementType.HYPERDASH)
                {
                    var snap = (int) (currentObject.time - lastCheckedObject.time);

                    if (snap < 125 && snap > 0)
                    {
                        yield return new Issue(
                            GetTemplate("HyperdashSnap"),
                            beatmap,
                            Timestamp.Get(currentObject.time),
                            125,
                            snap
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }

                    if (snap < 62 && snap > 0)
                    {
                        yield return new Issue(
                            GetTemplate("HyperdashSnap"),
                            beatmap,
                            Timestamp.Get(currentObject.time),
                            62,
                            snap
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }
                }

                lastCheckedObject = currentObject;

                //Check snaps for slider parts
                foreach (var sliderObjectExtra in currentObject.Extras)
                {
                    if (lastCheckedObject.MovementType == MovementType.HYPERDASH)
                    {
                        var snap = (int) (sliderObjectExtra.time - lastCheckedObject.time);

                        if (snap < 62 && snap > 0)
                        {
                            yield return new Issue(
                                GetTemplate("HyperdashSnap"),
                                beatmap,
                                Timestamp.Get(currentObject.time),
                                62,
                                snap
                            ).ForDifficulties(Beatmap.Difficulty.Insane);
                        }
                    }

                    lastCheckedObject = sliderObjectExtra;
                }
            }
        }
    }
}
