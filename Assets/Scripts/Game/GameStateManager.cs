using UnityEngine;
using System.Collections;

namespace Game
{
    public enum GameState
    {
        Menu,       // 메인 메뉴
        Playing,    // 게임 플레이 중
        Paused,     // 일시정지
        GameOver    // 게임 오버
    }

    public class GameStateManager : MonoBehaviour
    {
        [Header("Game Components")]
        [SerializeField] private MoleManager moleManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private HUDManager hudManager;
        [SerializeField] private SimpleMenuController menuController;

        [Header("Game Settings")]
        [SerializeField] private float gameDuration = 60f;

        public GameState CurrentState { get; private set; } = GameState.Menu;
        public static GameStateManager Instance { get; private set; }

        private Coroutine gameTimerCoroutine;

        private void Awake()
        {
            // 싱글톤 패턴
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 컴포넌트 자동 찾기
            FindGameComponents();
        }

        private void Start()
        {
            InitializeGame();
        }

        private void FindGameComponents()
        {
            if (moleManager == null)
                moleManager = FindObjectOfType<MoleManager>();

            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>();

            if (hudManager == null)
                hudManager = FindObjectOfType<HUDManager>();

            if (menuController == null)
                menuController = FindObjectOfType<SimpleMenuController>();
        }

        private void InitializeGame()
        {
            // 초기 상태 설정
            ChangeState(GameState.Menu);
        }

        public void StartGame()
        {
            Debug.Log("게임 시작!");
            ChangeState(GameState.Playing);

            // 게임 초기화
            if (scoreManager != null)
                scoreManager.ResetScore();

            if (hudManager != null)
            {
                hudManager.ResetTimer();
                hudManager.SetGameDuration(gameDuration);
            }

            // 게임 타이머 시작
            if (gameTimerCoroutine != null)
                StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = StartCoroutine(GameTimerCoroutine());
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                Debug.Log("게임 일시정지");
                ChangeState(GameState.Paused);

                // 시간 정지
                Time.timeScale = 0f;

                // VR은 계속 동작해야 함
                // 필요하다면 VR 관련 컴포넌트만 계속 동작시키는 로직 추가
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                Debug.Log("게임 재개");
                ChangeState(GameState.Playing);

                // 시간 재개
                Time.timeScale = 1f;
            }
        }

        public void EndGame()
        {
            Debug.Log("게임 종료!");
            ChangeState(GameState.GameOver);

            // 게임 타이머 정지
            if (gameTimerCoroutine != null)
            {
                StopCoroutine(gameTimerCoroutine);
                gameTimerCoroutine = null;
            }

            // 두더지 스폰 정지
            if (moleManager != null)
            {
                moleManager.enabled = false;
            }

            // 최종 점수 표시 및 메뉴 컨트롤러에 알림
            if (scoreManager != null && menuController != null)
            {
                var gameData = scoreManager.GetGameData();
                Debug.Log($"최종 점수: {gameData.finalScore}, 최대 콤보: {gameData.maxCombo}");
                menuController.OnGameEnded(gameData.finalScore);
            }
        }

        public void RestartGame()
        {
            Debug.Log("게임 재시작!");

            // 모든 상태 초기화
            if (moleManager != null)
            {
                moleManager.enabled = true;
            }

            // 바로 새 게임 시작
            StartGame();
        }

        public void GoToMenu()
        {
            Debug.Log("메인 메뉴로");
            ChangeState(GameState.Menu);

            // 시간 스케일 정상화
            Time.timeScale = 1f;

            // 게임 정리
            if (gameTimerCoroutine != null)
            {
                StopCoroutine(gameTimerCoroutine);
                gameTimerCoroutine = null;
            }
        }

        private void ChangeState(GameState newState)
        {
            var previousState = CurrentState;
            CurrentState = newState;

            Debug.Log($"게임 상태 변경: {previousState} -> {newState}");

            // 상태 변경에 따른 처리
            OnStateChanged(previousState, newState);
        }

        private void OnStateChanged(GameState fromState, GameState toState)
        {
            switch (toState)
            {
                case GameState.Playing:
                    OnGameStart();
                    break;
                case GameState.Paused:
                    OnGamePaused();
                    break;
                case GameState.GameOver:
                    OnGameOver();
                    break;
                case GameState.Menu:
                    OnMenuEntered();
                    break;
            }
        }

        private void OnGameStart()
        {
            // 게임 시작 시 처리
            if (moleManager != null)
            {
                moleManager.enabled = true;
            }

            // 메뉴 컨트롤러에 알림
            if (menuController != null)
            {
                menuController.OnGameStarted();
            }
        }

        private void OnGamePaused()
        {
            // 일시정지 시 처리
        }

        private void OnGameOver()
        {
            // 게임 오버 시 처리
        }

        private void OnMenuEntered()
        {
            // 메뉴 진입 시 처리
            if (moleManager != null)
            {
                moleManager.enabled = false;
            }

            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }
        }

        private IEnumerator GameTimerCoroutine()
        {
            float timeRemaining = gameDuration;

            while (timeRemaining > 0f)
            {
                if (CurrentState == GameState.Playing)
                {
                    timeRemaining -= Time.deltaTime;

                    // 타이머 업데이트
                    if (hudManager != null)
                    {
                        // HUDManager에서 타이머 처리
                    }
                }

                yield return null;
            }

            // 시간 종료 시 게임 오버
            EndGame();
        }

        // 현재 게임 상태 정보 반환
        public GameSessionInfo GetCurrentGameInfo()
        {
            return new GameSessionInfo
            {
                state = CurrentState,
                score = scoreManager != null ? scoreManager.CurrentScore : 0,
                combo = scoreManager != null ? scoreManager.ComboCount : 0
            };
        }
    }

    // 게임 세션 정보
    [System.Serializable]
    public class GameSessionInfo
    {
        public GameState state;
        public int score;
        public int combo;
    }
}