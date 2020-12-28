namespace MapsetChecksCatch.checks
{
    /*class CatchBeatmapCheck : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata
        {
            Category = Settings.CATEGORY_SETTINGS,
            Message = "Loading of ObjectMapper.",
            Modes = new[] { Beatmap.Mode.Catch },
            Author = Settings.AUTHOR,

            Documentation = new Dictionary<string, string>
            {
                {
                    "Purpose",
                    @"
                    -."
                },
                {
                    "Reasoning",
                    @"
                    -."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>
            {
                {
                    "TEMP",
                    new IssueTemplate(Issue.Level.Info, "TODO")
                }
            };
        }

        // FIXME: Osu does something else here although doing luminosity * 240 does seem to be pretty close
        public double GetLuminosity(float r, float g, float b)
        {
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));

            var luminosity = 240 - Math.Round((max - min) * (120f / 255f));

            return luminosity;
        }

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            yield return new Issue("TEST");
        }
    }*/
}
