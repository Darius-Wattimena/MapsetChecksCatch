﻿using System.Collections.Generic;
using MapsetChecksCatch.Checks.General;
using MapsetChecksCatch.Helper;
using MapsetVerifier.Framework.Objects;
using MapsetVerifier.Framework.Objects.Attributes;
using MapsetVerifier.Framework.Objects.Metadata;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Parser.Statics;

namespace MapsetChecksCatch.Checks.Compose
{
    [Check]
    public class CheckHasEdgeDash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "Edge dashes.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = "Greaper",
            Difficulties = new[]
            {
                Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard, Beatmap.Difficulty.Insane, Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra
            },

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
                { "StrongDash",
                    new IssueTemplate(Issue.Level.Minor,
                            "{0} {1} is {2} pixel(s) away from being a hyper, this dash might be too strong.",
                            "timestamp - ", "object", "x")
                        .WithCause(
                            "X amount of pixels off to become a hyperdash.")
                },
                { "EdgeDashMinor",
                    new IssueTemplate(Issue.Level.Minor,
                            "{0} {1} is {2} pixel(s) away from being a hyper, make sure this is intended.",
                            "timestamp - ", "object", "x")
                        .WithCause(
                            "X amount of pixels off to become a hyperdash.")
                },
                { "EdgeDash",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} {1} is {2} pixel(s) away from being a hyper, make sure this is used properly.",
                            "timestamp - ", "object", "x")
                        .WithCause(
                            "X amount of pixels off to become a hyperdash.")
                },
                { "EdgeDashProblem",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} {1} is {2} pixel(s) away from being a hyper.",
                            "timestamp - ", "object", "x")
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
                currentObject.GetNoteTypeName(),
                $"{currentObject.DistanceToHyper}"
            ).ForDifficulties(difficulties);
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            // All objects with potential issues
            var issueObjects = new List<CatchHitObject>();

            foreach (var catchObject in catchObjects)
            {
                if (catchObject.MovementType == MovementType.DASH)
                {
                    issueObjects.Add(catchObject);
                }
            }

            foreach (var issueObject in issueObjects)
            {
                if (issueObject.IsEdgeMovement)
                {
                    yield return EdgeDashIssue(GetTemplate("EdgeDash"), beatmap, issueObject,
                        Beatmap.Difficulty.Insane);

                    yield return EdgeDashIssue(GetTemplate("EdgeDashMinor"), beatmap, issueObject,
                        Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                    
                    yield return EdgeDashIssue(GetTemplate("EdgeDashProblem"), beatmap, issueObject,
                        Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard);
                }
                else
                {
                    var isStrongDash = IsStrongDash(beatmap, issueObject);

                    if (isStrongDash)
                    {
                        yield return EdgeDashIssue(GetTemplate("StrongDash"), beatmap, issueObject,
                            Beatmap.Difficulty.Normal, Beatmap.Difficulty.Hard, Beatmap.Difficulty.Insane,
                            Beatmap.Difficulty.Expert, Beatmap.Difficulty.Ultra);
                    }
                }
            }
        }
        
        private static bool IsStrongDash(Beatmap beatmap, CatchHitObject hitObject)
        {
            if (hitObject.MovementType == MovementType.HYPERDASH)
            {
                return false;
            }

            var pixelsScale = (int) beatmap.GetBpmScale(hitObject) * 60;

            return hitObject.DistanceToHyper <= pixelsScale;
        }
    }
}
