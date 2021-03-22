using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose.Salad
{
    [Check]
    public class CheckDashSnap : BeatmapCheck
    {
        private const int AllowedBasicSnappedDashes = 2;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Disallowed hyperdash snap.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new[] { Beatmap.Difficulty.Normal },
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
                { "AmountOfDashes",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} only 2 basic dashes in a row are allowed, currently its {1}.",
                            "timestamp - ", "current amount")
                        .WithCause("3 or more consecutive dashes in a row are used.")
                },
                { "HigherSnappedDash",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} is a higher-snapped dash and not followed by a walk.",
                            "timestamp - ")
                        .WithCause("Next object is a dash.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);
            
            if (catchObjects.Count == 0)
            {
                yield break;
            }

            CatchHitObject lastObject = null;
            var nextMustBeWalkable = false;
            var dashObjects = new List<CatchHitObject>();

            foreach (var catchObject in catchObjects)
            {
                if (nextMustBeWalkable)
                {
                    if (catchObject.MovementType != MovementType.WALK)
                    {
                        yield return new Issue(
                            GetTemplate("HigherSnappedDash"),
                            beatmap,
                            Timestamp.Get(lastObject)
                        ).ForDifficulties(Beatmap.Difficulty.Normal);
                    }

                    nextMustBeWalkable = false;
                }

                if (catchObject.MovementType is MovementType.DASH)
                {
                    var isHigherSnapped = catchObject.IsHigherSnapped(Beatmap.Difficulty.Normal);

                    if (isHigherSnapped)
                    {
                        nextMustBeWalkable = true;
                    }
                    
                    dashObjects.Add(catchObject);
                }
                else
                {
                    if (dashObjects.Count > AllowedBasicSnappedDashes)
                    {
                        yield return new Issue(
                            GetTemplate("AmountOfDashes"),
                            beatmap,
                            Timestamp.Get(dashObjects.ToArray())
                        ).ForDifficulties(Beatmap.Difficulty.Normal);
                    }
                    
                    // This was no dash so we can reset the counter
                    dashObjects = new List<CatchHitObject>();
                }

                lastObject = catchObject;
            }
        }
    }
}