using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CheckHasEdgeWalk : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Edge walks.",
            Modes = new[] { Beatmap.Mode.Catch },
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
                { "EdgeWalkMinor",
                    new IssueTemplate(Issue.Level.Minor,
                            "{0} This object is {1} pixel(s) away from being a dash, make sure this is intended.",
                            "timestamp - ", "x")
                        .WithCause(
                            "X amount of pixels off to become a dash.")
                },
                { "EdgeWalk",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} This object is {1} pixel(s) away from being a dash, this should be avoided if possible.",
                            "timestamp - ", "x")
                        .WithCause(
                            "X amount of pixels off to become a dash.")
                }
            };
        }

        private static Issue EdgeDashIssue(IssueTemplate template, Beatmap beatmap, CatchHitObject currentObject, params Beatmap.Difficulty[] difficulties)
        {
            return new Issue(
                template,
                beatmap,
                Timestamp.Get(currentObject.time),
                $"{Math.Round(currentObject.DistanceToDash)}"
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            CheckBeatmapSetDistanceCalculation.SetBeatmaps.TryGetValue(beatmap.metadataSettings.version, out var catchObjects);

            var issueObjects = new List<CatchHitObject>();

            foreach (var currentObject in catchObjects
                .Where(currentObject => currentObject.type != HitObject.Type.Spinner && currentObject.MovementType != MovementType.DASH))
            {
                var dashDistance = currentObject.DistanceToDash;

                if (dashDistance > 0)
                {
                    issueObjects.Add(currentObject);
                }

                if (currentObject.Extras == null) continue;

                foreach (var sliderExtras in currentObject.Extras)
                {
                    var sliderObjectDashDistance = sliderExtras.DistanceToDash;

                    if (sliderExtras.MovementType != MovementType.DASH && sliderObjectDashDistance > 0)
                    {
                        issueObjects.Add(sliderExtras);
                    }
                }
            }

            foreach (var issueObject in issueObjects)
            {
                if (issueObject.DistanceToDash < 5)
                {
                    yield return EdgeDashIssue(GetTemplate("EdgeWalkMinor"), beatmap, issueObject,
                        Beatmap.Difficulty.Hard);
                }

                if (issueObject.DistanceToDash < 10)
                {
                    yield return EdgeDashIssue(GetTemplate("EdgeWalk"), beatmap, issueObject,
                        Beatmap.Difficulty.Easy, Beatmap.Difficulty.Normal);
                }
            }
        }
    }
}
