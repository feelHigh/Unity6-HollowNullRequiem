// ============================================
// Card.cs
// Card UI component for hand display
// ============================================

using System;
using UnityEngine;
using HNR.Core.Interfaces;

namespace HNR.Cards
{
    /// <summary>
    /// Card UI component displayed in hand.
    /// Handles visual display, hover, and selection states.
    /// </summary>
    /// <remarks>
    /// TODO: Implement full card visuals in Week 4.
    /// </remarks>
    public class Card : MonoBehaviour, IPoolable
    {
        // ============================================
        // Events
        // ============================================

        /// <summary>Fired when card is clicked.</summary>
        public event Action<Card> OnCardClicked;

        /// <summary>Fired when pointer enters card.</summary>
        public event Action<Card> OnCardHoverEnter;

        /// <summary>Fired when pointer exits card.</summary>
        public event Action<Card> OnCardHoverExit;

        // ============================================
        // Properties
        // ============================================

        /// <summary>The card data instance this UI represents.</summary>
        public CardInstance CardInstance { get; private set; }

        /// <summary>Whether this card is currently selected.</summary>
        public bool IsSelected { get; private set; }

        // ============================================
        // Private Fields
        // ============================================

        private Canvas _canvas;
        private int _baseSortOrder;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize card with instance data.
        /// </summary>
        public void Initialize(CardInstance cardInstance)
        {
            CardInstance = cardInstance;
            IsSelected = false;
            // TODO: Update visuals based on cardInstance.Data
        }

        /// <summary>
        /// Set the sorting order for proper overlap display.
        /// </summary>
        public void SetSortOrder(int order)
        {
            _baseSortOrder = order;
            if (_canvas != null)
            {
                _canvas.sortingOrder = order;
            }
        }

        /// <summary>
        /// Set selected state visual.
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            // TODO: Update selection visual
        }

        // ============================================
        // Input Handling (Placeholder)
        // ============================================

        private void OnMouseDown()
        {
            OnCardClicked?.Invoke(this);
        }

        private void OnMouseEnter()
        {
            OnCardHoverEnter?.Invoke(this);
        }

        private void OnMouseExit()
        {
            OnCardHoverExit?.Invoke(this);
        }

        // ============================================
        // IPoolable Implementation
        // ============================================

        public void OnSpawnFromPool()
        {
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            CardInstance = null;
            IsSelected = false;
            OnCardClicked = null;
            OnCardHoverEnter = null;
            OnCardHoverExit = null;
            gameObject.SetActive(false);
        }
    }
}
