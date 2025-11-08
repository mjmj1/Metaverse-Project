using UnityEngine;
using TMPro;

namespace Game
{
    public class HUDManager : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Score Manager")]
        [SerializeField] private ScoreManager scoreManager;

        [Header("Timer")]
        [SerializeField] private float gameDuration = 60f;
        private float timeRemaining;

        private void Awake()
        {
            // 자동으로 UI 요소 찾기
            FindHUDElements();
        }

        private void Start()
        {
            InitializeHUD();
        }

        private void FindHUDElements()
        {
            // Score Text 찾기
            if (scoreText == null)
            {
                var scoreObj = GameObject.FindGameObjectWithTag("ScoreText");
                if (scoreObj != null)
                    scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            }

            // Combo Text 찾기
            if (comboText == null)
            {
                var comboObj = GameObject.FindGameObjectWithTag("ComboText");
                if (comboObj != null)
                    comboText = comboObj.GetComponent<TextMeshProUGUI>();
            }

            // Timer Text 찾기
            if (timerText == null)
            {
                var timerObj = GameObject.FindGameObjectWithTag("TimerText");
                if (timerObj != null)
                    timerText = timerObj.GetComponent<TextMeshProUGUI>();
            }

            // Score Manager 찾기
            if (scoreManager == null)
            {
                scoreManager = FindObjectOfType<ScoreManager>();
            }
        }

        private void InitializeHUD()
        {
            // Score Manager 이벤트 구독
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += UpdateScoreDisplay;
                scoreManager.OnComboChanged += UpdateComboDisplay;
            }

            // 타이머 초기화
            timeRemaining = gameDuration;
            UpdateTimerDisplay();
        }

        private void Update()
        {
            // 타이머 업데이트
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();

                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    OnTimeExpired();
                }
            }
        }

        private void UpdateScoreDisplay(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {newScore:N0}";
            }
        }

        private void UpdateComboDisplay(int combo)
        {
            if (comboText != null)
            {
                comboText.text = combo > 1 ? $"{combo}x Combo!" : "";
                comboText.gameObject.SetActive(combo > 1);
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                timerText.text = $"Time: {minutes:0}:{seconds:00}";

                // 시간이 얼마 남지 않았을 때 색상 변경
                if (timeRemaining <= 10f)
                {
                    timerText.color = Color.red;
                }
            }
        }

        private void OnTimeExpired()
        {
            Debug.Log("시간 종료! 게임 오버");
            // 여기에 게임 오버 로직 추가
            var gameStateManager = FindObjectOfType<GameStateManager>();
            if (gameStateManager != null)
            {
                gameStateManager.EndGame();
            }
        }

        // 타이머 재설정
        public void ResetTimer()
        {
            timeRemaining = gameDuration;
            UpdateTimerDisplay();
            if (timerText != null)
            {
                timerText.color = Color.white;
            }
        }

        // 게임 시간 설정
        public void SetGameDuration(float duration)
        {
            gameDuration = duration;
            timeRemaining = gameDuration;
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= UpdateScoreDisplay;
                scoreManager.OnComboChanged -= UpdateComboDisplay;
            }
        }
    }
}