using System;
using System.Collections.Generic;
using System.Linq;
using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.checks.compose
{
    [Check]
    public class CheckEdgeDash : BeatmapCheck
    {
        private const string EdgeDash = "EdgeDash";
        private const string EdgeDashProblem = "EdgeDashProblem";
        private const string EdgeDashMinor = "EdgeDashMinor";

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_COMPOSE,
            Message = "Edge dash.",
            Modes = new[] {Beatmap.Mode.Catch},
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Edge dashes are hard and shouldn't be used on lower difficulties. 
                    They can be used on Rains and Overdoses, although if they are used they mush be used properly."
                },
                {
                    "Reasoning",
                    @"
                    Edge dashes require precise movement, on lower difficulties we cannot expect such accuracy from players.
                    On Rains "
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    EdgeDashMinor,
                    new IssueTemplate(Issue.Level.Minor,
                            "{0} This object is {1} pixel(s) away from being a hyper, make sure this is intended.",
                            "timestamp - ", "pixel(s)")
                        .WithCause("X amount of pixels off to become a hyperdash.")
                },
                {
                    EdgeDash,
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} This object is {1} pixel(s) away from being a hyper, make sure this is used properly.",
                            "timestamp - ", "pixel(s)")
                        .WithCause("X amount of pixels off to become a hyperdash.")
                },
                {
                    EdgeDashProblem,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} This object is {1} pixel(s) away from being a hyper.",
                            "timestamp - ", "pixel(s)")
                        .WithCause("Usage of edge dashes on lower diffs.")
                }
            };
        }

        private static Issue EdgeDashIssue(IssueTemplate template, Beatmap beatmap, CatchHitObject currentObject, params Beatmap.Difficulty[] difficulties)
        {
            return new Issue(
                template,
                beatmap,
                Timestamp.Get(currentObject.time),
                $"{(int) Math.Ceiling(currentObject.DistanceToHyperDash)}"
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjectManager = new ObjectManager();

            var catchObjects = catchObjectManager.GenerateCatchObjects(beatmap);
            catchObjectManager.CalculateJumps(catchObjects, beatmap);

            foreach (var currentObject in catchObjects
                .Where(currentObject => !currentObject.IsHyperDash && currentObject.type != HitObject.Type.Spinner))
            {
                if (currentObject.Extras == null)
                {
                    if ((int) Math.Ceiling(currentObject.DistanceToHyperDash) < 1) continue;

                    if (currentObject.DistanceToHyperDash < 5)
                    {
                        yield return EdgeDashIssue(GetTemplate(EdgeDash), beatmap, currentObject, Beatmap.Difficulty.Insane);

                        yield return EdgeDashIssue(GetTemplate(EdgeDashMinor), beatmap, currentObject, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                    }

                    if (currentObject.DistanceToHyperDash < 10)
                    {
                        yield return EdgeDashIssue(GetTemplate(EdgeDashProblem), beatmap, currentObject, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);
                    }
                }
                else
                {
                    foreach (var sliderPart in currentObject.Extras
                        .Where(sliderPart => !sliderPart.IsHyperDash && !((int) Math.Ceiling(sliderPart.DistanceToHyperDash) < 1)))
                    {
                        if (sliderPart.DistanceToHyperDash < 5)
                        {
                            yield return EdgeDashIssue(GetTemplate(EdgeDash), beatmap, sliderPart, Beatmap.Difficulty.Insane);

                            yield return EdgeDashIssue(GetTemplate(EdgeDashMinor), beatmap, sliderPart, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                        }

                        if (sliderPart.DistanceToHyperDash < 10)
                        {
                            yield return EdgeDashIssue(GetTemplate(EdgeDashProblem), beatmap, sliderPart, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);
                        }
                    }
                }
            }
        }
    }
}