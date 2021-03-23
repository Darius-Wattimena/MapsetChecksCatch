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
    public class CheckBasicDash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Higher-snapped anti-flow dash.",
            Modes = new[] {Beatmap.Mode.Catch},
            Difficulties = new[] {Beatmap.Difficulty.Normal},
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
                {
                    "Warning",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} should be basic-snapped dashes of the same snap.",
                            "timestamp - ")
                        .WithCause("A basic-snapped dash is followed by a different basic-snapped dash")
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

            var nextShouldBeSameSnap = false;
            CatchHitObject lastObject = null;

            foreach (var catchObject in catchObjects)
            {
                if (nextShouldBeSameSnap)
                {
                    if (catchObject.MovementType == MovementType.DASH &&
                        catchObject.IsHigherSnapped(Beatmap.Difficulty.Normal))
                    {
                        if (!lastObject.IsSameSnap(catchObject))
                        {
                            yield return new Issue(
                                GetTemplate("Warning"),
                                beatmap,
                                Timestamp.Get(lastObject, catchObject, catchObject.Target)
                            ).ForDifficulties(Beatmap.Difficulty.Normal);
                        }
                    }

                    nextShouldBeSameSnap = false;
                }

                if (catchObject.MovementType == MovementType.DASH)
                {
                    var higherSnapped = catchObject.IsHigherSnapped(Beatmap.Difficulty.Normal);

                    if (higherSnapped)
                    {
                        continue;
                    }

                    nextShouldBeSameSnap = true;
                    lastObject = catchObject;
                }
            }
        }
    }
}