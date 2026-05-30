// LevelController.cs
// Controlador de nivel de ejemplo: expone Win()/Fail() que conectan con GameManager y disparan juiciness.
// Reemplaza la lógica de prueba por tu jugabilidad real.
using UnityEngine;
using Game.Juice;

namespace Game.Core
{
    public class LevelController : MonoBehaviour
    {
        [Header("Config de prueba")]
        [SerializeField] private int rewardScore = 100;

        // Llamar cuando el jugador cumple el objetivo del nivel.
        public void Win()
        {
            if (JuiceManager.Instance != null) JuiceManager.Instance.AddTrauma(0.3f);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");
            GameManager.Instance.CompleteLevel(rewardScore);
        }

        // Llamar en un error recuperable (resta vida; derrota si llega a 0).
        public void Fail()
        {
            if (JuiceManager.Instance != null) JuiceManager.Instance.TriggerHeavyImpact(0.5f);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("prueba");
            GameManager.Instance.RegisterFailure();
        }

        // Llamar en derrota inmediata (game over).
        public void GameOver()
        {
            GameManager.Instance.Lose();
        }
    }
}
