// ============================================
// SharedVitalityBar.cs
// Wide HP bar with embedded party portraits
// Uses Slider component for reliable gauge updates
// Layout with damage linger effect
// ============================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

#pragma warning disable CS0414 // Field is assigned but never used (reserved for future Inspector configuration)

namespace HNR.UI.Combat
{
    /// <summary>
    /// Wide HP bar with embedded party portraits.
    /// Shows team HP with damage linger effect and block indicator.
    /// Uses Slider component for reliable visual updates.
    /// </summary>
    public class SharedVitalityBar : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform _barContainer;
        [SerializeField] private float _barWidth = 400f;
        [SerializeField] private float _barHeight = 40f;

        [Header("Embedded Portraits")]
        [SerializeField] private PortraitCorruptionSlot[] _portraitSlots;
        [SerializeField] private RectTransform _portraitContainer;
        [SerializeField] private float _portraitSize = 48f;
        [SerializeField] private float _portraitSpacing = 8f;

        [Header("Health Slider")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Slider _damageSlider;
        [SerializeField] private Image _damageFillImage;
        [SerializeField] private TMP_Text _hpText;

        [Header("Block Indicator")]
        [SerializeField] private GameObject _blockContainer;
        [SerializeField] private Image _shieldIcon;
        [SerializeField] private TMP_Text _blockText;

        [Header("Animation")]
        [SerializeField] private float _fillSpeed = 5f;
        [SerializeField] private float _damageLingerTime = 0.5f;
        [SerializeField] private float _shakeIntensity = 5f;

        [Header("Colors")]
        [SerializeField] private Color _healthColor;
        [SerializeField] private Color _damageColor;
        [SerializeField] private Color _blockColor = new Color(0.2f, 0.6f, 0.86f, 1f); // #3498DB cyan

        private float _targetHealthFill;
        private Coroutine _damageLingerCoroutine;

        private void Awake()
        {
            _healthColor = UIColors.SoulCyan;
            _damageColor = UIColors.CorruptionGlow;

            // Auto-wire references if not set in Inspector
            AutoWireReferences();

            // Configure sliders
            ConfigureSliders();

            if (_healthFillImage != null) _healthFillImage.color = _healthColor;
            if (_damageFillImage != null) _damageFillImage.color = _damageColor;
            if (_shieldIcon != null) _shieldIcon.color = _blockColor;
            if (_blockText != null) _blockText.color = _blockColor;
        }

        private void ConfigureSliders()
        {
            if (_healthSlider != null)
            {
                _healthSlider.minValue = 0f;
                _healthSlider.maxValue = 1f;
                _healthSlider.value = 1f;
                _healthSlider.interactable = false;
                Debug.Log("[SharedVitalityBar] Health slider configured");
            }

            if (_damageSlider != null)
            {
                _damageSlider.minValue = 0f;
                _damageSlider.maxValue = 1f;
                _damageSlider.value = 1f;
                _damageSlider.interactable = false;
                Debug.Log("[SharedVitalityBar] Damage slider configured");
            }
        }

        private void AutoWireReferences()
        {
            // Auto-wire health slider elements
            if (_healthSlider == null)
            {
                var healthSliderT = transform.Find("HPBarContainer/HealthSlider");
                _healthSlider = healthSliderT?.GetComponent<Slider>();
            }
            if (_damageSlider == null)
            {
                var damageSliderT = transform.Find("HPBarContainer/DamageSlider");
                _damageSlider = damageSliderT?.GetComponent<Slider>();
            }
            if (_hpText == null)
            {
                var hpTextT = transform.Find("HPBarContainer/HPText");
                _hpText = hpTextT?.GetComponent<TMP_Text>();
            }

            // Auto-wire block indicator elements
            if (_blockContainer == null)
            {
                var blockT = transform.Find("BlockContainer");
                _blockContainer = blockT?.gameObject;
            }
            if (_shieldIcon == null)
            {
                var shieldT = transform.Find("BlockContainer/ShieldIcon");
                _shieldIcon = shieldT?.GetComponent<Image>();
            }
            if (_blockText == null)
            {
                var blockTextT = transform.Find("BlockContainer/BlockText");
                _blockText = blockTextT?.GetComponent<TMP_Text>();
            }

            // Log wiring status
            Debug.Log($"[SharedVitalityBar] Auto-wired: _healthSlider={(_healthSlider != null ? "OK" : "NULL")}, " +
                      $"_damageSlider={(_damageSlider != null ? "OK" : "NULL")}, " +
                      $"_blockContainer={(_blockContainer != null ? "OK" : "NULL")}");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Subscribe<BlockChangedEvent>(OnBlockChanged);
            Debug.Log("[SharedVitalityBar] Subscribed to HP and Block events");
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Unsubscribe<BlockChangedEvent>(OnBlockChanged);
            Debug.Log("[SharedVitalityBar] Unsubscribed from HP and Block events");
        }

        /// <summary>
        /// Sets the party portraits and initializes corruption tracking from the team.
        /// </summary>
        /// <param name="team">Array of RequiemInstance for the party.</param>
        public void SetPartyPortraits(RequiemInstance[] team)
        {
            if (_portraitSlots == null || team == null) return;

            for (int i = 0; i < _portraitSlots.Length; i++)
            {
                if (i < team.Length && team[i] != null)
                {
                    _portraitSlots[i].Initialize(team[i]);
                    _portraitSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    _portraitSlots[i].gameObject.SetActive(false);
                }
            }

            Debug.Log($"[SharedVitalityBar] Initialized {team.Length} portrait slots with corruption tracking");
        }

        private void OnTeamHPChanged(TeamHPChangedEvent evt)
        {
            Debug.Log($"[SharedVitalityBar] TeamHPChangedEvent received: {evt.CurrentHP}/{evt.MaxHP} (delta: {evt.Delta})");
            UpdateHealth(evt.CurrentHP, evt.MaxHP, evt.Delta);
        }

        private void OnBlockChanged(BlockChangedEvent evt)
        {
            Debug.Log($"[SharedVitalityBar] BlockChangedEvent received: block={evt.Block}, _blockContainer={(_blockContainer != null ? "OK" : "NULL")}");
            UpdateBlock(evt.Block);
        }

        /// <summary>
        /// Updates the health bar display with optional damage linger effect.
        /// </summary>
        /// <param name="current">Current HP.</param>
        /// <param name="max">Maximum HP.</param>
        /// <param name="delta">Change in HP (negative = damage).</param>
        public void UpdateHealth(int current, int max, int delta = 0)
        {
            float newFill = max > 0 ? (float)current / max : 0;
            Debug.Log($"[SharedVitalityBar] UpdateHealth: {current}/{max}, newFill={newFill:F2}, _healthSlider={(_healthSlider != null ? "OK" : "NULL")}");

            // Damage taken - trigger linger effect
            if (newFill < _targetHealthFill)
            {
                if (_damageSlider != null)
                {
                    _damageSlider.value = _healthSlider != null ? _healthSlider.value : _targetHealthFill;
                }

                if (_damageLingerCoroutine != null)
                {
                    StopCoroutine(_damageLingerCoroutine);
                }
                _damageLingerCoroutine = StartCoroutine(LingerDamageFill(newFill));

                TriggerDamageShake();
            }

            _targetHealthFill = newFill;

            if (_hpText != null)
            {
                _hpText.text = $"{current} / {max}";
            }
        }

        /// <summary>
        /// Updates the block indicator display.
        /// </summary>
        /// <param name="block">Current block value.</param>
        public void UpdateBlock(int block)
        {
            if (_blockContainer != null)
            {
                _blockContainer.SetActive(block > 0);
            }

            if (block > 0)
            {
                if (_blockText != null)
                {
                    _blockText.text = block.ToString();
                }

                if (_shieldIcon != null)
                {
                    _shieldIcon.transform.DOScale(1.2f, 0.1f)
                        .OnComplete(() => _shieldIcon.transform.DOScale(1f, 0.1f).SetLink(gameObject))
                        .SetLink(gameObject);
                }
            }
        }

        /// <summary>
        /// Shows a preview of incoming healing on the bar.
        /// Note: Heal preview not implemented with slider system - consider future enhancement.
        /// </summary>
        /// <param name="healAmount">Amount of healing to preview.</param>
        /// <param name="currentHP">Current HP.</param>
        /// <param name="maxHP">Maximum HP.</param>
        public void ShowHealPreview(int healAmount, int currentHP, int maxHP)
        {
            // Heal preview visualization could be added as a third slider layer if needed
            Debug.Log($"[SharedVitalityBar] ShowHealPreview: +{healAmount} HP");
        }

        /// <summary>
        /// Hides the heal preview overlay.
        /// </summary>
        public void HideHealPreview()
        {
            // Heal preview visualization could be added as a third slider layer if needed
        }

        private void Update()
        {
            // Smoothly update health slider toward target
            if (_healthSlider != null)
            {
                _healthSlider.value = Mathf.MoveTowards(
                    _healthSlider.value, _targetHealthFill, _fillSpeed * Time.deltaTime);
            }
        }

        private IEnumerator LingerDamageFill(float targetFill)
        {
            yield return new WaitForSeconds(_damageLingerTime);

            // Smoothly reduce damage slider to match health
            while (_damageSlider != null && _damageSlider.value > targetFill)
            {
                _damageSlider.value = Mathf.MoveTowards(
                    _damageSlider.value, targetFill, _fillSpeed * Time.deltaTime);
                yield return null;
            }

            _damageLingerCoroutine = null;
        }

        /// <summary>
        /// Triggers a shake animation on damage.
        /// </summary>
        public void TriggerDamageShake()
        {
            transform.DOShakePosition(0.3f, _shakeIntensity).SetLink(gameObject);
        }

        /// <summary>
        /// Initializes the bar with starting values.
        /// </summary>
        /// <param name="currentHP">Starting current HP.</param>
        /// <param name="maxHP">Starting max HP.</param>
        /// <param name="block">Starting block value.</param>
        public void Initialize(int currentHP, int maxHP, int block = 0)
        {
            _targetHealthFill = maxHP > 0 ? (float)currentHP / maxHP : 0;

            if (_healthSlider != null) _healthSlider.value = _targetHealthFill;
            if (_damageSlider != null) _damageSlider.value = _targetHealthFill;
            if (_hpText != null) _hpText.text = $"{currentHP} / {maxHP}";

            UpdateBlock(block);
        }
    }
}
