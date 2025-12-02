// ============================================
// RequiemDataSO.cs
// Forward declaration - full implementation in Week 5
// ============================================

using UnityEngine;

namespace HNR.Characters
{
    /// <summary>
    /// Defines a playable Requiem character's static data.
    /// Forward declaration for card system references.
    /// Full implementation per TDD 05 in Week 5.
    /// </summary>
    [CreateAssetMenu(fileName = "New Requiem", menuName = "HNR/Requiem Data")]
    public class RequiemDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _requiemId;
        [SerializeField] private string _requiemName;

        // Public accessors
        public string RequiemId => _requiemId;
        public string RequiemName => _requiemName;

        // TODO: Full implementation in Week 5
        // - Classification (class, soul aspect)
        // - Base stats (HP, ATK, DEF, SE rate)
        // - Starting/unlockable cards
        // - Null State effects
        // - Visual assets
    }
}
