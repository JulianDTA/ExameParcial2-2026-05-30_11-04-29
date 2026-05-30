// LevelCompleteScreen.cs
// Pantalla de progreso al pasar el nivel: anima el conteo de puntaje y ofrece "siguiente" o "hub".
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Juice;
using System.Collections;

namespace Game.UI
{
    public class LevelCompleteScreen : UIScreen
    {
        [Header("Refs")]
        [SerializeField] private TMP_Text headlineText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private UIJuice scoreJuice;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button hubButton;
        [SerializeField] private float countDuration = 0.6f;

        private Coroutine _countCoroutine;

        protected override void Awake()
        {
            base.Awake();
            if (nextButton != null) nextButton.onClick.AddListener(OnNext);
            if (hubButton != null) hubButton.onClick.AddListener(OnHub);
        }

        protected override void OnShow()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (headlineText != null)
                headlineText.text = gm.HasNextLevel ? "¡Nivel superado!" : "¡Juego completado!";

            if (nextButton != null)
            {
                var lbl = nextButton.GetComponentInChildren<TMP_Text>();
                if (lbl != null) lbl.text = gm.HasNextLevel ? "Siguiente" : "Finalizar";
            }

            // Feedback juicy de victoria
            if (JuiceManager.Instance != null) JuiceManager.Instance.AddTrauma(0.25f);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");

            if (_countCoroutine != null) StopCoroutine(_countCoroutine);
            _countCoroutine = StartCoroutine(CountUpScore(gm.Score));
        }

        private IEnumerator CountUpScore(int target)
        {
            float elapsed = 0f;
            int start = 0;
            while (elapsed < countDuration)
            {
                int value = Mathf.RoundToInt(Mathf.Lerp(start, target, elapsed / countDuration));
                if (scoreText != null) scoreText.text = value.ToString();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (scoreText != null) scoreText.text = target.ToString();
            if (scoreJuice != null) scoreJuice.PunchScale();
        }

        private void OnNext()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");
            GameManager.Instance.GoToNextLevel();
        }

        private void OnHub()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");
            GameManager.Instance.GoToHub();
        }
    }
}
