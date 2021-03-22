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
        
        /// <summary>
        /// Hyperdashes that are higher-snapped must not be used in conjunction with higher-snapped dashes or any other hyperdashes.
        /// Hyperdashes that are higher-snapped must not be used within a slider.
        /// 
        /// Hyperdashes that are higher-snapped should not be followed by antiflow dashes with a gap lower than 250ms.
        /// </summary>
        private IEnumerable<Issue> GetHigherSnappedHyperdashesRainIssues(CatchHitObject lastObject, CatchHitObject currentObject, Beatmap beatmap)
        {
            switch (currentObject.MovementType)
            {
                case MovementType.DASH:
                    if (currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane))
                    {
                        yield return new Issue(
                            GetTemplate("RainHigherSnapFollowedByHigherSnapDash"),
                            beatmap,
                            Timestamp.Get(lastObject, currentObject)
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }
                    break;
                case MovementType.HYPERDASH:
                    yield return new Issue(
                        GetTemplate("RainHigherSnapFollowedByHyperdash"),
                        beatmap,
                        Timestamp.Get(lastObject, currentObject)
                    ).ForDifficulties(Beatmap.Difficulty.Insane);
                    break;
            }

            yield return null;
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);
            var hyperdashCount = 0;

            foreach (var currentObject in catchObjects)
            {
                if (currentObject.MovementType == MovementType.HYPERDASH)
                {
                    // Always count the amount of hypers.
                    hyperdashCount += 1;
                }
                else
                {
                    foreach (var issue in GetConsecutiveHyperdashIssues(beatmap, hyperdashCount))
                    {
                        yield return issue;
                    }
                    
                    hyperdashCount = 0;
                }
            }
        }

        /*public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);

            var count = 0;
            CatchHitObject lastObject = null;
            var nextMustBeSameSnap = false;
            var issues = new List<Issue>();
            foreach (var currentObject in catchObjects)
            {
                if (nextMustBeSameSnap && lastObject != null)
                {
                    if (lastObject.NoteType == NoteType.CIRCLE || lastObject.NoteType == NoteType.TAIL)
                    {
                        var lastObjectMsGap = lastObject.TimeToTarget;
                        var currentMsGap = currentObject.TimeToTarget;

                        if (lastObjectMsGap == 0 || currentMsGap == 0) continue;

                        // add + 5 or - 5 to reduce false positives for ~1 ms wrongly snapped objects
                        if ((lastObjectMsGap > currentMsGap + 5 || lastObjectMsGap < currentMsGap - 5) 
                            && lastObject.MovementType == MovementType.HYPERDASH && currentObject.MovementType == MovementType.HYPERDASH)
                        {
                            yield return new Issue(
                                GetTemplate("ConsecutiveHigherSnap"),
                                beatmap,
                                Timestamp.Get(currentObject.time)
                            ).ForDifficulties(Beatmap.Difficulty.Hard);

                            yield return new Issue(
                                GetTemplate("ConsecutiveHigherSnap"),
                                beatmap,
                                Timestamp.Get(currentObject.time)
                            ).ForDifficulties(Beatmap.Difficulty.Insane);
                        }
                    }

                    nextMustBeSameSnap = false;
                }

                // Check if we came from a hyperdash
                if (lastObject != null)
                {
                    // Check if it was highersnapped for platter/rain rule
                    if (BeatmapUtil.IsHigherSnapped(Beatmap.Difficulty.Hard, currentObject, lastObject)
                        || BeatmapUtil.IsHigherSnapped(Beatmap.Difficulty.Insane, currentObject, lastObject))
                    {
                        nextMustBeSameSnap = true;
                    }
                }

                if (currentObject.MovementType == MovementType.HYPERDASH)
                {
                    count++;
                    lastObject = currentObject;
                }
                else
                {
                    issues.AddRange(GetConsecutiveHyperdashIssues(beatmap, count, lastObject));
                    lastObject = null;
                    count = 0;
                }

                foreach (var currentObjectExtra in currentObject.Extras)
                {
                    if (currentObjectExtra.MovementType == MovementType.HYPERDASH)
                    {
                        count++;
                        lastObject = currentObject;
                    }
                    else
                    {
                        issues.AddRange(GetConsecutiveHyperdashIssues(beatmap, count, lastObject));
                        lastObject = null;
                        count = 0;
                    }
                }
            }
        }*/
    }
}
