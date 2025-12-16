using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Settings")] [SerializeField]
        private int baseMoleScore = 1; // 기본 점수

        [Header("Combo Settings")] [SerializeField]
        private float comboTimeWindow = 3.0f; // 콤보 유지 시간

        [SerializeField] private int comboStep = 5; // 보너스가 주어지는 콤보 단위 (5회)
        [SerializeField] private int bonusScorePerStep = 1; // 단위 당 추가 점수

        [SerializeField] private RectTransform comboGauge;

        [Header("Events")]
        public UnityEvent<int> onScoreChanged;

        public UnityEvent<int> onComboChanged;

        private int comboCount;
        private float lastHitTime;
        private float maxGaugeWidth;

        public int CurrentScore { get; private set; }

        private void Start()
        {
            ResetScore();
        }

        private void Update()
        {
            if (comboCount > 0)
            {
                var timeElapsed = Time.time - lastHitTime;

                if (timeElapsed > comboTimeWindow)
                {
                    ResetCombo();
                }
                else
                {
                    var remainingRatio = 1.0f - timeElapsed / comboTimeWindow;

                    var currentWidth = maxGaugeWidth * remainingRatio;

                    var size = comboGauge.sizeDelta;
                    size.x = currentWidth;
                    comboGauge.sizeDelta = size;
                }
            }
        }

        public void AddMoleHit()
        {
            UpdateComboState();

            // 2. 콤보 보너스 점수 계산
            // 공식: (현재콤보 / 5) * 단계별 점수
            // 예: 5콤보 -> 1 * 1 = 1점 추가
            // 예: 12콤보 -> 2 * 1 = 2점 추가
            var bonusStep = comboCount / comboStep;
            var comboBonus = bonusStep * bonusScorePerStep;

            var totalScore = baseMoleScore + comboBonus;

            AddScore(totalScore);
        }

        private void UpdateComboState()
        {
            if (Time.time - lastHitTime < comboTimeWindow)
            {
                comboGauge.gameObject.SetActive(true);
                comboCount++;
            }
            else
                comboCount = 0;

            lastHitTime = Time.time;
            onComboChanged?.Invoke(comboCount);
        }

        public void AddScore(int points)
        {
            CurrentScore += points;
            onScoreChanged?.Invoke(CurrentScore);
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            comboCount = 0;
            lastHitTime = -10f;
            maxGaugeWidth = comboGauge.sizeDelta.x;

            onScoreChanged?.Invoke(0);
            onComboChanged?.Invoke(0);

            ResetCombo();
        }

        public void ResetCombo()
        {
            comboCount = 0;
            onComboChanged?.Invoke(0);
            comboGauge.gameObject.SetActive(false);
        }
    }
}