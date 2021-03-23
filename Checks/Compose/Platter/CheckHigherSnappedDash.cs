using System;
using System.Collections.Generic;
using System.Linq;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose.Platter
{
    [Check]
    public class CheckHigherSnappedDash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Higher-snapped dash.",
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
                { "HigherSnappedConsecutive",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Too many consecutive higher-snapped dashes were used and should be at most {1}, currently {2}.",
                            "timestamp - ", "allowed amount", "amount")
                        .WithCause(
                            "Too many consecutive higher-snapped dashes.")
                },
                { "HigherSnappedAntiFlowConsecutive",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Consecutive higher-snapped dashes must not contain anti-flow.",
                            "timestamp - ", "amount")
                        .WithCause(
                            "Two consecutive higher-snapped dashes with a direction change.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);
            var consecutiveObjects = new List<CatchHitObject>();
            var checkForIssues = false;

            foreach (var currentObject in catchObjects)
            {
                if (currentObject.MovementType == MovementType.DASH)
                {
                    if (currentObject.IsHigherSnapped(Beatmap.Difficulty.Hard))
                    {
                        consecutiveObjects.Add(currentObject);

                        // Check for anti-flow if we have 2 consecutive higher-snapped dashes
                        if (consecutiveObjects.Count == 2)
                        {
                            var firstObject = consecutiveObjects[0];
                            var secondObject = consecutiveObjects[1];

                            if (firstObject.NoteDirection != secondObject.NoteDirection)
                            {
                                yield return new Issue(
                                    GetTemplate("HigherSnappedAntiFlowConsecutive"),
                                    beatmap,
                                    Timestamp.Get(consecutiveObjects.ToArray())
                                ).ForDifficulties(Beatmap.Difficulty.Hard);
                            }
                        }
                    }
                    else
                    {
                        checkForIssues = true;
                    }
                }
                else
                {
                    checkForIssues = true;
                }

                if (checkForIssues)
                {
                    var totalConsecutiveHigherSnappedDashes = consecutiveObjects.Count;
                    consecutiveObjects.Add(currentObject);

                    if (totalConsecutiveHigherSnappedDashes > 2)
                    {
                        
                        yield return new Issue(
                            GetTemplate("HigherSnappedConsecutive"),
                            beatmap,
                            Timestamp.Get(consecutiveObjects.ToArray()),
                            2,
                            totalConsecutiveHigherSnappedDashes
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }
                    
                    // Reset all values after checking for potential issues
                    consecutiveObjects = new List<CatchHitObject>();
                    checkForIssues = false;
                }
            }
        }
    }
}