using System;
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
        public int DistanceToHyperDash { get; set; }
        public int DistanceToDash { get; set; }
        public CatchHitObject Target { get; set; }
        public CatchHitObject Origin { get; set; }
        public List<CatchHitObject> Extras { get; set; } = new List<CatchHitObject>();
        public MovementType MovementType { get; set; }
        public NoteType NoteType { get; set; }

        public string GetNoteTypeName()
        {
            return NoteType switch
            {
                NoteType.CIRCLE => "Circle",
                NoteType.HEAD => "Slider Head",
                NoteType.REPEAT => "Slider Repeat",
                NoteType.TAIL => "Slider Tail",
                NoteType.DROPLET => "Droplet",
                _ => "NULL"
            };
        }
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
