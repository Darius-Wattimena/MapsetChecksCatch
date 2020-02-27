using MapsetParser.objects;
using System.Collections.Generic;

namespace MapsetChecksCatch.helper
{
    public class CatchHitObject : HitObject
    {
        public CatchHitObject(string[] anArgs, Beatmap aBeatmap) : base(anArgs, aBeatmap)
        {
            x = Position.X;
        }

        public float x;
        public float DistanceToHyperDash { get; set; }
        public double PixelsToHyperDash { get; set; }
        public bool IsHyperDash => HyperDashTarget != null;
        public CatchHitObject HyperDashTarget { get; set; }
        public CatchHitObject Origin { get; set; }
        public List<CatchHitObject> Extras { get; set; }

        public bool IsHigherSnapped(Beatmap.Difficulty difficulty)
        {
            var ms = GetPrevDeltaTime();

            if (difficulty == Beatmap.Difficulty.Normal)
            {
                return ms < (int) AllowedDash.DIFF_N;
            }
            else if (difficulty == Beatmap.Difficulty.Hard)
            {
                return ms < (IsHyperDash ? (int) AllowedHyperDash.DIFF_H * 2 : (int) AllowedDash.DIFFs_HI * 2);
            }
            else if (difficulty == Beatmap.Difficulty.Insane)
            {
                return ms < (IsHyperDash ? (int) AllowedHyperDash.DIFF_I * 2 : (int) AllowedDash.DIFFs_HI * 2);
            }
            else
            {
                // Cup and Overdoses don't have highersnapped distances
                return false;
            }
        }
    }
}
