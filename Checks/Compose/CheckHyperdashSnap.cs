using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Parser.Statics;
using MapsetVerifier.Framework.Objects;
using MapsetVerifier.Framework.Objects.Attributes;
using MapsetVerifier.Framework.Objects.Metadata;

namespace MapsetChecksCatch.Checks.Compose
{
    [Check]
    public class CheckHyperdashSnap : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Disallowed hyperdash snap.",
            Modes = new[] { Beatmap.Mode.Catch },
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
                            "{0} Current snap is not allowed, only ticks with at least {1}ms are allowed, currently {2}ms.",
                            "timestamp - ", "allowed", "current")
                        .WithCause("The used snap is not allowed.")
                }
            };
        }

        // TODO REWORK
        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            if (catchObjects.Count == 0)
            {
                yield break;
            }

            var lastCheckedObject = catchObjects[0];

            // We set i = 1 to skip the first object
            for (var i = 1; i < catchObjects.Count; i++)
            {
                var currentObject = catchObjects[i];

                if (lastCheckedObject.MovementType == MovementType.HYPERDASH)
                {
                    var snap = (int) (currentObject.time - lastCheckedObject.time);

                    if (snap < 125 && snap > 0)
                    {
                        yield return new Issue(
                            GetTemplate("HyperdashSnap"),
                            beatmap,
                            TimestampHelper.Get(lastCheckedObject, currentObject),
                            125,
                            snap
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }

                    if (snap < 62 && snap > 0)
                    {
                        yield return new Issue(
                            GetTemplate("HyperdashSnap"),
                            beatmap,
                            TimestampHelper.Get(lastCheckedObject, currentObject),
                            62,
                            snap
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }
                }

                lastCheckedObject = currentObject;
            }
        }
    }
}
