using MapsetChecksCatch.helper;
using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MapsetChecksCatch.checks.settings
{
    [Check]
    public class CheckComboColours : BeatmapCheck
    {
        private const string COMBO_COLOURS_LOW = "ComboColoursLow";
        private const string COMBO_COLOURS_HIGH = "ComboColoursHigh";

        private const int THREASHOLD_LUMINOSITY_LOW = 50;
        private const int THREASHOLD_LUMINOSITY_HIGH = 220;

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Category = Settings.CATEGORY_SETTINGS,
            Message = "Odd luminosity in combo colours.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>()
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
            return new Dictionary<string, IssueTemplate>()
            {
                {
                    COMBO_COLOURS_LOW,
                    new IssueTemplate(Issue.Level.Minor, "Combo {0} uses a low luminosity value, recommend to use a value above ~70.",
                    "combo")
                    .WithCause("Luminosity is around or lower then ~50.")
                },
                {
                    COMBO_COLOURS_HIGH,
                    new IssueTemplate(Issue.Level.Minor, "Combo {0} uses a high luminosity value, recommend to use a value below ~200.",
                    "combo")
                    .WithCause("Luminosity is around or higher then ~220.")
                }
            };
        }

        // FIXME: Osu does something else here although doing luminosity * 240 does seem to be pretty close
        public double GetLuminosity(float R, float G, float B)
        {
            
            float r = R / 255;
            float g = G / 255;
            float b = B / 255;


            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            double luminosity = (max + min) * 0.5;

            return luminosity * 240;
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            for(var i = 0; i < beatmap.colourSettings.combos.Count; i++) {
                Vector3 comboColor = beatmap.colourSettings.combos[i];
                
                double luminosity = GetLuminosity(comboColor.X, comboColor.Y, comboColor.Z);

                if (luminosity < THREASHOLD_LUMINOSITY_LOW)
                {
                    yield return new Issue(GetTemplate(COMBO_COLOURS_LOW), 
                        beatmap,
                        i + 1
                    );
                }

                if (luminosity > THREASHOLD_LUMINOSITY_HIGH)
                {
                    yield return new Issue(GetTemplate(COMBO_COLOURS_HIGH),
                        beatmap,
                        i + 1
                    );
                }
            }
        }
    }
}
