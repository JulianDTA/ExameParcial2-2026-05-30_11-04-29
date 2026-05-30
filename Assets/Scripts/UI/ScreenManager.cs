// ScreenManager.cs
// Escucha GameManager.OnStateChanged y muestra/oculta la pantalla correcta. Desacopla UI de lógica.
using UnityEngine;
using Game.Core;

namespace Game.UI
{
    public class ScreenManager : MonoBehaviour
    {
        [Header("Pantallas")]
        [SerializeField] private UIScreen hubScreen;
        [SerializeField] private UIScreen levelCompleteScreen; // pantalla de progreso (victoria)
        [SerializeField] private UIScreen defeatScreen;
        [SerializeField] private GameObject gameplayHud;       // HUD visible solo en Playing

        void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                HandleStateChanged(GameManager.Instance.State);
            }
        }

        void Start()
        {
            // Asegura suscripción si GameManager se inicializa después.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                HandleStateChanged(GameManager.Instance.State);
            }
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            HideAll();
            switch (state)
            {
                case GameState.Hub:
                    if (hubScreen != null) hubScreen.Show();
                    break;
                case GameState.Playing:
                    if (gameplayHud != null) gameplayHud.SetActive(true);
                    break;
                case GameState.LevelComplete:
                    if (levelCompleteScreen != null) levelCompleteScreen.Show();
                    break;
                case GameState.Defeat:
                    if (defeatScreen != null) defeatScreen.Show();
                    break;
            }
        }

        private void HideAll()
        {
            if (hubScreen != null) hubScreen.Hide();
            if (levelCompleteScreen != null) levelCompleteScreen.Hide();
            if (defeatScreen != null) defeatScreen.Hide();
            if (gameplayHud != null) gameplayHud.SetActive(false);
        }
    }
}
