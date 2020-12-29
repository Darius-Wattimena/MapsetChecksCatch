using System;
using System.Collections.Generic;
using System.Text;
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
    public class CheckMaxCombo : BeatmapCheck
    {
        private const int ThresholdCup = 8;
        private const int ThresholdSalad = 10;
        private const int ThresholdPlatter = 12;
        private const int ThresholdRainAndOverdose = 16;

        private const double SignificantMultiplier = 1.5;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Too high combo.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Combos should not reach unreasonable lengths."
                },
                {
                    "Reasoning",
                    @"
                    Caught fruits will stack up on the plate and can potentially obstruct the player's view. 
                    Bear in mind that slider tails, repeats and spinner bananas also count as fruits."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "MaxComboSignificantly",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} The amount of combo exceeds the guideline of {1} significantly, currently it has {2}.",
                            "timestamp - ", "guideline combo", "combo")
                        .WithCause("The combo amount exceeds the guideline significantly.")
                },
                { "MaxCombo",
                    new IssueTemplate(Issue.Level.Minor,
                            "{0} The amount of combo exceeds the guideline of {1}, currently it has {2}.",
                            "timestamp - ", "guideline combo", "combo")
                        .WithCause("The combo amount exceeds the guideline.")
                }
            };
        }

        private enum Type
        {
            NewCombo = 4
        }

        private Issue ComboIssue(Beatmap beatmap, CatchHitObject startObject, int maxCombo, int currentCount,
            Beatmap.Difficulty[] difficulties, bool isSignificant = false)
        {
            return new Issue(
                GetTemplate(isSignificant ? "MaxComboSignificantly" : "MaxCombo"),
                beatmap,
                Timestamp.Get(startObject),
                maxCombo,
                currentCount
            ).ForDifficulties(difficulties);
        }

        public Issue GetComboIssues(Beatmap beatmap, CatchHitObject startObject, int count, int threshold, params Beatmap.Difficulty[] difficulties)
        {
            if (count > threshold * SignificantMultiplier)
            {
                return ComboIssue(beatmap, startObject, threshold, count, difficulties, true);
            }

            if (count > threshold)
            {
                return ComboIssue(beatmap, startObject, threshold, count, difficulties);
            }

            return null;
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var count = 1;
            CheckBeatmapSetDistanceCalculation.SetBeatmaps.TryGetValue(beatmap.metadataSettings.version, out var catchObjects);

            if (catchObjects == null || catchObjects.Count == 0)
            {
                yield break;
            }

            var startObject = catchObjects[0];
            var issues = new List<Issue>();

            for (var i = 1; i < catchObjects.Count; i++)
            {
                var currentObject = catchObjects[i];

                // Parse hitobject types as we can't check flags
                var objectCodeArgs = currentObject.code.Split(',');
                var objectTypes = (Type)int.Parse(objectCodeArgs[3]);

                if (objectTypes.HasFlag(Type.NewCombo))
                {
                    AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdCup, Beatmap.Difficulty.Easy));
                    AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdSalad, Beatmap.Difficulty.Normal));
                    AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdPlatter, Beatmap.Difficulty.Hard));
                    AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdRainAndOverdose, Beatmap.Difficulty.Insane, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra));

                    // Reset values for new combo
                    startObject = currentObject;
                    count = 1;
                }
                else
                {
                    count++;
                    if (currentObject.Extras == null) continue;
                    count += currentObject.Extras.Count;
                }
            }

            // Check last combo of the map
            AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdCup, Beatmap.Difficulty.Easy));
            AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdSalad, Beatmap.Difficulty.Normal));
            AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdPlatter, Beatmap.Difficulty.Hard));
            AddIfNotNull(issues, GetComboIssues(beatmap, startObject, count, ThresholdRainAndOverdose, Beatmap.Difficulty.Insane, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra));

            foreach (var issue in issues)
            {
                yield return issue;
            }
        }

        public static void AddIfNotNull(List<Issue> issues, Issue issue)
        {
            if (issue != null)
            {
                issues.Add(issue);
            }
        }
    }
}
