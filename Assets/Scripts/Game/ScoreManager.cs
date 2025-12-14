using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Game
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Settings")]
        [SerializeField] private int baseMoleScore = 100;
        [SerializeField] private int comboBonusMultiplier = 10;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("Events")]
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int> OnComboChanged;

        private int currentScore = 0;
        private int comboCount = 0;
        private float lastHitTime = 0f;
        private const float COMBO_TIME_WINDOW = 1.5f;

        public int CurrentScore => currentScore;
        public int ComboCount => comboCount;

        private void Awake()
        {
            // UI 참조가 없으면 자동으로 찾기
            if (scoreText == null)
            {
                var scoreObj = GameObject.FindGameObjectWithTag("ScoreText");
                if (scoreObj != null)
                    scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            }

            if (comboText == null)
            {
                var comboObj = GameObject.FindGameObjectWithTag("ComboText");
                if (comboObj != null)
                    comboText = comboObj.GetComponent<TextMeshProUGUI>();
            }
        }

        private void Start()
        {
            ResetScore();
        }

        /// <summary>
        /// 두더지 타격 시 호출될 메인 점수 추가 함수
        /// </summary>
        public void AddMoleHit()
        {
            // 콤보 계산
            int comboBonus = CalculateCombo();

            // 총 점수 계산 (기본 점수 + 콤보 보너스)
            int totalScore = baseMoleScore + comboBonus;

            // 점수 추가
            AddScore(totalScore);

            print($"두더지 타격! 기본: {baseMoleScore}, 콤보 보너스: {comboBonus}, 총: {totalScore}");
        }

        /// <summary>
        /// 특정 점수 직접 추가 (테스트용)
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            UpdateScoreDisplay();
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// 콤보 계산 및 업데이트
        /// </summary>
        private int CalculateCombo()
        {
            if (Time.time - lastHitTime < COMBO_TIME_WINDOW)
            {
                comboCount++;
            }
            else
            {
                comboCount = 1; // 콤보 초기화
            }

            lastHitTime = Time.time;
            UpdateComboDisplay();
            OnComboChanged?.Invoke(comboCount);

            // 콤보 보너스 계산 (2콤보부터 보너스)
            return comboCount > 1 ? (comboCount - 1) * comboBonusMultiplier : 0;
        }

        /// <summary>
        /// 점수 표시 업데이트
        /// </summary>
        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore:N0}";
            }
        }

        /// <summary>
        /// 콤보 표시 업데이트
        /// </summary>
        private void UpdateComboDisplay()
        {
            if (comboText != null)
            {
                comboText.text = comboCount > 1 ? $"{comboCount}x Combo!" : "";
                comboText.gameObject.SetActive(comboCount > 1);
            }
        }

        /// <summary>
        /// 점수 초기화
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            comboCount = 0;
            lastHitTime = 0f;
            UpdateScoreDisplay();
            UpdateComboDisplay();
            OnScoreChanged?.Invoke(0);
            OnComboChanged?.Invoke(0);
        }

        /// <summary>
        /// 현재 게임 데이터 반환
        /// </summary>
        public GameSessionData GetGameData()
        {
            return new GameSessionData
            {
                finalScore = currentScore,
                maxCombo = comboCount,
                gameTime = Time.time
            };
        }

        /// <summary>
        /// 콤보 초기화 (시간 초과 등)
        /// </summary>
        public void ResetCombo()
        {
            comboCount = 0;
            UpdateComboDisplay();
            OnComboChanged?.Invoke(0);
        }

        private void Update()
        {
            // 콤보 타임아웃 체크
            if (comboCount > 0 && Time.time - lastHitTime > COMBO_TIME_WINDOW)
            {
                ResetCombo();
            }
        }
    }

    /// <summary>
    /// 게임 세션 데이터 구조체
    /// </summary>
    [Serializable]
    public class GameSessionData
    {
        public int finalScore;
        public int maxCombo;
        public float gameTime;
        public DateTime timestamp = DateTime.Now;
    }
}