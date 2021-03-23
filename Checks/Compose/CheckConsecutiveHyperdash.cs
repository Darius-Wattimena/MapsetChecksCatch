using System;
using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Compose
{
    [Check]
    public class CheckConsecutiveHyperdash : BeatmapCheck
    {
        private const int ThresholdPlatter = 2;
        private const int ThresholdRain = 4;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Too many consecutive hyperdashes.",
            Difficulties = new[] { Beatmap.Difficulty.Hard, Beatmap.Difficulty.Insane },
            Modes = new[] { Beatmap.Mode.Catch },
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Note",
                    "So far only checks consecutive hyperdashes and highersnapped hyperdashes followed by a different snap hyperdash."
                },
                {
                    "Purpose",
                    @"
                    <b>Rain</b> : 
                    Basic hyperdashes must not be used more than four times between consecutive fruits. 
                    If higher-snapped hyperdashes are used, they must not be used in conjunction with other hyperdashes or higher-snapped dashes.
                    <br/>
                    <b>Platter</b> : 
                    Basic hyperdashes must not be used more than two times between consecutive fruits. 
                    If higher-snapped hyperdashes are used, they must be used singularly (not in conjunction with other hyperdashes or dashes)."
                },
                {
                    "Reasoning",
                    @"
                    The amount of hyperdashes used in a difficulty should be increasing which each difficulty level.
                    In platters the maximum amount of hyperdashes is set to two because the difficulty is meant to be an introduction to hypers."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "Consecutive",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Too many consecutive hyperdashes were used and should be at most {1}, currently {2}.",
                            "timestamp - ", "rule amount", "amount")
                        .WithCause(
                            "Too many consecutive hyperdash are used.")
                }
            };
        }

        private IEnumerable<Issue> GetConsecutiveHyperdashIssues(Beatmap beatmap, int count, params HitObject[] objects)
        {
            if (count > ThresholdPlatter)
            {
                yield return new Issue(
                    GetTemplate("Consecutive"),
                    beatmap,
                    Timestamp.Get(objects),
                    ThresholdPlatter,
                    count
                ).ForDifficulties(Beatmap.Difficulty.Hard);
            }

            if (count > ThresholdRain)
            {
                yield return new Issue(
                    GetTemplate("Consecutive"),
                    beatmap,
                    Timestamp.Get(objects),
                    ThresholdRain,
                    count
                ).ForDifficulties(Beatmap.Difficulty.Insane);
            }
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);
            var hyperdashObjects = new List<CatchHitObject>();

            foreach (var currentObject in catchObjects)
            {
                if (currentObject.MovementType == MovementType.HYPERDASH)
                {
                    // Always count the amount of hypers.
                    hyperdashObjects.Add(currentObject);
                }
                else
                {
                    foreach (var issue in GetConsecutiveHyperdashIssues(beatmap, hyperdashObjects.Count, hyperdashObjects.ToArray()))
                    {
                        yield return issue;
                    }

                    hyperdashObjects = new List<CatchHitObject>();
                }
            }
        }
    }
}
