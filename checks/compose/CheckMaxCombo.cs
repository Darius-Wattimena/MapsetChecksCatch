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
        private const string MAX_COMBO = "MaxCombo";
        private const string MAX_COMBO_SIGNIFICANTLY = "MaxComboSignificantly";

        private const int THREASHOLD_E = 8;
        private const int THREASHOLD_N = 10;
        private const int THREASHOLD_H = 12;
        private const int THREASHOLD_IX = 16;

        private const double SIGNIFICANT_MULTIPLIER = 1.5;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Too high combo.",
            Modes = new[] { Mode.Catch },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>()
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
            return new Dictionary<string, IssueTemplate>()
            {
                {
                    MAX_COMBO_SIGNIFICANTLY,
                    new IssueTemplate(Issue.Level.Warning,
                        "{0} The amount of combo exceeds the guideline of {1} significantly, currently it has {2}.",
                        "timestamp - ", "guideline combo", "combo")
                    .WithCause("The combo amount exceeds the guideline significantly.")
                },
                {
                    MAX_COMBO,
                    new IssueTemplate(Issue.Level.Minor,
                        "{0} The amount of combo exceeds the guideline of {1}, currently it has {2}.",
                        "timestamp - ", "guideline combo", "combo")
                    .WithCause("The combo amount exceeds the guideline.")
                }
            };
        }

        private enum Type
        {
            Circle = 1,
            Slider = 2,
            NewCombo = 4,
            Spinner = 8,
            ComboSkip1 = 16,
            ComboSkip2 = 32,
            ComboSkip3 = 64,
            ManiaHoldNote = 128
        }

        private Issue ComboIssue(Beatmap beatmap, CatchHitObject startObject, int maxCombo, int currentCount, Difficulty[] difficulties, bool isSignificant = false)
        {
            return new Issue(
                GetTemplate(isSignificant ? MAX_COMBO_SIGNIFICANTLY : MAX_COMBO),
                beatmap,
                Timestamp.Get(startObject),
                maxCombo,
                currentCount
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            int count = 1;
            CatchHitObject startObject;
            CatchHitObject currentObject;
            var catchObjectManager = new ObjectManager();

            List<CatchHitObject> catchObjects = catchObjectManager.GenerateCatchObjects(beatmap);

            if (catchObjects.Count == 0)
            {
                yield break;
            }

            startObject = catchObjects[0];

            for (var i = 1; i < catchObjects.Count; i++)
            {
                currentObject = catchObjects[i];

                // Parse hitobject types as we can't check flags
                string[] objectCodeArgs = currentObject.code.Split(',');
                Type objectTypes = (Type) int.Parse(objectCodeArgs[3]);


                if (objectTypes.HasFlag(Type.NewCombo) || i == catchObjects.Count - 1)
                {
                    // FIXME: amount of the same calls
                    if (count > THREASHOLD_E)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_E, count, new[] { Difficulty.Easy });
                    }

                    if (count > THREASHOLD_E * SIGNIFICANT_MULTIPLIER)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_E, count, new[] { Difficulty.Easy }, true);
                    }

                    if (count > THREASHOLD_N)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_N, count, new[] { Difficulty.Normal });
                    }

                    if (count > THREASHOLD_N * SIGNIFICANT_MULTIPLIER)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_N, count, new[] { Difficulty.Normal }, true);
                    }

                    if (count > THREASHOLD_H)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_H, count, new[] { Difficulty.Hard });
                    }

                    if (count > THREASHOLD_H * SIGNIFICANT_MULTIPLIER)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_H, count, new[] { Difficulty.Hard }, true);
                    }

                    if (count > THREASHOLD_IX)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_IX, count, new[] { Difficulty.Insane, Difficulty.Expert, Difficulty.Ultra });
                    }

                    if (count > THREASHOLD_IX * SIGNIFICANT_MULTIPLIER)
                    {
                        yield return ComboIssue(beatmap, startObject, THREASHOLD_IX, count, new[] { Difficulty.Insane, Difficulty.Expert, Difficulty.Ultra }, true);
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
