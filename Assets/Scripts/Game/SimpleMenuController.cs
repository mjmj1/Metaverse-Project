using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game
{
    public class SimpleMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Game References")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private ScoreManager scoreManager;

        private void Awake()
        {
            // 컴포넌트 자동 찾기
            FindComponents();
        }

        private void Start()
        {
            InitializeMenu();
            ShowMainMenu();
        }

        private void FindComponents()
        {
            // GameStateManager 찾기
            if (gameStateManager == null)
                gameStateManager = FindObjectOfType<GameStateManager>();

            // ScoreManager 찾기
            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>();

            // 패널 자동 찾기
            if (mainMenuPanel == null)
            {
                var menuObj = GameObject.FindGameObjectWithTag("MainMenuPanel");
                if (menuObj != null) mainMenuPanel = menuObj;
            }

            if (gameplayPanel == null)
            {
                var gameplayObj = GameObject.FindGameObjectWithTag("GameplayPanel");
                if (gameplayObj != null) gameplayPanel = gameplayObj;
            }

            if (gameOverPanel == null)
            {
                var gameOverObj = GameObject.FindGameObjectWithTag("GameOverPanel");
                if (gameOverObj != null) gameOverPanel = gameOverObj;
            }

            // UI 요소 자동 찾기
            if (startButton == null)
            {
                var startObj = GameObject.FindGameObjectWithTag("StartButton");
                if (startObj != null) startButton = startObj.GetComponent<Button>();
            }

            if (restartButton == null)
            {
                var restartObj = GameObject.FindGameObjectWithTag("RestartButton");
                if (restartObj != null) restartButton = restartObj.GetComponent<Button>();
            }

            if (menuButton == null)
            {
                var menuObj = GameObject.FindGameObjectWithTag("MenuButton");
                if (menuObj != null) menuButton = menuObj.GetComponent<Button>();
            }

            if (finalScoreText == null)
            {
                var scoreObj = GameObject.FindGameObjectWithTag("FinalScoreText");
                if (scoreObj != null) finalScoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            }

            if (highScoreText == null)
            {
                var highScoreObj = GameObject.FindGameObjectWithTag("HighScoreText");
                if (highScoreObj != null) highScoreText = highScoreObj.GetComponent<TextMeshProUGUI>();
            }
        }

        private void InitializeMenu()
        {
            // 버튼 이벤트 연결
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            if (menuButton != null)
                menuButton.onClick.AddListener(GoToMenu);

            Debug.Log("메뉴 시스템 초기화 완료");
        }

        public void StartGame()
        {
            Debug.Log("게임 시작 버튼 클릭!");

            // 게임 시작
            if (gameStateManager != null)
            {
                gameStateManager.StartGame();
            }

            // 게임 플레이 패널 표시
            ShowGameplayUI();
        }

        public void RestartGame()
        {
            Debug.Log("재시작 버튼 클릭!");

            // 게임 재시작
            if (gameStateManager != null)
            {
                gameStateManager.RestartGame();
            }

            // 게임 플레이 패널 표시
            ShowGameplayUI();
        }

        public void GoToMenu()
        {
            Debug.Log("메인 메뉴 버튼 클릭!");

            // 메인 메뉴로 이동
            if (gameStateManager != null)
            {
                gameStateManager.GoToMenu();
            }

            // 메인 메뉴 표시
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            Debug.Log("메인 메뉴 표시");

            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            if (gameplayPanel != null)
                gameplayPanel.SetActive(false);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        public void ShowGameplayUI()
        {
            Debug.Log("게임 플레이 UI 표시");

            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (gameplayPanel != null)
                gameplayPanel.SetActive(true);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        public void ShowGameOver(int finalScore)
        {
            Debug.Log($"게임 오버! 최종 점수: {finalScore}");

            // 최종 점수 표시
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore:N0}";
            }

            // 최고 점수 표시 (간단한 로컬 저장)
            if (highScoreText != null)
            {
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                if (finalScore > highScore)
                {
                    highScore = finalScore;
                    PlayerPrefs.SetInt("HighScore", highScore);
                    PlayerPrefs.Save();
                    highScoreText.text = $"New High Score: {highScore:N0}!";
                    highScoreText.color = Color.yellow;
                }
                else
                {
                    highScoreText.text = $"High Score: {highScore:N0}";
                    highScoreText.color = Color.white;
                }
            }

            // 패널 전환
            if (gameplayPanel != null)
                gameplayPanel.SetActive(false);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }

        // GameStateManager에서 호출할 메서드
        public void OnGameStarted()
        {
            ShowGameplayUI();
        }

        public void OnGameEnded(int finalScore)
        {
            ShowGameOver(finalScore);
        }

        // 테스트용 메서드
        [ContextMenu("테스트: 메인 메뉴 표시")]
        public void TestShowMainMenu()
        {
            ShowMainMenu();
        }

        [ContextMenu("테스트: 게임 오버 표시")]
        public void TestShowGameOver()
        {
            ShowGameOver(1500);
        }
    }
}