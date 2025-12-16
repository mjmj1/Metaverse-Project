using System;
using System.Collections;
using UnityEngine;
using Game.UI;
using Meta.WitAi;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;

namespace Game
{
    public enum GameState
    {
        Menu,       // 메인 메뉴
        Playing,    // 게임 플레이 중
        Paused,     // 일시정지
        GameOver    // 게임 오버
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Game Components")]
        [SerializeField] private Transform playerHead;
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private MoleManager moleManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private ScoreManager scoreManager;

        private float gameDuration = 30f;
        private float spawnRange = 0f;
        private float spawnRadius = 10f;

        // 망치 오브젝트 풀링
        private GameObject weapon;

        // 게임 상태 변경 이벤트
        public event UnityAction<GameState, GameState> OnStateChanged;

        public event UnityAction OnMenuEnter;
        public event UnityAction OnGameStart;
        public event UnityAction OnGamePause;
        public event UnityAction OnGameResume;
        public event UnityAction OnGameOver;

        private GameState previousState;
        private GameState currentState;
        
        public GameState GameState
        {
            get => currentState;
            set
            {
                previousState = currentState;
                currentState = value;
                OnStateChanged?.Invoke(previousState, currentState);
            }
        }

        public static GameManager Instance { get; private set; }

        // 내부 변수
        private float currentTime;
        private Coroutine gameRoutine;

        private void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);

            OnStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            OnStateChanged -= OnGameStateChanged;
        }

        private void Start()
        {
            InitializeWeapon();
            GameState = GameState.Menu;
        }

        private void InitializeWeapon()
        {
            if (!weaponPrefab) return;

            weapon = Instantiate(weaponPrefab);
            weapon.name = "Weapon";
            weapon.SetActive(false);
        }

        private void HandleWeapon(bool visible, Vector3 pos = default, Quaternion rot = default)
        {
            if (!weapon)
            {
                InitializeWeapon();
            }

            weapon.SetActive(visible);
            weapon.transform.position = pos;
            weapon.transform.rotation = rot;
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Start)) TogglePause();
        }

        private void TogglePause()
        {
            if (currentState == GameState.Playing) PauseGame();
            else if (currentState == GameState.Paused) ResumeGame();
        }

        public ScoreManager ScoreManager => scoreManager;

        public void SetGameTime(float time) => gameDuration = time;
        public float GetGameTime() => gameDuration;

        public void SetSpawnRange(float range) => spawnRange = range;
        public float GetSpawnRange() => spawnRange;

        public void SetSpawnRadius(float radius) => spawnRadius = radius;
        public float GetSpawnRadius() => spawnRadius * 0.1f;

        public void StartGame() => GameState = GameState.Playing;
        public void EndGame() => GameState = GameState.GameOver;
        public void ReturnMenu() => GameState = GameState.Menu;

        public void PauseGame()
        {
            if (currentState == GameState.Playing) GameState = GameState.Paused;
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused) GameState = GameState.Playing;
        }

        public void RestartGame()
        {
            EndGame();

            StartGame();
        }

        private void OnGameStateChanged(GameState prevState, GameState newState)
        {
            print($"Game State Changed: [{prevState}] -> [{newState}]");

            switch (newState)
            {
                case GameState.Menu:
                    AudioManager.Instance.PlayLobbyBGM();
                    OnMenuEnter?.Invoke();
                    HandleMenuState();
                    break;
                case GameState.Playing:
                    if (prevState == GameState.Paused)
                    {
                        Time.timeScale = 1f;
                        OnGameResume?.Invoke();
                    }
                    else
                    {
                        OnGameStart?.Invoke();
                        HandlePlayingState();
                    }
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    OnGamePause?.Invoke();
                    break;
                case GameState.GameOver:
                    OnGameOver?.Invoke();
                    HandleGameOverState();
                    break;
            }
        }

        private void HandleMenuState()
        {
            Time.timeScale = 1f;
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            if (moleManager) moleManager.StopGameLoop();
            if (weapon) HandleWeapon(false);
        }

        private void HandlePlayingState()
        {
            Time.timeScale = 1f;
            currentTime = 0f;

            // ScoreManager 초기화
            if (scoreManager) scoreManager.ResetScore();

            // UI 초기화
            if (uiManager)
            {
                uiManager.UpdateScore(0);
                uiManager.UpdateTimer(gameDuration);
            }

            // 게임 시퀀스 시작 (카운트다운 -> 게임)
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            gameRoutine = StartCoroutine(GameSequenceRoutine());
        }

        private void HandleGameOverState()
        {
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            if (moleManager) moleManager.StopGameLoop();

            SaveHighScore();
        }

        private void SaveHighScore()
        {
            var finalScore = scoreManager.CurrentScore;
            var highScore = PlayerPrefs.GetInt("HighScore", 0);

            if (finalScore <= highScore) return;

            PlayerPrefs.SetInt("HighScore", finalScore);
            PlayerPrefs.Save();

            uiManager.UpdateHighScore();
            print($"New High Score: {finalScore}");
        }

        public int GetHighScore() => PlayerPrefs.GetInt("HighScore", 0);

        private IEnumerator GameSequenceRoutine()
        {
            AudioManager.Instance.StopBGM();

            // 1. 초기화
            moleManager.Initialize();

            // 망치 오브젝트 풀에서 활성화
            if (weapon)
            {
                var spawnDistance = 0.5f;
                var spawnPos = playerHead.position + playerHead.forward * spawnDistance + playerHead.up * -0.3f;
                var spawnRot = Quaternion.LookRotation(playerHead.forward);

                HandleWeapon(true, spawnPos, spawnRot);
            }

            currentTime = gameDuration;
            if (scoreManager) scoreManager.ResetScore();

            if (uiManager)
            {
                uiManager.UpdateTimer(currentTime);

                uiManager.SetCountdownText("3", true);
                if (AudioManager.Instance) AudioManager.Instance.PlayCountdown();
                yield return new WaitForSeconds(1f);

                uiManager.SetCountdownText("2", true);
                if (AudioManager.Instance) AudioManager.Instance.PlayCountdown();
                yield return new WaitForSeconds(1f);

                uiManager.SetCountdownText("1", true);
                if (AudioManager.Instance) AudioManager.Instance.PlayCountdown();
                yield return new WaitForSeconds(1f);

                uiManager.SetCountdownText("START!", true);
                if (AudioManager.Instance) AudioManager.Instance.PlayGameStart();
                yield return new WaitForSeconds(0.5f);

                uiManager.SetCountdownText("", false);
            }
            else
            {
                yield return new WaitForSeconds(3.5f);
            }

            if (moleManager) moleManager.StartGameLoop();

            while (0 < currentTime)
            {
                currentTime -= Time.deltaTime;

                if (uiManager) uiManager.UpdateTimer(currentTime);

                yield return null; // 1프레임 대기
            }

            currentTime = 0.00f;

            if (uiManager)
            {
                uiManager.UpdateTimer(currentTime);
                uiManager.SetFinalScoreText(scoreManager.CurrentScore);
            }

            if (moleManager) moleManager.StopGameLoop();
            if (AudioManager.Instance) AudioManager.Instance.PlayGameOver();
            if (weapon) HandleWeapon(false);

            EndGame();
        }
    }
}