// ============================================
// SharedVitalityBar.cs
// Wide HP bar with embedded party portraits
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
    /// </summary>
    public class SharedVitalityBar : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform _barContainer;
        [SerializeField] private float _barWidth = 400f;
        [SerializeField] private float _barHeight = 40f;

        [Header("Embedded Portraits")]
        [SerializeField] private Image[] _partyPortraits;
        [SerializeField] private float _portraitSize = 32f;
        [SerializeField] private float _portraitOverlap = 8f;

        [Header("Health Display")]
        [SerializeField] private Image _healthFill;
        [SerializeField] private Image _damageFill;
        [SerializeField] private Image _healPreview;
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
        [SerializeField] private Color _healColor;
        [SerializeField] private Color _blockColor = new Color(0.2f, 0.6f, 0.86f, 1f); // #3498DB cyan

        private float _targetHealthFill;
        private Coroutine _damageLingerCoroutine;

        private void Awake()
        {
            _healthColor = UIColors.SoulCyan;
            _damageColor = UIColors.CorruptionGlow;
            _healColor = UIColors.NatureAspect;

            // Auto-wire references if not set in Inspector
            AutoWireReferences();

            if (_healthFill != null) _healthFill.color = _healthColor;
            if (_damageFill != null) _damageFill.color = _damageColor;
            if (_healPreview != null) _healPreview.color = _healColor;
            if (_shieldIcon != null) _shieldIcon.color = _blockColor;
            if (_blockText != null) _blockText.color = _blockColor;
        }

        private void AutoWireReferences()
        {
            // Auto-wire health bar elements
            if (_healthFill == null)
            {
                var healthFillT = transform.Find("HPBarContainer/HealthFill");
                _healthFill = healthFillT?.GetComponent<Image>();
            }
            if (_damageFill == null)
            {
                var damageFillT = transform.Find("HPBarContainer/DamageFill");
                _damageFill = damageFillT?.GetComponent<Image>();
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
            Debug.Log($"[SharedVitalityBar] Auto-wired: _healthFill={(_healthFill != null ? "OK" : "NULL")}, " +
                      $"_blockContainer={(_blockContainer != null ? "OK" : "NULL")}, " +
                      $"_shieldIcon={(_shieldIcon != null ? "OK" : "NULL")}, " +
                      $"_blockText={(_blockText != null ? "OK" : "NULL")}");
        }

        private void Start()
        {
            EventBus.Subscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Subscribe<BlockChangedEvent>(OnBlockChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Unsubscribe<BlockChangedEvent>(OnBlockChanged);
        }

        /// <summary>
        /// Sets the party portrait images from the team.
        /// </summary>
        /// <param name="team">Array of RequiemInstance for the party.</param>
        public void SetPartyPortraits(RequiemInstance[] team)
        {
            if (_partyPortraits == null || team == null) return;

            for (int i = 0; i < _partyPortraits.Length; i++)
            {
                if (i < team.Length && team[i]?.Data?.Portrait != null)
                {
                    _partyPortraits[i].sprite = team[i].Data.Portrait;
                    _partyPortraits[i].gameObject.SetActive(true);
                }
                else
                {
                    _partyPortraits[i].gameObject.SetActive(false);
                }
            }
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
            Debug.Log($"[SharedVitalityBar] UpdateHealth: {current}/{max}, newFill={newFill:F2}, _healthFill={(_healthFill != null ? "OK" : "NULL")}");

            // Damage taken - trigger linger effect
            if (newFill < _targetHealthFill)
            {
                if (_damageFill != null)
                {
                    _damageFill.fillAmount = _healthFill != null ? _healthFill.fillAmount : _targetHealthFill;
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
        /// </summary>
        /// <param name="healAmount">Amount of healing to preview.</param>
        /// <param name="currentHP">Current HP.</param>
        /// <param name="maxHP">Maximum HP.</param>
        public void ShowHealPreview(int healAmount, int currentHP, int maxHP)
        {
            if (_healPreview == null || maxHP <= 0) return;

            float currentFill = (float)currentHP / maxHP;
            float previewFill = Mathf.Min(1f, (float)(currentHP + healAmount) / maxHP);

            _healPreview.fillAmount = previewFill;
            _healPreview.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the heal preview overlay.
        /// </summary>
        public void HideHealPreview()
        {
            if (_healPreview != null)
            {
                _healPreview.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_healthFill != null)
            {
                _healthFill.fillAmount = Mathf.MoveTowards(
                    _healthFill.fillAmount, _targetHealthFill, _fillSpeed * Time.deltaTime);
            }
        }

        private IEnumerator LingerDamageFill(float targetFill)
        {
            yield return new WaitForSeconds(_damageLingerTime);

            while (_damageFill != null && _damageFill.fillAmount > targetFill)
            {
                _damageFill.fillAmount = Mathf.MoveTowards(
                    _damageFill.fillAmount, targetFill, _fillSpeed * Time.deltaTime);
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

            if (_healthFill != null) _healthFill.fillAmount = _targetHealthFill;
            if (_damageFill != null) _damageFill.fillAmount = _targetHealthFill;
            if (_hpText != null) _hpText.text = $"{currentHP} / {maxHP}";

            UpdateBlock(block);
        }
    }
}
