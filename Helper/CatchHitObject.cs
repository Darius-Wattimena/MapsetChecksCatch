using System.Collections.Generic;
using MapsetParser.objects;

namespace MapsetChecksCatch.Helper
{
    public sealed class CatchHitObject : HitObject
    {
        public CatchHitObject(string[] anArgs, Beatmap beatmap, NoteType type) : base(anArgs, beatmap)
        {
            X = Position.X;
            NoteType = type;
        }

        public float X;
        public double DistanceToHyperDash { get; set; }
        public double DistanceToDash { get; set; }
        public CatchHitObject Target { get; set; }
        public CatchHitObject Origin { get; set; }
        public List<CatchHitObject> Extras { get; set; } = new List<CatchHitObject>();
        public MovementType MovementType { get; set; }
        public NoteType NoteType { get; set; }
    }

    public enum NoteType
    {
        CIRCLE,
        HEAD,
        REPEAT,
        TAIL,
        DROPLET
    }
}
