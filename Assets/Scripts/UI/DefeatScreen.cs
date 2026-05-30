// DefeatScreen.cs
// Pantalla de derrota: feedback de impacto pesado (shake + hit stop + post FX) y botones reintentar/hub.
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Juice;

namespace Game.UI
{
    public class DefeatScreen : UIScreen
    {
        [Header("Refs")]
        [SerializeField] private TMP_Text headlineText;
        [SerializeField] private UIJuice headlineJuice;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button hubButton;

        protected override void Awake()
        {
            base.Awake();
            if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
            if (hubButton != null) hubButton.onClick.AddListener(OnHub);
        }

        protected override void OnShow()
        {
            if (headlineText != null) headlineText.text = "Derrota";

            // Golpe de juiciness fuerte para vender la derrota.
            if (JuiceManager.Instance != null) JuiceManager.Instance.TriggerHeavyImpact(0.7f);
            if (headlineJuice != null) headlineJuice.ShakeHorizontal(14f, 0.4f);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("prueba");
        }

        private void OnRetry()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");
            GameManager.Instance.RetryLevel();
        }

        private void OnHub()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");
            GameManager.Instance.GoToHub();
        }
    }
}
