// UIJuice.cs
// Animaciones de feedback para UI: punch scale (Bézier), color flash (TMP) y shake horizontal.
using UnityEngine;
using TMPro;
using System.Collections;

namespace Game.Juice
{
    [RequireComponent(typeof(RectTransform))]
    public class UIJuice : MonoBehaviour
    {
        [Header("Punch Scale")]
        [SerializeField] private float punchDuration = 0.25f;
        [SerializeField] private float punchStrength = 0.2f;

        [Header("Color Flash (TextMeshPro)")]
        [SerializeField] private TMP_Text tmpText;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.2f;

        private RectTransform _rect;
        private Vector3 _originalScale;
        private Color _originalTextColor;
        private Coroutine _scaleCoroutine;
        private Coroutine _colorCoroutine;
        private Coroutine _shakeCoroutine;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _originalScale = _rect.localScale;
            if (tmpText != null) _originalTextColor = tmpText.color;
        }

        // ── Punch Scale ─────────────────────────────────────────────────────
        public void PunchScale()
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(PunchScaleRoutine());
        }

        private IEnumerator PunchScaleRoutine()
        {
            float elapsed = 0f;
            while (elapsed < punchDuration)
            {
                float t = elapsed / punchDuration;
                float scale = BezierBounce(t) * punchStrength;
                _rect.localScale = _originalScale * (1f + scale);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _rect.localScale = _originalScale;
        }

        // Bézier cúbico con overshoot: sube, sobrepasa, asienta. Centrado en 0.
        private float BezierBounce(float t)
        {
            float u = 1f - t;
            return 3f * u * u * t * 1.4f
                 + 3f * u * t * t * 0.9f
                 + t * t * t * 1f
                 - 1f;
        }

        // ── Color Flash (TMP) ───────────────────────────────────────────────
        public void FlashColor()
        {
            if (tmpText == null) return;
            if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
            _colorCoroutine = StartCoroutine(FlashColorRoutine());
        }

        private IEnumerator FlashColorRoutine()
        {
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                float t = elapsed / flashDuration;
                tmpText.color = Color.Lerp(flashColor, _originalTextColor, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            tmpText.color = _originalTextColor;
        }

        // ── Shake horizontal (para inputs incorrectos / derrota) ────────────
        public void ShakeHorizontal(float magnitude = 10f, float duration = 0.3f)
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeRoutine(magnitude, duration));
        }

        private IEnumerator ShakeRoutine(float magnitude, float duration)
        {
            Vector2 origin = _rect.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float dampen = 1f - t;
                float offsetX = Mathf.Sin(elapsed * 60f) * magnitude * dampen;
                _rect.anchoredPosition = origin + new Vector2(offsetX, 0f);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _rect.anchoredPosition = origin;
        }
    }
}
