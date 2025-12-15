// ============================================
// AllyIndicator.cs
// World-space ally identification near 3D models
// ============================================

using UnityEngine;
using UnityEngine.UI;
using HNR.Characters;

#pragma warning disable CS0414 // Field is assigned but never used (reserved for future Inspector configuration)

namespace HNR.UI.Combat
{
    /// <summary>
    /// Small circular indicator near ally 3D models for quick identification.
    /// Billboards to camera and shows active/inactive state.
    /// </summary>
    public class AllyIndicator : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image _miniPortrait;
        [SerializeField] private Image _circleFrame;
        [SerializeField] private float _indicatorSize = 40f;

        [Header("Position")]
        [SerializeField] private Vector3 _offsetFromModel = new(0, -0.5f, 0);

        private RequiemInstance _requiem;
        private Transform _followTarget;
        private Color _activeColor;
        private Color _inactiveColor;
        private Camera _mainCamera;

        private void Start()
        {
            _activeColor = UIColors.SoulCyan;
            _inactiveColor = UIColors.PanelGray;
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Initializes the indicator for a Requiem.
        /// </summary>
        /// <param name="requiem">The Requiem to represent.</param>
        /// <param name="modelTransform">Transform to follow.</param>
        public void Initialize(RequiemInstance requiem, Transform modelTransform)
        {
            _requiem = requiem;
            _followTarget = modelTransform;

            if (_miniPortrait != null && requiem?.Data?.Portrait != null)
            {
                _miniPortrait.sprite = requiem.Data.Portrait;
            }

            if (_circleFrame != null)
            {
                _circleFrame.color = _inactiveColor;
            }
        }

        private void LateUpdate()
        {
            if (_followTarget != null)
            {
                transform.position = _followTarget.position + _offsetFromModel;

                if (_mainCamera != null)
                {
                    transform.LookAt(_mainCamera.transform);
                    transform.Rotate(0, 180, 0);
                }
            }
        }

        /// <summary>
        /// Sets the active state visual (cyan when active, gray when inactive).
        /// </summary>
        /// <param name="active">True for active highlight.</param>
        public void SetActive(bool active)
        {
            if (_circleFrame != null)
            {
                _circleFrame.color = active ? _activeColor : _inactiveColor;
            }
        }

        /// <summary>
        /// Gets the associated Requiem instance.
        /// </summary>
        public RequiemInstance Requiem => _requiem;
    }
}
