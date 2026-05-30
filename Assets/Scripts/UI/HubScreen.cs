// HubScreen.cs
// Pantalla central (lobby): muestra score total y botones de nivel que respetan el progreso desbloqueado.
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Juice;

namespace Game.UI
{
    public class HubScreen : UIScreen
    {
        [Header("Refs")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Button[] levelButtons; // 1 por nivel, en orden

        protected override void OnShow()
        {
            if (GameManager.Instance == null) return;
            RefreshScore(GameManager.Instance.Score);
            RefreshButtons();
        }

        void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged += RefreshScore;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged -= RefreshScore;
        }

        private void RefreshScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Puntaje: {score}";
        }

        private void RefreshButtons()
        {
            var gm = GameManager.Instance;
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int level = i + 1;
                var btn = levelButtons[i];
                if (btn == null) continue;

                bool unlocked = level <= gm.HighestUnlocked;
                btn.interactable = unlocked;

                var label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = unlocked ? level.ToString() : "🔒";

                btn.onClick.RemoveAllListeners();
                if (unlocked)
                {
                    int captured = level;
                    btn.onClick.AddListener(() =>
                    {
                        if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");
                        gm.StartLevel(captured);
                    });
                }
            }
        }
    }
}
