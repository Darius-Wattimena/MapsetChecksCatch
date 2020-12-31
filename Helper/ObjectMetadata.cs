namespace MapsetChecksCatch.Helper
{ public class ObjectMetadata
    {
        public NoteDirection Direction { get; set; } = NoteDirection.NONE;

        /// <summary>
        /// Time in milliseconds between the current and next object
        /// </summary>
        public double TimeToNext { get; set; }

        /// <summary>
        /// Amount of distance in osu x cords
        /// </summary>
        public float DistanceInOsuCords { get; set; }

        /// <summary>
        /// Distance based on the osu cords and direction
        /// </summary>
        public double HyperDistanceToNext { get; set; }

        /// <summary>
        /// Distance based on the osu cords and direction
        /// </summary>
        public double DashDistanceToNext { get; set; }

        /// <summary>
        /// The amount extra needed distance until a Hyper fruit is generated
        /// When the value is below 0 it means a hyper is generated
        /// Otherwise it must be a dash or walk
        /// </summary>
        public int DistanceToHyper { get; set; }

        /// <summary>
        /// The amount of extra distance needed until a Dash movement is required
        /// When the value ie below 0 it means a dash movement is mandatory
        /// Otherwise it must be a walk
        /// </summary>
        public int DistanceToDash { get; set; }

        public MovementType MovementType { get; set; }
    }

    public enum MovementType
    {
        WALK,
        DASH,
        HYPERDASH
    }

    public enum NoteDirection
    {
        NONE,
        LEFT,
        RIGHT
    }
}
