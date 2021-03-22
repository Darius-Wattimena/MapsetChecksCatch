using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MapsetParser.objects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetChecksCatch.Checks.Settings
{
    [Check]
    public class CheckLuminosity : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Modes = new[] { Beatmap.Mode.Catch },
            Category = "Settings",
            Message = "Too dark or bright combo colours.",
            Author = "Greaper",

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
                    Light colours create bright pulses during Kiai time, this can be unpleasant to the eyes and should be avoided."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                { "LuminosityLow",
                    new IssueTemplate(Issue.Level.Warning,
                            "Combo {0} uses a luminosity value of {1}, this is below 50 and should not be used.",
                            "x", "luminosity")
                        .WithCause(
                            "Luminosity is lower then 50.") },
                { "LuminosityHigh",
                    new IssueTemplate(Issue.Level.Warning,
                            "Combo {0} uses a luminosity value of {1}, this is above 220 and should not be used.",
                            "x", "luminosity")
                        .WithCause(
                            "Luminosity is higher then 220.") }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            for (var i = 0; i < beatmap.colourSettings.combos.Count; i++)
            {
                var comboColour = beatmap.colourSettings.combos[i];
                var luminosity = GetLuminosity(comboColour);

                if (luminosity < 50)
                {
                    yield return new Issue(GetTemplate("LuminosityLow"), beatmap, i, luminosity);
                }

                if (luminosity > 220)
                {
                    yield return new Issue(GetTemplate("LuminosityHigh"), beatmap, i, luminosity);
                }
            }
        }

        public double GetLuminosity(Vector3 comboColour)
        {
            var rgbArray = new[]
            {
                comboColour.X / 255, 
                comboColour.Y / 255, 
                comboColour.Z / 255
            };
            var max = rgbArray.Max();
            var min = rgbArray.Min();

            // Get the luminosity percentage
            var l = (min + max) / 2;

            // Luminosity in osu is a number between 0 and 240 so we multiply by 240
            return Math.Round(l * 240);
        }
    }
}
