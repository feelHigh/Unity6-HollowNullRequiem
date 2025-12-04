// ============================================
// MapEnums.cs
// Enumerations for map node types and states
// ============================================

namespace HNR.Map
{
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
