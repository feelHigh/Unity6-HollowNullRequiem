// ============================================
// MapEnums.cs
// Enumerations for map node types and states
// ============================================

namespace HNR.Map
{
    /// <summary>
    /// Predefined map shape patterns controlling node distribution across columns.
    /// Creates organic diamond/hourglass shapes instead of random grids.
    /// </summary>
    public enum MapShapePattern
    {
        /// <summary>
        /// Classic roguelike: 1 → expand → plateau → contract → 1
        /// Example (7 cols): 1 → 2 → 3 → 4 → 3 → 2 → 1
        /// Example (5 cols): 1 → 3 → 4 → 3 → 1
        /// </summary>
        Diamond,

        /// <summary>
        /// Aggressive expansion then sharp convergence.
        /// Example: 1 → 4 → 4 → 3 → 2 → 1
        /// </summary>
        Hourglass,

        /// <summary>
        /// Wide middle plateau with gradual expansion/contraction.
        /// Example: 1 → 2 → 3 → 4 → 4 → 4 → 3 → 2 → 1
        /// </summary>
        WideDiamond,

        /// <summary>
        /// Uses custom array from ZoneConfigSO.CustomNodeDistribution.
        /// Allows complete control over node counts per column.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Types of nodes that can appear on the map.
    /// </summary>
    public enum NodeType
    {
        /// <summary>Starting node (row 0), no encounter.</summary>
        Start,

        /// <summary>Standard enemy encounter.</summary>
        Combat,

        /// <summary>Elite enemy encounter with better rewards.</summary>
        Elite,

        /// <summary>Void Market - purchase cards/relics.</summary>
        Shop,

        /// <summary>Narrative event with choices.</summary>
        Echo,

        /// <summary>Rest site - heal or upgrade cards.</summary>
        Sanctuary,

        /// <summary>Free reward chest.</summary>
        Treasure,

        /// <summary>Zone boss (final row).</summary>
        Boss
    }

    /// <summary>
    /// Current state of a map node.
    /// </summary>
    public enum NodeState
    {
        /// <summary>Cannot access yet - no path from visited nodes.</summary>
        Locked,

        /// <summary>Can travel to - connected to a visited node.</summary>
        Available,

        /// <summary>Player is currently at this node.</summary>
        Current,

        /// <summary>Already completed this node.</summary>
        Visited
    }
}
