using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;

namespace MapsetChecksCatch.checks.settings
{
    [Check]
    public class CheckComboColours : BeatmapCheck
    {
        private const string ComboColoursLow = "ComboColoursLow";
        private const string ComboColoursHigh = "ComboColoursHigh";

        private const int ThresholdLuminosityLow = (int) Luminosity.LOWER;
        private const int ThresholdLuminosityHigh = (int) Luminosity.HIGHER;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_SETTINGS,
            Message = "Odd luminosity in combo colours.",
            Modes = new[] {Beatmap.Mode.Catch},
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    Combo colours should use a luminosity higher then ~50, a luminosity lower then ~220 must be used if Kiai time is used."
                },
                {
                    "Reasoning",
                    @"
                    Dark colours impact readability of fruits with low background dim. 
                    Light colours create bright pulses during Kiai time, which can be unpleasant to the eyes."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    ComboColoursLow,
                    new IssueTemplate(Issue.Level.Minor,
                            "Combo {0} uses a low luminosity value, recommend to use a value above ~70. Current {1}",
                            "combo", "temp")
                        .WithCause("Luminosity is around or lower then ~50.")
                },
                {
                    ComboColoursHigh,
                    new IssueTemplate(Issue.Level.Minor,
                            "Combo {0} uses a high luminosity value, recommend to use a value below ~200. Current {1}",
                            "combo", "temp")
                        .WithCause("Luminosity is around or higher then ~220.")
                }
            };
        }

        // FIXME: Osu does something else here although doing luminosity * 240 does seem to be pretty close
        public double GetLuminosity(float r, float g, float b)
        {
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));

            var luminosity = 240 - Math.Round((max - min) * (120f/255f));

            return luminosity;
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            for (var i = 0; i < beatmap.colourSettings.combos.Count; i++)
            {
                var comboColor = beatmap.colourSettings.combos[i];

                var luminosity = GetLuminosity(comboColor.X, comboColor.Y, comboColor.Z);

                if (luminosity < ThresholdLuminosityLow)
                {
                    yield return new Issue(GetTemplate(ComboColoursLow),
                        beatmap,
                        i + 1,
                        luminosity
                    );
                }

                if (luminosity > ThresholdLuminosityHigh)
                {
                    yield return new Issue(GetTemplate(ComboColoursHigh),
                        beatmap,
                        i + 1,
                        luminosity
                    );
                }
            }
        }
    }
}