using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Main Menu UI")]
        [SerializeField] private Canvas mainMenu;
        [SerializeField] private Button gameStartButton;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private TextMeshProUGUI timeSliderValueText;
        [SerializeField] private Slider rangeSlider;
        [SerializeField] private TextMeshProUGUI rangeSliderValueText;

        [Header("Game Info UI")]
        [SerializeField] private Canvas gameInfo;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Result UI")]
        [SerializeField] private Canvas result;
        [SerializeField] private Button resultHomeButton;
        [SerializeField] private TextMeshProUGUI finalScoreText;

        [Header("Pause UI")]
        [SerializeField] private Canvas pause;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button homeButton;

        public void Start()
        {
            GameManager.Instance.OnMenuEnter += OnMenuEnter;
            GameManager.Instance.OnGameStart += OnGameStart;
            GameManager.Instance.OnGameResume += OnGameStart;
            GameManager.Instance.OnGamePause += OnGamePause;
            GameManager.Instance.OnGameOver += OnGameOver;

            gameStartButton.onClick.AddListener(GameManager.Instance.StartGame);
            resultHomeButton.onClick.AddListener(GameManager.Instance.ReturnMenu);

            resumeButton.onClick.AddListener(GameManager.Instance.ResumeGame);
            restartButton.onClick.AddListener(GameManager.Instance.RestartGame);
            homeButton.onClick.AddListener(GameManager.Instance.ReturnMenu);

            var defaultTime = GameManager.Instance.GetGameTime();
            timeSlider.value = defaultTime;
            UpdateTimeSliderText(defaultTime);

            timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);

            var defaultRange = GameManager.Instance.GetGameTime();
            rangeSlider.value = defaultRange;
            UpdateRangeSliderText(defaultRange);

            rangeSlider.onValueChanged.AddListener(OnRangeSliderChanged);

            // ScoreManager 이벤트 구독
            if (GameManager.Instance.ScoreManager)
            {
                GameManager.Instance.ScoreManager.OnScoreChanged.AddListener(UpdateScore);
                GameManager.Instance.ScoreManager.OnComboChanged.AddListener(UpdateCombo);
            }

            // 최고 점수 표시
            UpdateHighScore();

            SwitchUI(GameState.Menu);
        }

        public void OnDestroy()
        {
            GameManager.Instance.OnMenuEnter -= OnMenuEnter;
            GameManager.Instance.OnGameStart -= OnGameStart;
            GameManager.Instance.OnGameResume -= OnGameStart;
            GameManager.Instance.OnGamePause -= OnGamePause;
            GameManager.Instance.OnGameOver -= OnGameOver;

            gameStartButton.onClick.RemoveListener(GameManager.Instance.StartGame);
            resultHomeButton.onClick.RemoveListener(GameManager.Instance.ReturnMenu);

            resumeButton.onClick.RemoveListener(GameManager.Instance.ResumeGame);
            restartButton.onClick.RemoveListener(GameManager.Instance.RestartGame);
            homeButton.onClick.RemoveListener(GameManager.Instance.ReturnMenu);

            timeSlider.onValueChanged.RemoveListener(OnTimeSliderChanged);
            rangeSlider.onValueChanged.RemoveListener(OnRangeSliderChanged);

            // ScoreManager 이벤트 구독 해제
            if (GameManager.Instance?.ScoreManager)
            {
                GameManager.Instance.ScoreManager.OnScoreChanged.RemoveListener(UpdateScore);
                GameManager.Instance.ScoreManager.OnComboChanged.RemoveListener(UpdateCombo);
            }
        }

        private void SwitchUI(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    mainMenu.gameObject.SetActive(false);
                    gameInfo.gameObject.SetActive(true);
                    result.gameObject.SetActive(false);
                    pause.gameObject.SetActive(false);
                    break;
                case GameState.Paused:
                    pause.gameObject.SetActive(true);
                    break;
                case GameState.GameOver:
                    result.gameObject.SetActive(true);
                    mainMenu.gameObject.SetActive(false);
                    gameInfo.gameObject.SetActive(false);
                    pause.gameObject.SetActive(false);
                    break;
                case GameState.Menu:
                default:
                    mainMenu.gameObject.SetActive(true);
                    gameInfo.gameObject.SetActive(false);
                    result.gameObject.SetActive(false);
                    pause.gameObject.SetActive(false);
                    break;
            }
        }

        private void OnMenuEnter() => SwitchUI(GameState.Menu);

        private void OnGameStart() => SwitchUI(GameState.Playing);

        private void OnGamePause() => SwitchUI(GameState.Paused);

        private void OnGameOver() => SwitchUI(GameState.GameOver);

        private void OnTimeSliderChanged(float value)
        {
            GameManager.Instance.SetGameTime(value);

            UpdateTimeSliderText(value);
        }

        private void OnRangeSliderChanged(float value)
        {
            GameManager.Instance.SetSpawnRange(value);

            UpdateRangeSliderText(value);
        }

        private void UpdateTimeSliderText(float value) => timeSliderValueText.text = $"{value:F0}";
        private void UpdateRangeSliderText(float value) => rangeSliderValueText.text = $"{value:F0}";

        public void UpdateTimer(float time) => timerText.text = time.ToString("F2");

        public void UpdateScore(int score) => scoreText.text = score.ToString();

        public void UpdateCombo(int combo)
        {
            if (!comboText) return;

            if (combo > 1)
            {
                comboText.text = $"{combo}x COMBO!";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.text = "";
                comboText.gameObject.SetActive(false);
            }
        }

        public void UpdateHighScore()
        {
            if (!highScoreText) return;

            int highScore = GameManager.Instance.GetHighScore();
            highScoreText.text = $"High Score: {highScore:N0}";
        }

        public void SetFinalScoreText(int score) => finalScoreText.text = $"SCORE: {score}";

        public void SetCountdownText(string text, bool isActive)
        {
            countdownText.gameObject.SetActive(isActive);
            countdownText.text = text;
        }
    }
}