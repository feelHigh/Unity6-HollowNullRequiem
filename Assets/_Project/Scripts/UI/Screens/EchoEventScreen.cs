// ============================================
// EchoEventScreen.cs
// UI screen for displaying Echo narrative events
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.UI;

namespace HNR.Map
{
    /// <summary>
    /// UI screen for displaying Echo events with narrative and choices.
    /// Shows event text, choice buttons, and outcome results.
    /// </summary>
    public class EchoEventScreen : ScreenBase
    {
        // ============================================
        // UI References
        // ============================================

        [Header("Event Display")]
        [SerializeField, Tooltip("Event title text")]
        private TextMeshProUGUI _titleText;

        [SerializeField, Tooltip("Narrative/description text")]
        private TextMeshProUGUI _narrativeText;

        [SerializeField, Tooltip("Background illustration")]
        private Image _backgroundImage;

        [Header("Choices")]
        [SerializeField, Tooltip("Container for choice buttons")]
        private Transform _choiceContainer;

        [SerializeField, Tooltip("Prefab for choice buttons")]
        private Button _choiceButtonPrefab;

        [Header("Outcome")]
        [SerializeField, Tooltip("Panel shown after choice selection")]
        private GameObject _outcomePanel;

        [SerializeField, Tooltip("Outcome description text")]
        private TextMeshProUGUI _outcomeText;

        [SerializeField, Tooltip("Button to continue after viewing outcome")]
        private Button _continueButton;

        // ============================================
        // Runtime State
        // ============================================

        private readonly List<Button> _choiceButtons = new();
        private EchoEventManager _echoManager;
        private EchoEventDataSO _currentEvent;

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            // Get manager reference
            _echoManager = ServiceLocator.Get<EchoEventManager>();

            // Subscribe to events
            EventBus.Subscribe<EchoEventStartedEvent>(OnEventStarted);
            EventBus.Subscribe<EchoChoiceSelectedEvent>(OnChoiceSelected);

            // Setup continue button
            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            // Hide outcome panel initially
            if (_outcomePanel != null)
            {
                _outcomePanel.SetActive(false);
            }

            // Display current event if already started
            if (_echoManager?.CurrentEvent != null)
            {
                DisplayEvent(_echoManager.CurrentEvent);
            }
        }

        public override void OnHide()
        {
            base.OnHide();

            // Unsubscribe from events
            EventBus.Unsubscribe<EchoEventStartedEvent>(OnEventStarted);
            EventBus.Unsubscribe<EchoChoiceSelectedEvent>(OnChoiceSelected);

            // Cleanup
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(OnContinueClicked);
            }

            ClearChoices();
            _currentEvent = null;
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnEventStarted(EchoEventStartedEvent evt)
        {
            DisplayEvent(evt.Event);
        }

        private void OnChoiceSelected(EchoChoiceSelectedEvent evt)
        {
            ShowOutcome(evt.Choice);
        }

        // ============================================
        // Display Methods
        // ============================================

        private void DisplayEvent(EchoEventDataSO eventData)
        {
            if (eventData == null)
            {
                Debug.LogWarning("[EchoEventScreen] Cannot display null event");
                return;
            }

            _currentEvent = eventData;

            // Set title
            if (_titleText != null)
            {
                _titleText.text = eventData.EventTitle;
            }

            // Set narrative
            if (_narrativeText != null)
            {
                _narrativeText.text = eventData.Narrative;
            }

            // Set background image
            if (_backgroundImage != null && eventData.BackgroundImage != null)
            {
                _backgroundImage.sprite = eventData.BackgroundImage;
                _backgroundImage.enabled = true;
            }
            else if (_backgroundImage != null)
            {
                _backgroundImage.enabled = false;
            }

            // Create choice buttons
            ClearChoices();
            CreateChoiceButtons(eventData);

            // Hide outcome panel
            if (_outcomePanel != null)
            {
                _outcomePanel.SetActive(false);
            }

            Debug.Log($"[EchoEventScreen] Displaying event: {eventData.EventTitle}");
        }

        private void CreateChoiceButtons(EchoEventDataSO eventData)
        {
            if (_choiceButtonPrefab == null || _choiceContainer == null)
            {
                Debug.LogWarning("[EchoEventScreen] Choice button prefab or container not set");
                return;
            }

            for (int i = 0; i < eventData.Choices.Count; i++)
            {
                var choice = eventData.Choices[i];
                CreateChoiceButton(choice, i);
            }
        }

        private void CreateChoiceButton(EchoChoice choice, int index)
        {
            var button = Instantiate(_choiceButtonPrefab, _choiceContainer);

            // Set button text
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.ChoiceText;
            }

            // Add click listener (capture index by value)
            int choiceIndex = index;
            button.onClick.AddListener(() => OnChoiceClicked(choiceIndex));

            _choiceButtons.Add(button);
        }

        private void ClearChoices()
        {
            foreach (var button in _choiceButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    Destroy(button.gameObject);
                }
            }
            _choiceButtons.Clear();
        }

        private void ShowOutcome(EchoChoice choice)
        {
            // Disable choice buttons
            foreach (var button in _choiceButtons)
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            }

            // Show outcome panel
            if (_outcomePanel != null)
            {
                _outcomePanel.SetActive(true);

                if (_outcomeText != null)
                {
                    _outcomeText.text = choice.OutcomeText;
                }
            }

            Debug.Log($"[EchoEventScreen] Showing outcome: {choice.OutcomeText}");
        }

        // ============================================
        // Button Callbacks
        // ============================================

        private void OnChoiceClicked(int index)
        {
            if (_echoManager == null)
            {
                Debug.LogWarning("[EchoEventScreen] EchoEventManager not available");
                return;
            }

            Debug.Log($"[EchoEventScreen] Choice selected: {index}");
            _echoManager.SelectChoice(index);
        }

        private void OnContinueClicked()
        {
            // Complete the event
            _echoManager?.CompleteEvent();

            // Return to map screen
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<MapScreen>();
            }
            else
            {
                Debug.LogWarning("[EchoEventScreen] UIManager not available for screen transition");
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Displays the specified event directly.
        /// </summary>
        public void ShowEvent(EchoEventDataSO eventData)
        {
            DisplayEvent(eventData);
        }
    }
}
