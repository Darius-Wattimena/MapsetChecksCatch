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
                    </br>
                    On Rains edge dashes may only be used singularly. 
                    </br>
                    On Overdose they may be used with caution for a maximum of three consecutive objects, and should not be used after hyperdashes."
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
                            "timestamp - ", "amount")
                        .WithCause("X amount of pixels off to become a hyperdash.")
                },
                {
                    EdgeDash,
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} This object is {1} pixel(s) away from being a hyper, make sure this is used properly.",
                            "timestamp - ", "amount")
                        .WithCause("X amount of pixels off to become a hyperdash.")
                },
                {
                    EdgeDashProblem,
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} This object is {1} pixel(s) away from being a hyper.",
                            "timestamp - ", "amount")
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
                $"{currentObject.DistanceToHyperDash}"
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjectManager = new ObjectManager();
            var catchObjects = catchObjectManager.LoadBeatmap(beatmap);

            catchObjectManager.CalculateJumps(catchObjects, beatmap);

            var issueObjects = new List<CatchHitObject>();

            foreach (var currentObject in catchObjects
                .Where(currentObject => currentObject.type != HitObject.Type.Spinner && !currentObject.IsHyperDash))
            {
                var hyperDistance = currentObject.DistanceToHyperDash;

                if (hyperDistance >= 0.1)
                {
                    issueObjects.Add(currentObject);
                }

                if (currentObject.Extras == null) continue;

                foreach (var currentObjectExtra in currentObject.Extras)
                {
                    var extraHyperDistance = currentObjectExtra.DistanceToHyperDash;

                    if (!currentObjectExtra.IsHyperDash && extraHyperDistance >= 0.1)
                    {
                        issueObjects.Add(currentObjectExtra);
                    }
                }

                
            }

            foreach (var issueObject in issueObjects)
            {
                if (issueObject.DistanceToHyperDash < 5)
                {
                    yield return EdgeDashIssue(GetTemplate(EdgeDash), beatmap, issueObject, Beatmap.Difficulty.Insane);

                    yield return EdgeDashIssue(GetTemplate(EdgeDashMinor), beatmap, issueObject, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                }

                if (issueObject.DistanceToHyperDash < 10)
                {
                    yield return EdgeDashIssue(GetTemplate(EdgeDashProblem), beatmap, issueObject, Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);
                }
            }
        }
    }
}