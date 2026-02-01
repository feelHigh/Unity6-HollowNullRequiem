// ============================================
// EventBannerCarousel.cs
// Main carousel component for event banners
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using HNR.UI.Config;

namespace HNR.UI.Components
{
    /// <summary>
    /// Horizontal scrolling event banner carousel.
    /// Auto-advances every N seconds, pauses on touch, snaps to slides.
    /// Similar to anime gacha game event banners (Genshin, FGO, Arknights).
    /// </summary>
    public class EventBannerCarousel : MonoBehaviour
    {
        // ============================================
        // References
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("Banner configuration asset")]
        private BannerConfigSO _bannerConfig;

        [Header("UI References")]
        [SerializeField, Tooltip("ScrollRect for horizontal scrolling")]
        private ScrollRect _scrollRect;

        [SerializeField, Tooltip("Content container (HorizontalLayoutGroup)")]
        private RectTransform _contentContainer;

        [SerializeField, Tooltip("Container for page indicators")]
        private RectTransform _indicatorContainer;

        [SerializeField, Tooltip("Drag handler for interaction")]
        private BannerDragHandler _dragHandler;

        [Header("Prefabs")]
        [SerializeField, Tooltip("Banner slide prefab (optional)")]
        private GameObject _slidePrefab;

        [SerializeField, Tooltip("Page indicator prefab (optional)")]
        private GameObject _indicatorPrefab;

        // ============================================
        // State
        // ============================================

        private int _currentIndex;
        private int _bannerCount;
        private bool _isInteracting;
        private bool _isAutoAdvancing;
        private bool _isInitialized;
        private Coroutine _autoAdvanceCoroutine;
        private Tween _scrollTween;
        private List<GameObject> _slideObjects = new List<GameObject>();
        private List<Image> _indicatorImages = new List<Image>();
        private BannerSlide[] _activeBanners;

        // ============================================
        // Constants
        // ============================================

        private const float SNAP_VELOCITY_THRESHOLD = 100f;
        private const float SNAP_POSITION_THRESHOLD = 0.1f;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initializes the carousel with configured banners.
        /// Call this from the owning screen's OnShow method.
        /// </summary>
        public void Initialize()
        {
            if (_bannerConfig == null)
            {
                Debug.LogWarning("[EventBannerCarousel] No banner config assigned - hiding carousel");
                gameObject.SetActive(false);
                return;
            }

            // Get active banners
            _activeBanners = _bannerConfig.GetActiveBannersArray();
            _bannerCount = _activeBanners.Length;

            if (_bannerCount == 0)
            {
                Debug.LogWarning("[EventBannerCarousel] No active banners - hiding carousel");
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Clear existing content
            ClearSlides();
            ClearIndicators();

            // Delay slide creation to next frame so layout is calculated
            StartCoroutine(InitializeDelayed());
        }

        private IEnumerator InitializeDelayed()
        {
            // Wait multiple frames for layout to fully calculate
            yield return null;
            yield return null;

            // Force canvas update
            Canvas.ForceUpdateCanvases();

            // Force layout rebuild on our hierarchy
            var carouselRect = GetComponent<RectTransform>();
            if (carouselRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(carouselRect);
            }

            if (_scrollRect != null && _scrollRect.viewport != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.viewport);
            }

            // Wait one more frame after rebuild
            yield return null;

            // Create slides and indicators
            CreateSlides();
            CreateIndicators();

            // Force another layout rebuild after creating slides
            if (_contentContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);
            }

            // Setup drag handler
            SetupDragHandler();

            // Reset state
            _currentIndex = 0;
            _isInteracting = false;
            _isInitialized = true;

            // Set initial position
            GoToSlide(0, false);

            // Start auto-advance
            if (_bannerConfig.IsAutoAdvanceEnabled() && _bannerCount > 1)
            {
                StartAutoAdvance();
            }

            Debug.Log($"[EventBannerCarousel] Initialized with {_bannerCount} banners");

            // Debug: Log hierarchy info
            LogDebugInfo();
        }

        private void LogDebugInfo()
        {
            var carouselRect = GetComponent<RectTransform>();
            Debug.Log($"[EventBannerCarousel] DEBUG - Carousel rect: {carouselRect.rect.width}x{carouselRect.rect.height}");

            if (_scrollRect != null)
            {
                Debug.Log($"[EventBannerCarousel] DEBUG - ScrollRect exists, viewport: {_scrollRect.viewport != null}");
                if (_scrollRect.viewport != null)
                {
                    Debug.Log($"[EventBannerCarousel] DEBUG - Viewport rect: {_scrollRect.viewport.rect.width}x{_scrollRect.viewport.rect.height}");
                }
                if (_scrollRect.content != null)
                {
                    Debug.Log($"[EventBannerCarousel] DEBUG - Content rect: {_scrollRect.content.rect.width}x{_scrollRect.content.rect.height}");
                    Debug.Log($"[EventBannerCarousel] DEBUG - Content childCount: {_scrollRect.content.childCount}");
                }
            }

            // Log each slide's info
            for (int i = 0; i < _slideObjects.Count; i++)
            {
                var slide = _slideObjects[i];
                if (slide != null)
                {
                    var slideRect = slide.GetComponent<RectTransform>();
                    var slideImage = slide.GetComponent<Image>();
                    Debug.Log($"[EventBannerCarousel] DEBUG - Slide {i}: rect={slideRect.rect.width}x{slideRect.rect.height}, sizeDelta={slideRect.sizeDelta}, sprite={(slideImage.sprite != null ? slideImage.sprite.name : "NULL")}, color={slideImage.color}");
                }
            }
        }

        // ============================================
        // Slide Creation
        // ============================================

        private void CreateSlides()
        {
            if (_contentContainer == null) return;

            for (int i = 0; i < _bannerCount; i++)
            {
                var banner = _activeBanners[i];
                GameObject slide = CreateSlideObject(banner, i);
                _slideObjects.Add(slide);
            }
        }

        private GameObject CreateSlideObject(BannerSlide banner, int index)
        {
            // Use local prefab first, then fall back to RuntimeUIPrefabConfig
            GameObject prefab = _slidePrefab;
            if (prefab == null)
            {
                var config = RuntimeUIPrefabConfigSO.Instance;
                if (config != null)
                {
                    prefab = config.BannerSlidePrefab;
                }
            }

            if (prefab == null)
            {
                Debug.LogError("[EventBannerCarousel] BannerSlidePrefab not assigned. Check RuntimeUIPrefabConfig.");
                return null;
            }

            var slideObj = Instantiate(prefab, _contentContainer);
            slideObj.name = $"Slide_{index}";

            // Configure slide from prefab
            ConfigureSlideFromPrefab(slideObj, banner, index);
            return slideObj;
        }

        /// <summary>
        /// Configures a slide instantiated from prefab.
        /// </summary>
        private void ConfigureSlideFromPrefab(GameObject slideObj, BannerSlide banner, int index)
        {
            // Get slide dimensions
            float slideWidth = 0f;
            float slideHeight = 0f;

            if (_scrollRect != null && _scrollRect.viewport != null)
            {
                slideWidth = _scrollRect.viewport.rect.width;
                slideHeight = _scrollRect.viewport.rect.height;
            }

            if (slideWidth <= 10f || slideHeight <= 10f)
            {
                var carouselRect = GetComponent<RectTransform>();
                if (carouselRect != null)
                {
                    slideWidth = carouselRect.rect.width;
                    slideHeight = carouselRect.rect.height * 0.85f;
                }
            }

            slideWidth = Mathf.Max(slideWidth, 200f);
            slideHeight = Mathf.Max(slideHeight, 80f);

            // Update RectTransform
            var rect = slideObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(slideWidth, slideHeight);
            }

            // Update LayoutElement
            var layout = slideObj.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredWidth = slideWidth;
                layout.preferredHeight = slideHeight;
                layout.minWidth = slideWidth;
                layout.minHeight = slideHeight;
            }

            // Update background image
            var bgImage = slideObj.GetComponent<Image>();
            if (bgImage != null)
            {
                if (banner.HasImage)
                {
                    bgImage.sprite = banner.Image;
                    bgImage.color = Color.white;
                }
                else
                {
                    bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
                }
            }

            // Update title text
            var titleTransform = slideObj.transform.Find("Title");
            if (titleTransform != null)
            {
                var titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = banner.Title;
                    titleText.gameObject.SetActive(!banner.HasImage);
                }
            }

            // Update description text
            var descTransform = slideObj.transform.Find("Description");
            if (descTransform != null)
            {
                var descText = descTransform.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                {
                    descText.text = banner.Description;
                    descText.gameObject.SetActive(!banner.HasImage);
                }
            }

            // Wire button click
            var button = slideObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSlideClicked(index));
            }
        }

        // ============================================
        // Indicator Creation
        // ============================================

        private void CreateIndicators()
        {
            if (_indicatorContainer == null || _bannerCount <= 1) return;

            for (int i = 0; i < _bannerCount; i++)
            {
                GameObject indicator = CreateIndicatorObject(i);
                _indicatorImages.Add(indicator.GetComponent<Image>());
            }

            UpdateIndicators();
        }

        private GameObject CreateIndicatorObject(int index)
        {
            float size = _bannerConfig?.IndicatorSize ?? 12f;

            // Use local prefab first, then fall back to RuntimeUIPrefabConfig
            GameObject prefab = _indicatorPrefab;
            if (prefab == null)
            {
                var config = RuntimeUIPrefabConfigSO.Instance;
                if (config != null)
                {
                    prefab = config.BannerIndicatorPrefab;
                }
            }

            if (prefab == null)
            {
                Debug.LogError("[EventBannerCarousel] BannerIndicatorPrefab not assigned. Check RuntimeUIPrefabConfig.");
                return null;
            }

            var indicatorObj = Instantiate(prefab, _indicatorContainer);
            indicatorObj.name = $"Indicator_{index}";

            var rect = indicatorObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(size, size);
            }

            var image = indicatorObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = _bannerConfig?.InactiveIndicatorColor ?? new Color(1f, 1f, 1f, 0.4f);
            }

            return indicatorObj;
        }

        private void UpdateIndicators()
        {
            if (_indicatorImages == null) return;

            for (int i = 0; i < _indicatorImages.Count; i++)
            {
                if (_indicatorImages[i] != null)
                {
                    bool isActive = i == _currentIndex;
                    _indicatorImages[i].color = isActive
                        ? (_bannerConfig?.ActiveIndicatorColor ?? Color.white)
                        : (_bannerConfig?.InactiveIndicatorColor ?? new Color(1f, 1f, 1f, 0.4f));
                }
            }
        }

        // ============================================
        // Navigation
        // ============================================

        /// <summary>
        /// Navigates to a specific slide.
        /// </summary>
        /// <param name="index">Target slide index</param>
        /// <param name="animate">Whether to animate the transition</param>
        public void GoToSlide(int index, bool animate = true)
        {
            if (!_isInitialized || _bannerCount == 0) return;

            // Handle looping
            if (_bannerConfig.EnableLoop)
            {
                if (index < 0) index = _bannerCount - 1;
                else if (index >= _bannerCount) index = 0;
            }
            else
            {
                index = Mathf.Clamp(index, 0, _bannerCount - 1);
            }

            _currentIndex = index;

            // Calculate target position (0 = first slide, 1 = last slide)
            float targetPosition = _bannerCount > 1
                ? (float)index / (_bannerCount - 1)
                : 0f;

            // Kill existing tween
            _scrollTween?.Kill();

            if (animate && _scrollRect != null)
            {
                float duration = _bannerConfig?.TransitionDuration ?? 0.3f;
                _scrollTween = DOTween.To(
                    () => _scrollRect.horizontalNormalizedPosition,
                    x => _scrollRect.horizontalNormalizedPosition = x,
                    targetPosition,
                    duration
                ).SetEase(Ease.OutCubic).SetLink(gameObject);
            }
            else if (_scrollRect != null)
            {
                _scrollRect.horizontalNormalizedPosition = targetPosition;
            }

            UpdateIndicators();
        }

        /// <summary>
        /// Advances to the next slide.
        /// </summary>
        public void NextSlide()
        {
            GoToSlide(_currentIndex + 1);
        }

        /// <summary>
        /// Goes to the previous slide.
        /// </summary>
        public void PreviousSlide()
        {
            GoToSlide(_currentIndex - 1);
        }

        /// <summary>
        /// Snaps to the nearest slide based on current scroll position.
        /// </summary>
        public void SnapToNearestSlide()
        {
            if (!_isInitialized || _scrollRect == null || _bannerCount <= 1) return;

            float currentPos = _scrollRect.horizontalNormalizedPosition;
            int nearestIndex = Mathf.RoundToInt(currentPos * (_bannerCount - 1));
            nearestIndex = Mathf.Clamp(nearestIndex, 0, _bannerCount - 1);

            GoToSlide(nearestIndex);
        }

        // ============================================
        // Auto-Advance
        // ============================================

        /// <summary>
        /// Starts the auto-advance coroutine.
        /// </summary>
        public void StartAutoAdvance()
        {
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
            }
            _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
            _isAutoAdvancing = true;
        }

        /// <summary>
        /// Pauses auto-advance (call during user interaction).
        /// </summary>
        public void PauseAutoAdvance()
        {
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }
            _isAutoAdvancing = false;
        }

        /// <summary>
        /// Resumes auto-advance after interaction.
        /// </summary>
        public void ResumeAutoAdvance()
        {
            if (!_bannerConfig.IsAutoAdvanceEnabled() || _bannerCount <= 1) return;

            // Use resume delay if configured
            float delay = _bannerConfig?.ResumeDelay ?? 1.5f;
            if (delay > 0)
            {
                StartCoroutine(ResumeAutoAdvanceAfterDelay(delay));
            }
            else
            {
                StartAutoAdvance();
            }
        }

        private IEnumerator ResumeAutoAdvanceAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            // Only resume if not currently interacting
            if (!_isInteracting)
            {
                StartAutoAdvance();
            }
        }

        private IEnumerator AutoAdvanceCoroutine()
        {
            while (true)
            {
                float interval = _bannerConfig?.AutoAdvanceInterval ?? 2f;
                yield return new WaitForSecondsRealtime(interval);

                // Only advance if not interacting
                if (!_isInteracting)
                {
                    NextSlide();
                }
            }
        }

        // ============================================
        // Drag Handling
        // ============================================

        private void SetupDragHandler()
        {
            if (_dragHandler == null) return;

            _dragHandler.ClearEvents();
            _dragHandler.OnDragBegin += OnDragBegin;
            _dragHandler.OnDragEnd += OnDragEnd;
            _dragHandler.OnTap += OnCarouselTap;
        }

        private void OnDragBegin()
        {
            _isInteracting = true;

            if (_bannerConfig != null && _bannerConfig.PauseOnInteraction)
            {
                PauseAutoAdvance();
            }
        }

        private void OnDragEnd()
        {
            _isInteracting = false;

            // Snap to nearest slide
            SnapToNearestSlide();

            // Resume auto-advance
            if (_bannerConfig != null && _bannerConfig.PauseOnInteraction)
            {
                ResumeAutoAdvance();
            }
        }

        private void OnCarouselTap()
        {
            // Tap on carousel area (not on specific slide)
            Debug.Log($"[EventBannerCarousel] Carousel tapped at index {_currentIndex}");
        }

        // ============================================
        // Slide Click Handling
        // ============================================

        private void OnSlideClicked(int index)
        {
            if (_activeBanners == null || index < 0 || index >= _activeBanners.Length) return;

            var banner = _activeBanners[index];
            Debug.Log($"[EventBannerCarousel] Banner {index} clicked: {banner.Title}");

            // Handle action if configured
            if (banner.HasAction)
            {
                ExecuteBannerAction(banner);
            }
        }

        private void ExecuteBannerAction(BannerSlide banner)
        {
            switch (banner.ActionType)
            {
                case BannerActionType.OpenURL:
                    Debug.Log($"[EventBannerCarousel] Would open URL: {banner.ActionParameter}");
                    // Application.OpenURL(banner.ActionParameter);
                    break;

                case BannerActionType.ChangeScene:
                    Debug.Log($"[EventBannerCarousel] Would change scene to: {banner.ActionParameter}");
                    // Publish scene change event
                    break;

                case BannerActionType.ShowModal:
                    Debug.Log($"[EventBannerCarousel] Would show modal: {banner.ActionParameter}");
                    // Show modal overlay
                    break;

                case BannerActionType.TriggerEvent:
                    Debug.Log($"[EventBannerCarousel] Would trigger event: {banner.ActionParameter}");
                    // Publish custom event
                    break;

                default:
                    Debug.Log($"[EventBannerCarousel] No action configured for banner");
                    break;
            }
        }

        // ============================================
        // Cleanup
        // ============================================

        private void ClearSlides()
        {
            foreach (var slide in _slideObjects)
            {
                if (slide != null)
                {
                    Destroy(slide);
                }
            }
            _slideObjects.Clear();
        }

        private void ClearIndicators()
        {
            foreach (var indicator in _indicatorImages)
            {
                if (indicator != null)
                {
                    Destroy(indicator.gameObject);
                }
            }
            _indicatorImages.Clear();
        }

        private void OnDisable()
        {
            PauseAutoAdvance();
            _scrollTween?.Kill();
        }

        private void OnDestroy()
        {
            PauseAutoAdvance();
            _scrollTween?.Kill();

            if (_dragHandler != null)
            {
                _dragHandler.ClearEvents();
            }
        }

        // ============================================
        // Public State
        // ============================================

        /// <summary>Current slide index</summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>Total number of slides</summary>
        public int SlideCount => _bannerCount;

        /// <summary>Whether carousel is initialized</summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>Whether auto-advance is currently running</summary>
        public bool IsAutoAdvancing => _isAutoAdvancing;

        /// <summary>Whether user is currently interacting</summary>
        public bool IsInteracting => _isInteracting;
    }
}
