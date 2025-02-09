using MapsetVerifier.Parser.Objects;

namespace MapsetChecksCatch.Helper
{
    public sealed class CatchHitObject : HitObject
    {
        public CatchHitObject(string[] anArgs, Beatmap beatmap, NoteType type, HitObject original) : base(anArgs, beatmap)
        {
            X = Position.X;
            NoteType = type;
            Original = original;
        }

        public HitObject Original;

        /// <summary>
        /// The x coordinate in the osu editor.
        /// </summary>
        public readonly float X;
        
        /// <summary>
        /// The amount of distance needed to make this object a hyperdash.
        ///
        /// The object is a hyperdash when the distance is below 0.
        /// </summary>
        public float DistanceToHyper { get; set; }
        
        /// <summary>
        /// The amount of distance needed to make this object a dash.
        ///
        /// The object is a dash when the distance is below 0.
        /// </summary>
        public float DistanceToDash { get; set; }
        
        /// <summary>
        /// Time between the this object and the its target.
        /// </summary>
        public double TimeToTarget { get; set; }
        
        public CatchHitObject SliderHead { get; set; }
        
        /// <summary>
        /// The target of this object.
        /// </summary>
        public CatchHitObject Target { get; set; }
        
        public bool IsEdgeMovement { get; set; }
        
        /// <summary>
        /// Specify what kind of movement this note is based on the DistanceToHyper and DistanceToDash
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

        public bool IsSlider => SliderHead != null;

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
        DROPLET,
        SPINNER
    }
}
