using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System.Collections.Generic;
using static MapsetParser.objects.Beatmap;

namespace MapsetChecksCatch.checks.compose
{
    [Check]
    public class CheckMaxCombo : BeatmapCheck
    {
        private const string MaxCombo = "MaxCombo";
        private const string MaxComboSignificantly = "MaxComboSignificantly";

        private const int ThresholdCup = 8;
        private const int ThresholdSalad = 10;
        private const int ThresholdPlatter = 12;
        private const int ThresholdRainAndOverdose = 16;

        private const double SignificantMultiplier = 1.5;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Too high combo.",
            Modes = new[] {Mode.Catch},
            Author = Settings.AUTHOR,

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
                {
                    MaxComboSignificantly,
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} The amount of combo exceeds the guideline of {1} significantly, currently it has {2}.",
                            "timestamp - ", "guideline combo", "combo")
                        .WithCause("The combo amount exceeds the guideline significantly.")
                },
                {
                    MaxCombo,
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
            Difficulty[] difficulties, bool isSignificant = false)
        {
            return new Issue(
                GetTemplate(isSignificant ? MaxComboSignificantly : MaxCombo),
                beatmap,
                Timestamp.Get(startObject),
                maxCombo,
                currentCount
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var count = 1;
            var catchObjectManager = new ObjectManager(beatmap);
            var catchObjects = catchObjectManager.Objects;

            if (catchObjects.Count == 0)
            {
                yield break;
            }

            var startObject = catchObjects[0];

            for (var i = 1; i < catchObjects.Count; i++)
            {
                var currentObject = catchObjects[i];

                // Parse hitobject types as we can't check flags
                var objectCodeArgs = currentObject.code.Split(',');
                var objectTypes = (Type) int.Parse(objectCodeArgs[3]);


                if (objectTypes.HasFlag(Type.NewCombo) || i == catchObjects.Count - 1)
                {
                    // FIXME: amount of the same calls
                    if (count > ThresholdCup)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdCup, count, new[] {Difficulty.Easy});
                    }

                    if (count > ThresholdCup * SignificantMultiplier)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdCup, count, new[] {Difficulty.Easy},
                            true);
                    }

                    if (count > ThresholdSalad)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdSalad, count, new[] {Difficulty.Normal});
                    }

                    if (count > ThresholdSalad * SignificantMultiplier)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdSalad, count, new[] {Difficulty.Normal},
                            true);
                    }

                    if (count > ThresholdPlatter)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdPlatter, count, new[] {Difficulty.Hard});
                    }

                    if (count > ThresholdPlatter * SignificantMultiplier)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdPlatter, count, new[] {Difficulty.Hard},
                            true);
                    }

                    if (count > ThresholdRainAndOverdose)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdRainAndOverdose, count,
                            new[] {Difficulty.Insane, Difficulty.Expert, Difficulty.Ultra});
                    }

                    if (count > ThresholdRainAndOverdose * SignificantMultiplier)
                    {
                        yield return ComboIssue(beatmap, startObject, ThresholdRainAndOverdose, count,
                            new[] {Difficulty.Insane, Difficulty.Expert, Difficulty.Ultra}, true);
                    }

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
        }
    }
}