// ============================================
// EchoEventScreen.cs
// UI screen for displaying Echo narrative events
// ============================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Cards;
using HNR.UI;
using HNR.UI.Components;

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

        [Header("Navigation")]
        [SerializeField, Tooltip("Button to skip/close event (shown when no choices available)")]
        private Button _skipButton;

        [Header("Card Selection")]
        [SerializeField, Tooltip("Deck viewer modal for card upgrade selection")]
        private DeckViewerModal _deckViewerModal;

        [Header("Outcome Card Display")]
        [SerializeField, Tooltip("Container for outcome card display in outcome panel")]
        private Transform _outcomeCardContainer;

        [SerializeField, Tooltip("Card prefab for outcome display")]
        private Card _cardPrefab;

        // ============================================
        // Runtime State
        // ============================================

        private readonly List<Button> _choiceButtons = new();
        private EchoEventManager _echoManager;
        private EchoEventDataSO _currentEvent;
        private int _pendingChoiceIndex = -1;

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

            // Setup skip button
            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(OnSkipClicked);
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
            else
            {
                // No event - show empty state with skip option
                ShowEmptyState();
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

            if (_skipButton != null)
            {
                _skipButton.onClick.RemoveListener(OnSkipClicked);
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

            // Show skip button if no choices available, hide otherwise
            if (_skipButton != null)
            {
                bool hasChoices = eventData.Choices != null && eventData.Choices.Count > 0;
                _skipButton.gameObject.SetActive(!hasChoices);
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

            // Ensure instantiated button is active (template may be inactive)
            button.gameObject.SetActive(true);

            // Set button text with choice and outcome preview
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string outcomePreview = FormatOutcomePreview(choice.Outcomes);
                if (!string.IsNullOrEmpty(outcomePreview))
                {
                    buttonText.text = $"{choice.ChoiceText}\n<size=80%><color=#AAAAAA>{outcomePreview}</color></size>";
                }
                else
                {
                    buttonText.text = choice.ChoiceText;
                }
            }

            // Improve button visual clarity with better sizing
            var rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Ensure button has enough height for text + outcome
                var layoutElement = button.GetComponent<UnityEngine.UI.LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                }
                layoutElement.minHeight = 60f;
                layoutElement.preferredHeight = 80f;
            }

            // Add click listener (capture index by value)
            int choiceIndex = index;
            button.onClick.AddListener(() => OnChoiceClicked(choiceIndex));

            _choiceButtons.Add(button);
        }

        /// <summary>
        /// Formats the outcomes into a readable preview string.
        /// </summary>
        private string FormatOutcomePreview(System.Collections.Generic.IReadOnlyList<EchoOutcome> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0)
                return string.Empty;

            var parts = new System.Collections.Generic.List<string>();

            foreach (var outcome in outcomes)
            {
                string part = FormatSingleOutcome(outcome);
                if (!string.IsNullOrEmpty(part))
                {
                    parts.Add(part);
                }
            }

            return string.Join(" | ", parts);
        }

        /// <summary>
        /// Formats a single outcome into readable text.
        /// </summary>
        private string FormatSingleOutcome(EchoOutcome outcome)
        {
            switch (outcome.Type)
            {
                case EchoOutcomeType.None:
                    return string.Empty;

                case EchoOutcomeType.GainGold:
                    return $"+{outcome.Value} Shards";

                case EchoOutcomeType.LoseGold:
                    return $"-{outcome.Value} Shards";

                case EchoOutcomeType.GainHP:
                    return $"+{outcome.Value} HP";

                case EchoOutcomeType.LoseHP:
                    return $"-{outcome.Value} HP";

                case EchoOutcomeType.GainMaxHP:
                    return $"+{outcome.Value} Max HP";

                case EchoOutcomeType.LoseMaxHP:
                    return $"-{outcome.Value} Max HP";

                case EchoOutcomeType.GainCorruption:
                    return $"+{outcome.Value} Corruption";

                case EchoOutcomeType.LoseCorruption:
                    return $"-{outcome.Value} Corruption";

                case EchoOutcomeType.GainCard:
                    return "+1 Card";

                case EchoOutcomeType.RemoveCard:
                    return "-1 Card";

                case EchoOutcomeType.UpgradeCard:
                    return "Upgrade Card";

                case EchoOutcomeType.GainRelic:
                    return "+1 Relic";

                case EchoOutcomeType.GainRandomCard:
                    return "+Random Card";

                case EchoOutcomeType.GainRandomRelic:
                    return "+Random Relic";

                case EchoOutcomeType.StartCombat:
                    return "Fight!";

                default:
                    return string.Empty;
            }
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

            // Display outcome card if any card was added
            DisplayOutcomeCard();

            Debug.Log($"[EchoEventScreen] Showing outcome: {choice.OutcomeText}");
        }

        /// <summary>
        /// Displays the outcome card in the outcome panel if one was added.
        /// </summary>
        private void DisplayOutcomeCard()
        {
            // Clear previous card
            if (_outcomeCardContainer != null)
            {
                foreach (Transform child in _outcomeCardContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Check if we have a card to display
            var outcomeCard = _echoManager?.LastOutcomeCard;
            if (outcomeCard == null || _outcomeCardContainer == null)
            {
                return;
            }

            // Instantiate and display card
            if (_cardPrefab != null)
            {
                var card = Instantiate(_cardPrefab, _outcomeCardContainer);
                card.SetCard(outcomeCard);
                card.transform.localScale = Vector3.one * 0.8f; // Scale down for panel
                Debug.Log($"[EchoEventScreen] Displaying outcome card: {outcomeCard.CardName}");
            }
            else
            {
                Debug.Log($"[EchoEventScreen] Card prefab not assigned, cannot display outcome card: {outcomeCard.CardName}");
            }
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

            // Check if this choice has an UpgradeCard outcome
            if (_currentEvent != null && index < _currentEvent.Choices.Count)
            {
                var choice = _currentEvent.Choices[index];
                bool hasUpgradeOutcome = choice.Outcomes.Any(o => o.Type == EchoOutcomeType.UpgradeCard);

                if (hasUpgradeOutcome)
                {
                    // Show card selection modal before applying outcomes
                    ShowCardUpgradeModal(index);
                    return;
                }
            }

            // No upgrade outcome, proceed normally
            _echoManager.SelectChoice(index);
        }

        private void ShowCardUpgradeModal(int choiceIndex)
        {
            // Find or use assigned DeckViewerModal
            var modal = _deckViewerModal;
            if (modal == null)
            {
                modal = FindAnyObjectByType<DeckViewerModal>(FindObjectsInactive.Include);
            }

            if (modal == null)
            {
                Debug.LogWarning("[EchoEventScreen] DeckViewerModal not found - falling back to random upgrade");
                _echoManager.SelectChoice(choiceIndex);
                return;
            }

            // Store pending choice to complete after modal
            _pendingChoiceIndex = choiceIndex;

            // Show deck viewer in upgrade mode
            modal.Show(DeckViewerModal.ViewMode.UpgradeCard, (upgradedCard) =>
            {
                // Whether upgrade was selected or cancelled, proceed with the choice
                // The modal already handled the upgrade via RunManager
                int pendingIndex = _pendingChoiceIndex;
                _pendingChoiceIndex = -1;

                if (upgradedCard != null)
                {
                    Debug.Log($"[EchoEventScreen] Card upgraded: {upgradedCard.CardName}");
                    // Skip the manager's UpgradeRandomCard by using a different approach
                    // Actually apply all OTHER outcomes (non-upgrade) through the manager
                }

                // Complete the choice (outcomes are applied here, but upgrade was already done)
                _echoManager.SelectChoiceWithoutUpgrade(pendingIndex);
            });

            Debug.Log("[EchoEventScreen] Showing card upgrade selection modal");
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

        private void OnSkipClicked()
        {
            Debug.Log("[EchoEventScreen] Skip clicked - returning to map");

            // Complete node without event resolution
            if (ServiceLocator.TryGet<MapManager>(out var mapManager))
            {
                mapManager.CompleteCurrentNode();
            }

            // Return to map screen
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<MapScreen>();
            }
        }

        // ============================================
        // Empty State
        // ============================================

        private void ShowEmptyState()
        {
            // Set title to indicate no event
            if (_titleText != null)
            {
                _titleText.text = "ECHO FADES...";
            }

            // Set narrative
            if (_narrativeText != null)
            {
                _narrativeText.text = "The whispers of the void grow silent. No echoes remain to be heard in this place.";
            }

            // Hide background image
            if (_backgroundImage != null)
            {
                _backgroundImage.enabled = false;
            }

            // Clear any choices
            ClearChoices();

            // Show skip button, hide outcome panel
            if (_skipButton != null)
            {
                _skipButton.gameObject.SetActive(true);
            }

            if (_outcomePanel != null)
            {
                _outcomePanel.SetActive(false);
            }

            Debug.Log("[EchoEventScreen] Showing empty state - no event available");
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
