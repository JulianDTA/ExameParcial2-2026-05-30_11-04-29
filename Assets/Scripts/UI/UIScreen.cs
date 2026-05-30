// UIScreen.cs
// Clase base para pantallas (Hub, LevelComplete, Defeat). Maneja show/hide con fade + punch juicy.
using UnityEngine;
using Game.Juice;
using System.Collections;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private UIJuice rootJuice; // opcional: punch al panel principal

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            SetVisibleImmediate(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(Fade(1f, true));
            if (rootJuice != null) rootJuice.PunchScale();
            OnShow();
        }

        public void Hide()
        {
            if (!gameObject.activeInHierarchy) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(Fade(0f, false));
        }

        protected virtual void OnShow() { }

        private void SetVisibleImmediate(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
            gameObject.SetActive(visible);
        }

        private IEnumerator Fade(float target, bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
            float start = _canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                _canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _canvasGroup.alpha = target;
            if (target <= 0f) gameObject.SetActive(false);
        }
    }
}
