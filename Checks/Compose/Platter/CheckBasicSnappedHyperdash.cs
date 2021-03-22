using System;
using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose.Platter
{
    public class CheckBasicSnappedHyperdash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Basic-snapped hyperdash.",
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
                            "{0} Too strong basic-snapped hyperdashes followed by antiflow walk, allowed trigger distance {1}.",
                            "timestamp - ", "amount")
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
                if (currentObject.MovementType == MovementType.HYPERDASH)
                {
                    var hyperTriggerDistance = (int) (Math.Abs(currentObject.X + currentObject.Target.X) 
                                                      - Math.Abs(currentObject.DistanceToHyperDash));
                    var currentTriggerDistance = (int) (Math.Abs(currentObject.X + currentObject.Target.X) 
                                                        + Math.Abs(currentObject.DistanceToHyperDash));
                    
                    if (!currentObject.IsHigherSnapped(Beatmap.Difficulty.Hard))
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
                                            Timestamp.Get(currentObject, currentObject.Target),
                                            (int) (hyperTriggerDistance * 1.2)
                                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                                    }
                                    break;
                                case MovementType.DASH:
                                    // Otherwise only 1.1 if followed by a basic-snapped dash
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