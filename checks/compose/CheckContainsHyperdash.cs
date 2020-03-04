using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System.Collections.Generic;
using System.Linq;
using MapsetParser.statics;

namespace MapsetChecksCatch.checks.compose
{
    [Check]
    public class CheckContainsHyperdash : BeatmapCheck
    {
        private const string ContainsHyperdash = "ContainsHyperdash";
        private const string ContainsHyperdashExtra = "ContainsHyperdashExtra";
        private const string ContainsHyperdashExtraRain = "ContainsHyperdashExtraRain";

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Hyperdash.",
            Modes = new[] { Beatmap.Mode.Catch },
            Difficulties = new [] { Beatmap.Difficulty.Easy,Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard, Beatmap.Difficulty.Insane },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Hyperdashes should only be used in the selected difficulties, those are: Platter, Rain, Overdose. 
                    For platters they should not be used on individual droplets or slider ends/repeats, on rains they are allowed but they should be used properly."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    ContainsHyperdash,
                    new IssueTemplate(Issue.Level.Problem, 
                            "{0} Is a hyperdash.",
                            "timestamp - ")
                        .WithCause("A hyperdash on a Cup or Salad is not allowed.")
                },
                {
                    ContainsHyperdashExtra,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} Is a hyperdash on a droplet, slider-repeat or slider-end.",
                            "timestamp - ")
                        .WithCause("A hyperdash on a droplet, slider-repeat or slider-end for Platter difficulties is not allowed.")
                },
                {
                    ContainsHyperdashExtraRain,
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Is a hyperdash on a droplet, slider-repeat or slider-end, make sure this is intended.",
                            "timestamp - ")
                        .WithCause("A hyperdash on a droplet, slider-repeat or slider-end for Rain difficulties should be used with caution.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var diff = beatmap.GetDifficulty();
            var diffByName = beatmap.GetDifficulty(true);

            if (diff == Beatmap.Difficulty.Expert || diff == Beatmap.Difficulty.Ultra || diffByName == Beatmap.Difficulty.Expert)
            {
                yield break;
            }

            var catchObjectManager = new ObjectManager();
            var catchObjects = catchObjectManager.LoadBeatmap(beatmap);
            catchObjectManager.LoadOrigins(catchObjects);

            catchObjectManager.CalculateJumps(catchObjects, beatmap);

            foreach (var currentObject in catchObjects)
            {
                if (currentObject.Origin == null || currentObject.type == HitObject.Type.Spinner)
                {
                    continue;
                }

                var currentCheckingObject = currentObject;

                if (currentCheckingObject.Origin.IsHyperDash)
                {
                    yield return new Issue(
                        GetTemplate(ContainsHyperdash),
                        beatmap,
                        Timestamp.Get(currentCheckingObject.Origin.time)
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
                }

                foreach (var currentObjectExtra in currentObject.Extras)
                {
                    currentCheckingObject = currentObjectExtra;

                    if (!currentCheckingObject.Origin.IsHyperDash)
                    {
                        continue;
                    }

                    yield return new Issue(
                        GetTemplate(ContainsHyperdash),
                        beatmap,
                        Timestamp.Get(currentCheckingObject.Origin.time)
                    ).ForDifficulties(Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);

                    yield return new Issue(
                        GetTemplate(ContainsHyperdashExtra),
                        beatmap,
                        Timestamp.Get(currentCheckingObject.Origin.time)
                    ).ForDifficulties(Beatmap.Difficulty.Hard);

                    yield return new Issue(
                        GetTemplate(ContainsHyperdashExtraRain),
                        beatmap,
                        Timestamp.Get(currentCheckingObject.Origin.time)
                    ).ForDifficulties(Beatmap.Difficulty.Insane);
                }
            }
        }
    }
}
