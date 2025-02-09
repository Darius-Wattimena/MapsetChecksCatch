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
    public class CheckBasicSnappedDash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[P] Basic-snapped dash.",
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
        
        //Dashes that are basic-snapped must not be used more than four times between consecutive fruits.
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "BasicSnappedConsecutive",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Too many consecutive basic-snapped dashes were used and should be at most {1}, currently {2}.",
                            "timestamp - ", "allowed amount", "amount")
                        .WithCause(
                            "Too many consecutive basic-snapped dashes.")
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
                    if (currentObject.IsBasicSnapped(Beatmap.Difficulty.Hard))
                    {
                        consecutiveObjects.Add(currentObject);
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
                    var total = consecutiveObjects.Count;
                    consecutiveObjects.Add(currentObject);

                    if (total > 2)
                    {
                        yield return new Issue(
                            GetTemplate("BasicSnappedConsecutive"),
                            beatmap,
                            TimestampHelper.Get(consecutiveObjects.ToArray()),
                            2,
                            total
                        ).ForDifficulties(Beatmap.Difficulty.Hard);
                    }
                    
                    // Reset all values after checking for potential issues
                    consecutiveObjects = new List<CatchHitObject>();
                    checkForIssues = false;
                }
            }
            
            var lastTotal = consecutiveObjects.Count;
            
            if (lastTotal > 2)
            {
                yield return new Issue(
                    GetTemplate("BasicSnappedConsecutive"),
                    beatmap,
                    TimestampHelper.Get(consecutiveObjects.ToArray()),
                    2,
                    lastTotal
                ).ForDifficulties(Beatmap.Difficulty.Hard);
            }
        }
    }
}