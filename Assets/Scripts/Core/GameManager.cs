// GameManager.cs
// Máquina de estados central del juego: Hub -> Playing -> LevelComplete / Defeat.
// Event-driven: las pantallas de UI se suscriben a OnStateChanged sin acoplarse al manager.
using UnityEngine;
using System;

namespace Game.Core
{
    public enum GameState
    {
        Hub,            // Menú/lobby central, selección de nivel
        Playing,        // En partida
        LevelComplete,  // Pantalla de progreso al pasar el nivel
        Defeat          // Pantalla de derrota al fallar/perder
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Progresión")]
        [SerializeField] private int totalLevels = 9;   // coincide con BtnColor1..9
        [SerializeField] private int startingLives = 3;

        public GameState State { get; private set; } = GameState.Hub;
        public int CurrentLevel { get; private set; } = 1;
        public int HighestUnlocked { get; private set; } = 1;
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int TotalLevels => totalLevels;

        // Eventos para desacoplar UI / audio / VFX
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnLevelStarted;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Lives = startingLives;
        }

        void Start() => SetState(GameState.Hub);

        // ── Transiciones de estado ──────────────────────────────────────────
        private void SetState(GameState next)
        {
            State = next;
            OnStateChanged?.Invoke(next);
        }

        public void GoToHub()
        {
            Time.timeScale = 1f;
            SetState(GameState.Hub);
        }

        public void StartLevel(int level)
        {
            if (level < 1 || level > totalLevels) return;
            if (level > HighestUnlocked) return; // nivel bloqueado

            CurrentLevel = level;
            Lives = startingLives;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            OnLevelStarted?.Invoke(level);
        }

        // Llamar desde la lógica del nivel cuando el jugador gana.
        public void CompleteLevel(int earnedScore = 100)
        {
            AddScore(earnedScore);
            HighestUnlocked = Mathf.Clamp(Mathf.Max(HighestUnlocked, CurrentLevel + 1), 1, totalLevels);
            SetState(GameState.LevelComplete);
        }

        // Llamar cuando el jugador falla. Resta una vida; si llega a 0 -> Defeat.
        public void RegisterFailure()
        {
            Lives = Mathf.Max(0, Lives - 1);
            OnLivesChanged?.Invoke(Lives);
            if (Lives <= 0) Lose();
        }

        // Derrota directa (game over).
        public void Lose()
        {
            SetState(GameState.Defeat);
        }

        // ── Navegación desde pantallas ──────────────────────────────────────
        public bool HasNextLevel => CurrentLevel < totalLevels;

        public void GoToNextLevel()
        {
            if (HasNextLevel) StartLevel(CurrentLevel + 1);
            else GoToHub(); // completó el juego
        }

        public void RetryLevel() => StartLevel(CurrentLevel);

        // ── Score ───────────────────────────────────────────────────────────
        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }
    }
}
