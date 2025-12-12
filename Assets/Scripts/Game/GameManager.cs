using System.Collections;
using UnityEngine;
using Game.UI;
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
        [SerializeField] private PlayManager playManager;
        [SerializeField] private UIManager uiManager;

        // 게임 설정 기능
        // 기본 30초
        private float gameDuration = 30f;

        // 게임 상태 변경 이벤트
        public event UnityAction<GameState, GameState> onGameStateChanged;

        public event UnityAction onMenuEnter;
        public event UnityAction onGameStart;
        public event UnityAction onGamePause;
        public event UnityAction onGameResume;
        public event UnityAction onGameOver;

        private GameState previousState;
        private GameState currentState;
        
        public GameState GameState
        {
            get => currentState;
            set
            {
                previousState = currentState;
                currentState = value;
                onGameStateChanged?.Invoke(previousState, currentState);
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

            onGameStateChanged += OnGameStateChanged;
        }

        private void Start()
        {
            GameState = GameState.Menu;
        }

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
            StartGame();
        }

        private void OnGameStateChanged(GameState prevState, GameState newState)
        {
            print($"Game State Changed: [{prevState}] -> [{newState}]");

            switch (newState)
            {
                case GameState.Menu:
                    onMenuEnter?.Invoke();
                    HandleMenuState();
                    break;
                case GameState.Playing:
                    if (prevState == GameState.Paused)
                    {
                        onGameResume?.Invoke();
                    }
                    else
                    {
                        onGameStart?.Invoke();
                        HandlePlayingState();
                    }
                    break;
                case GameState.Paused:
                    onGamePause?.Invoke();
                    break;
                case GameState.GameOver:
                    onGameOver?.Invoke();
                    break;
            }
        }

        private void HandleMenuState()
        {
            Time.timeScale = 1f;
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            if (playManager) playManager.StopGameLoop();
        }

        private void HandlePlayingState()
        {
            Time.timeScale = 1f;
            currentScore = 0;
            currentTime = 0f;

            // UI 초기화
            uiManager.UpdateScore(0);
            uiManager.UpdateTimer(0);

            // 게임 시퀀스 시작 (카운트다운 -> 게임)
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            gameRoutine = StartCoroutine(GameSequenceRoutine());
        }

        private void HandleGameOverState()
        {
            if (gameRoutine != null) StopCoroutine(gameRoutine);
            if (playManager) playManager.StopGameLoop();
        }

        // --- 핵심: 게임 루프 코루틴 ---
        private IEnumerator GameSequenceRoutine()
        {
            // 1. 초기화
            currentTime = 0f;
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

                uiManager.SetCountdownText("", false); // 텍스트 끄기
            }
            else
            {
                yield return new WaitForSeconds(3.5f); // UI가 없어도 시간은 기다림
            }

            // --- 여기부터 실제 게임 시작 (나중에 두더지 스폰 시작 지점) ---
            // if (playManager) playManager.StartSpawning();

            // 3. 타이머 루프 (0.00 -> 5.00)
            while (currentTime < gameDuration)
            {
                currentTime += Time.deltaTime;

                if (uiManager) uiManager.UpdateTimer(currentTime);

                yield return null; // 1프레임 대기
            }

            // 4. 게임 종료
            currentTime = gameDuration;
            if (uiManager) uiManager.UpdateTimer(currentTime);

            // --- (나중에 두더지 스폰 중지 지점) ---
            // if (playManager) playManager.StopSpawning();

            GameState = GameState.GameOver; // 상태 변경 -> 결과창 뜸
        }
    }
}