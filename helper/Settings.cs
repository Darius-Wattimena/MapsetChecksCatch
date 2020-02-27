namespace MapsetChecksCatch.helper
{
    public class Settings
    {
        public const string AUTHOR = "Greaper";

        public const string CATEGORY_COMPOSE = "Compose";
        public const string CATEGORY_SETTINGS = "Settings";

        public const float CATCHER_SPEED = 512;
        public const float CATCHER_WALK_SPEED = CATCHER_SPEED / 2;
    }

    public enum AllowedDash
    {
        DIFF_N = 125,
        DIFFs_HI = 62
    }

    public enum AllowedHyperDash
    {
        DIFF_H = 125,
        DIFF_I = 62,
    }

    public enum AllowedSpinnerGapStart
    {
        DIFFS_EN = 250,
        DIFFS_HI = 125,
        DIFF_X = 62
    }

    public enum AllowedSpinnerGapEnd
    {
        DIFFS_ENH = 250,
        DIFFS_IX = 125
    }
}
