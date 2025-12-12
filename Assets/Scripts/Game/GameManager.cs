using System.Collections;
using UnityEngine;
using Game.UI;
using Meta.WitAi;
using UnityEngine.Events;

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
        [SerializeField] private GameObject weapon;
        [SerializeField] private MoleManager moleManager;
        [SerializeField] private UIManager uiManager;

        // 게임 설정 기능
        // 기본 30초
        private float gameDuration = 30f;
        private float spawnRange = 0f;

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
        private int currentScore;
        private Coroutine gameRoutine;

        private void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);

            OnStateChanged += OnGameStateChanged;
        }

        private void Start()
        {
            GameState = GameState.Menu;
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Start))
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            if (currentState == GameState.Playing) PauseGame();
            else if (currentState == GameState.Paused) ResumeGame();
        }

        public void AddScore(int score)
        {
            currentScore += score;
            uiManager.UpdateScore(currentScore);
        }

        public int GetScore() => currentScore;

        public void SetGameTime(float time) => gameDuration = time;
        public float GetGameTime() => gameDuration;

        public void SetSpawnRange(float range) => spawnRange = range;
        public float GetSpawnRange() => spawnRange;

        public void StartGame() => GameState = GameState.Playing;
        public void EndGame() => GameState = GameState.GameOver;
        public void ReturnMenu() => GameState = GameState.Menu;

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;

            GameState = GameState.Paused;
        }
        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;

            GameState = GameState.Playing;
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
        }

        private void HandlePlayingState()
        {
            Time.timeScale = 1f;
            currentScore = 0;
            currentTime = 0f;

            // UI 초기화
            uiManager.UpdateScore(0);
            uiManager.UpdateTimer(gameDuration);

            // 게임 시퀀스 시작 (카운트다운 -> 게임)
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            gameRoutine = StartCoroutine(GameSequenceRoutine());
        }

        private void HandleGameOverState()
        {
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            if (moleManager) moleManager.StopGameLoop();
        }

        // --- 핵심: 게임 루프 코루틴 ---
        private IEnumerator GameSequenceRoutine()
        {
            // 1. 초기화
            moleManager.Initialize();

            var spawnDistance = 0.5f;

            var spawnPos = playerHead.position + playerHead.forward * spawnDistance + playerHead.up * -0.3f;

            var spawnRot = Quaternion.LookRotation(playerHead.forward);
            // 만약 망치 손잡이 각도가 이상하면 오프셋을 줘야 합니다. 예: spawnRot * Quaternion.Euler(90, 0, 0)

            var w = Instantiate(weapon, spawnPos, spawnRot);

            currentTime = gameDuration;
            currentScore = 0;
            if(uiManager)
            {
                uiManager.UpdateTimer(0);
                uiManager.UpdateScore(0);
            }

            // 2. 카운트다운 (3초)
            if (uiManager)
            {
                uiManager.SetCountdownText("3", true);
                yield return new WaitForSeconds(1f);

                uiManager.SetCountdownText("2", true);
                yield return new WaitForSeconds(1f);

                uiManager.SetCountdownText("1", true);
                yield return new WaitForSeconds(1f);

                uiManager.SetCountdownText("START!", true);
                yield return new WaitForSeconds(0.5f);

                uiManager.SetCountdownText("", false);
            }
            else
            {
                yield return new WaitForSeconds(3.5f);
            }

            // --- 여기부터 실제 게임 시작 ---
            if (moleManager) moleManager.StartGameLoop();

            while (0 < currentTime)
            {
                currentTime -= Time.deltaTime;

                if (uiManager) uiManager.UpdateTimer(currentTime);

                yield return null; // 1프레임 대기
            }

            // 4. 게임 종료
            currentTime = 0.00f;
            if (uiManager)
            {
                uiManager.UpdateTimer(currentTime);
                uiManager.SetFinalScoreText(currentScore);
            }

            if (moleManager) moleManager.StopGameLoop();

            Destroy(w.gameObject);

            EndGame();
        }
    }
}