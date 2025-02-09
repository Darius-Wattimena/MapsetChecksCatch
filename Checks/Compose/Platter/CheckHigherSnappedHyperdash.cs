using System;
using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Parser.Statics;
using MapsetVerifier.Framework.Objects;
using MapsetVerifier.Framework.Objects.Attributes;
using MapsetVerifier.Framework.Objects.Metadata;

namespace MapsetChecksCatch.Checks.Compose.Platter
{
    [Check]
    public class CheckHigherSnappedHyperdash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[P] Higher-snapped hyperdash.",
            Modes = new[] {Beatmap.Mode.Catch},
            Difficulties = new[] {Beatmap.Difficulty.Hard},
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
                { "HigherSnapFollowedByDashesOrHyperdashes",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Higher-snapped hyperdashes followed by a dash or hyperdash.",
                            "timestamp - ")
                        .WithCause(
                            "Higher-snapped hyperdash followed by a dash.")
                },
                { "TooStrongHigherSnapFollowedByAntiFlow",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Too strong higher-snapped hyperdashes followed by antiflow, allowed trigger distance {1} current {2}.",
                            "timestamp - ", "trigger distance allowed", "trigger distance")
                        .WithCause(
                            "Too strong higher-snapped hyperdash followed by antiflow.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            foreach (var currentObject in catchObjects)
            {
                if (currentObject.Target == null) continue;
                
                if (currentObject.MovementType == MovementType.HYPERDASH)
                {
                    var hyperTriggerDistance = (int) currentObject.GetTriggerDistance();
                    var currentTriggerDistance = (int) currentObject.GetCurrentTriggerDistance();
                    
                    if (currentObject.IsHigherSnapped(Beatmap.Difficulty.Hard))
                    {
                        var isNextAWalk = currentObject.Target.MovementType == MovementType.WALK;

                        if (isNextAWalk)
                        {
                            // Hyperdashes that are higher-snapped should not be followed by antiflow patterns.
                            //     If used, the spacing should not exceed a distance snap of 1.1 times the trigger distance
                            if (hyperTriggerDistance * 1.1 < currentTriggerDistance && currentObject.NoteDirection != currentObject.Target.NoteDirection)
                            {
                                yield return new Issue(
                                    GetTemplate("TooStrongHigherSnapFollowedByAntiFlow"),
                                    beatmap,
                                    TimestampHelper.Get(currentObject, currentObject.Target),
                                    (int) (hyperTriggerDistance * 1.1),
                                    currentTriggerDistance
                                ).ForDifficulties(Beatmap.Difficulty.Hard);
                            }
                        }
                        else
                        {
                            // Hyperdashes that are higher-snapped must not be used in conjunction with any other dashes or hyperdashes.
                            yield return new Issue(
                                GetTemplate("HigherSnapFollowedByDashesOrHyperdashes"),
                                beatmap,
                                TimestampHelper.Get(currentObject, currentObject.Target, currentObject.Target.Target)
                            ).ForDifficulties(Beatmap.Difficulty.Hard);
                        }
                    }
                }
            }
        }
    }
}