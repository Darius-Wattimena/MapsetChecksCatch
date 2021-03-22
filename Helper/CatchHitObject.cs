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

        /// <summary>
        /// The x coordinate in the osu editor
        /// </summary>
        public float X;
        
        /// <summary>
        /// The amount of distance needed to make this object a hyperdash.
        ///
        /// The object is a hyperdash when the distance is 0 or below.
        /// </summary>
        public int DistanceToHyperDash { get; set; }
        
        /// <summary>
        /// The amount of distance needed to make this object a dash.
        ///
        /// The object is a dash when the distance is 0 or below.
        /// </summary>
        public int DistanceToDash { get; set; }
        
        /// <summary>
        /// Time between the this object and the its target.
        /// </summary>
        public int TimeToTarget { get; set; }
        
        /// <summary>
        /// The target of this object.
        /// </summary>
        public CatchHitObject Target { get; set; }
        
        /// <summary>
        /// All the extra objects of this CatchHitObject.
        /// 
        /// As a slider it contains all the big droplets, slider repeats and tails.
        /// </summary>
        public List<CatchHitObject> Extras { get; set; } = new List<CatchHitObject>();
        
        public bool IsEdgeMovement { get; set; }
        
        /// <summary>
        /// Specify what kind of movement this note is based on the DistanceToHyperDash and DistanceToDash
        /// </summary>
        public MovementType MovementType { get; set; }
        
        /// <summary>
        /// Specify what directional movement is needed to catch the next object.
        /// </summary>
        public NoteDirection NoteDirection { get; set; }
        
        /// <summary>
        /// The type of the current note. This is useful when having rules about specific slider parts.
        /// </summary>
        public NoteType NoteType { get; }

        /// <summary>
        /// Helper function to get the name used in the RC for this hit object.
        /// </summary>
        /// <returns>Name of the hit object</returns>
        public string GetNoteTypeName()
        {
            return NoteType switch
            {
                NoteType.CIRCLE => "Circle",
                NoteType.HEAD => "Slider head",
                NoteType.REPEAT => "Slider repeat",
                NoteType.TAIL => "Slider tail",
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
