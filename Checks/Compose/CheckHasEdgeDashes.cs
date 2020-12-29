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
using static MapsetParser.objects.Beatmap;

namespace MapsetChecksCatch.Checks.Compose
{
    [Check]
    public class CheckHasEdgeDashes : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Edge dash.",
            Modes = new[] { Mode.Catch },
            Author = "Greaper",

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
                { "EdgeDashMinor",
                    new IssueTemplate(Issue.Level.Minor,
                            "{0} This object is {1} pixel(s) away from being a hyper, make sure this is intended.",
                            "timestamp - ", "x")
                        .WithCause(
                            "X amount of pixels off to become a hyperdash.")
                },
                { "EdgeDash",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} This object is {1} pixel(s) away from being a hyper, make sure this is used properly.",
                            "timestamp - ", "x")
                        .WithCause(
                            "X amount of pixels off to become a hyperdash.")
                },
                { "EdgeDashProblem",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} This object is {1} pixel(s) away from being a hyper.",
                            "timestamp - ", "x")
                        .WithCause(
                            "Usage of edge dashes on lower diffs.")
                }
            };
        }

        private static Issue EdgeDashIssue(IssueTemplate template, Beatmap beatmap, CatchHitObject currentObject, params Beatmap.Difficulty[] difficulties)
        {
            return new Issue(
                template,
                beatmap,
                Timestamp.Get(currentObject.time),
                $"{Math.Round(currentObject.DistanceToHyperDash)}"
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            CheckBeatmapSetDistanceCalculation.SetBeatmaps.TryGetValue(beatmap.metadataSettings.version, out var catchObjects);

            var issueObjects = new List<CatchHitObject>();

            foreach (var currentObject in catchObjects
                .Where(currentObject => currentObject.type != HitObject.Type.Spinner && currentObject.MovementType != MovementType.HYPERDASH))
            {
                var hyperDistance = currentObject.DistanceToHyperDash;

                if (hyperDistance > 0)
                {
                    issueObjects.Add(currentObject);
                }

                if (currentObject.Extras == null) continue;

                foreach (var sliderExtras in currentObject.Extras)
                {
                    var sliderObjectHyperDistance = sliderExtras.DistanceToHyperDash;

                    if (sliderExtras.MovementType != MovementType.HYPERDASH && sliderObjectHyperDistance > 0)
                    {
                        issueObjects.Add(sliderExtras);
                    }
                }
            }

            foreach (var issueObject in issueObjects)
            {
                if (issueObject.DistanceToHyperDash < 5)
                {
                    yield return EdgeDashIssue(GetTemplate("EdgeDash"), beatmap, issueObject,
                        Beatmap.Difficulty.Insane);

                    yield return EdgeDashIssue(GetTemplate("EdgeDashMinor"), beatmap, issueObject,
                        Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                }

                if (issueObject.DistanceToHyperDash < 10)
                {
                    yield return EdgeDashIssue(GetTemplate("EdgeDashProblem"), beatmap, issueObject,
                        Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);
                }
            }
        }
    }
}
