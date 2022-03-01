using System;
using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose.Rain
{
    [Check]
    public class CheckHigherSnapHyperdash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[R] Higher-snapped hyperdash.",
            Modes = new[] {Beatmap.Mode.Catch},
            Difficulties = new[] {Beatmap.Difficulty.Insane},
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
                { "HyperdashesWithHigherSnappedDashes",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Higher-snapped hyperdash must not be used together with a higher-snapped dash.",
                            "timestamp - ")
                        .WithCause(
                            "Higher-snapped hyperdash followed by a higher-snapped dash.")
                },
                { "ConsecutiveHyperdashes",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Higher-snapped hyperdashes can't be used consecutively.",
                            "timestamp - ")
                        .WithCause(
                            "Consecutive higher-snapped hyperdash.")
                },
                { "HyperdashSlider",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Higher-snapped hyperdash placed on slider head/repeat or droplet.",
                            "timestamp - ")
                        .WithCause(
                            "Higher-snapped hyperdash placed on slider head/repeat or droplet.")
                },
                { "HyperdashWithAntiFlowDash250",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Higher-snapped hyperdash followed by an antiflow dash which is smaller then 250ms.",
                            "timestamp - ")
                        .WithCause(
                            "Higher-snapped hyperdash followed by a <250 ms antiflow dash.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);
            CatchHitObject higherSnappedHyperdash = null;
            
            foreach (var currentObject in catchObjects)
            {
                if (higherSnappedHyperdash != null)
                {
                    // Hyperdashes that are higher-snapped must not be used in conjunction with any other hyperdashes.
                    if (currentObject.MovementType == MovementType.HYPERDASH)
                    {
                        yield return new Issue(
                            GetTemplate("ConsecutiveHyperdashes"),
                            beatmap,
                            TimestampHelper.Get(higherSnappedHyperdash, currentObject)
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }
                    else if (currentObject.MovementType == MovementType.DASH)
                    {
                        // Hyperdashes that are higher-snapped must not be used in conjunction with higher-snapped dashes.
                        if (currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane) 
                            && higherSnappedHyperdash.TimeToTarget > 125)  // Dash accuracy isn't correct at low ms
                        {
                            yield return new Issue(
                                GetTemplate("HyperdashesWithHigherSnappedDashes"),
                                beatmap,
                                TimestampHelper.Get(higherSnappedHyperdash, currentObject, currentObject.Target)
                            ).ForDifficulties(Beatmap.Difficulty.Insane);
                        }

                        // Hyperdashes that are higher-snapped should not be followed by antiflow dashes with a gap lower than 250ms.
                        if (higherSnappedHyperdash.NoteDirection != currentObject.NoteDirection)
                        {
                            if (currentObject.TimeToTarget < 250)
                            {
                                yield return new Issue(
                                    GetTemplate("HyperdashWithAntiFlowDash250"),
                                    beatmap,
                                    TimestampHelper.Get(higherSnappedHyperdash, currentObject, currentObject.Target)
                                ).ForDifficulties(Beatmap.Difficulty.Insane);
                            }
                        }
                    }
                }
                
                if (currentObject.Target == null) continue;
                
                if (currentObject.MovementType == MovementType.HYPERDASH)
                {
                    if (currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane))
                    {
                        // Hyperdashes that are higher-snapped must not be used within a slider.
                        switch (currentObject.NoteType)
                        {
                            case NoteType.HEAD:
                            case NoteType.REPEAT:
                            case NoteType.DROPLET:
                                yield return new Issue(
                                    GetTemplate("HyperdashSlider"),
                                    beatmap,
                                    TimestampHelper.Get(currentObject, currentObject.Target)
                                ).ForDifficulties(Beatmap.Difficulty.Insane);
                                break;
                        }
                        
                        higherSnappedHyperdash = currentObject;
                    }
                    else
                    {
                        higherSnappedHyperdash = null;
                    }
                }
                else
                {
                    higherSnappedHyperdash = null;
                }
            }
        }
    }
}