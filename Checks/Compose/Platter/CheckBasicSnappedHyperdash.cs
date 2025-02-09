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
    public class CheckBasicSnappedHyperdash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[P] Basic-snapped hyperdash.",
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
                { "AntiFlowWalk",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Too strong basic-snapped hyperdash followed by an antiflow walk, allowed trigger distance {1}.",
                            "timestamp - ", "amount")
                        .WithCause(
                            "Too strong basic-snapped hyperdash followed by antiflow.")
                },
                { "AntiFlowDash",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Too strong basic-snapped hyperdash followed by an antiflow dash, allowed trigger distance {1}.",
                            "timestamp - ", "amount")
                        .WithCause(
                            "Too strong basic-snapped hyperdash followed by antiflow.")
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
                    
                    if (currentObject.IsBasicSnapped(Beatmap.Difficulty.Hard))
                    {
                        // Check if next is antiflow
                        if (currentObject.NoteDirection != currentObject.Target.NoteDirection)
                        {
                            switch (currentObject.Target.MovementType)
                            {
                                case MovementType.WALK:
                                    // When next is a walk it should not exceed 1.2
                                    if (hyperTriggerDistance * 1.2 < currentTriggerDistance)
                                    {
                                        yield return new Issue(
                                            GetTemplate("AntiFlowWalk"),
                                            beatmap,
                                            TimestampHelper.Get(currentObject, currentObject.Target),
                                            (int) (hyperTriggerDistance * 1.2)
                                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                                    }
                                    break;
                                case MovementType.DASH:
                                    // Otherwise only 1.1 if followed by a basic-snapped dash
                                    if (currentObject.Target.IsBasicSnapped(Beatmap.Difficulty.Hard) && hyperTriggerDistance * 1.1 < currentTriggerDistance)
                                    {
                                        yield return new Issue(
                                            GetTemplate("AntiFlowDash"),
                                            beatmap,
                                            TimestampHelper.Get(currentObject, currentObject.Target),
                                            (int) (hyperTriggerDistance * 1.1)
                                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                                    }
                                    // TODO
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}