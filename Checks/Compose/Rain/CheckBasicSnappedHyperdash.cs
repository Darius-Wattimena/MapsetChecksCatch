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

namespace MapsetChecksCatch.Checks.Compose.Rain
{
    [Check]
    public class CheckBasicSnappedHyperdash : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = "Compose",
            Message = "[R] Basic-snapped hyperdash.",
            Modes = new[] {Beatmap.Mode.Catch},
            Difficulties = new[] {Beatmap.Difficulty.Insane},
            Author = "Greaper",

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    TODO"
                },
                {
                    "Reason",
                    @"
                    TODO"
                }
            }
        };

        //Hyperdashes that are basic-snapped must not be used more than two times within a slider. The slider path must be simple and easy-to-follow.
        //
        //Hyperdashes that are basic-snapped should not be used consecutively when different beat snaps are used.
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "SliderHyperdashes",
                    new IssueTemplate(Issue.Level.Problem,
                            "{0} At most 2 basic-snapped hyperdashes are allowed to be used within a slider, currently {1}.",
                            "timestamp - ", "amount")
                        .WithCause(
                            "More then 2 basic-snapped hyperdashes within a slider.")
                },
                { "DifferentSnap",
                    new IssueTemplate(Issue.Level.Warning,
                            "{0} Basic-snapped hyperdashes of different snap are used.",
                            "timestamp - ")
                        .WithCause(
                            "Different snapped basic-snapped hyperdashes.")
                }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            var catchObjects = CheckBeatmapSetDistanceCalculation.GetBeatmapDistances(beatmap);
            CatchHitObject basicSnappedHyperdash = null;

            foreach (var currentObject in catchObjects)
            {
                if (currentObject.Target == null) continue;

                if (basicSnappedHyperdash != null)
                {
                    if (currentObject.MovementType == MovementType.HYPERDASH 
                        && !currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane) 
                        && !basicSnappedHyperdash.IsSameSnap(currentObject))
                    {
                        yield return new Issue(
                            GetTemplate("DifferentSnap"),
                            beatmap,
                            TimestampHelper.Get(basicSnappedHyperdash, currentObject)
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }

                    basicSnappedHyperdash = null;
                }
                
                if (currentObject.NoteType == NoteType.HEAD)
                {
                    var sliderHeadHyperCount = currentObject.MovementType == MovementType.HYPERDASH 
                                               && !currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane)
                        ? 1
                        : 0;
                    var sliderHyperCount = sliderHeadHyperCount + currentObject.Extras.Count(x => 
                        x.MovementType == MovementType.HYPERDASH &&
                        !x.IsHigherSnapped(Beatmap.Difficulty.Insane));

                    if (sliderHyperCount > 2)
                    {
                        yield return new Issue(
                            GetTemplate("SliderHyperdashes"),
                            beatmap,
                            TimestampHelper.Get(currentObject),
                            sliderHyperCount
                        ).ForDifficulties(Beatmap.Difficulty.Insane);
                    }
                }
                
                if (currentObject.MovementType == MovementType.HYPERDASH 
                    && !currentObject.IsHigherSnapped(Beatmap.Difficulty.Insane))
                {
                    basicSnappedHyperdash = currentObject;
                }
            }
        }
    }
}