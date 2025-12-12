using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Main Menu UI")]
        [SerializeField] private Canvas mainMenu;
        [SerializeField] private Button gameStartButton;

        [Header("In Game UI")]
        [SerializeField] private Canvas inGame;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Result UI")]
        [SerializeField] private Canvas result;
        [SerializeField] private Button returnMenuButton;
        [SerializeField] private TextMeshProUGUI resultText;

        [Header("Pause UI")]
        [SerializeField] private Transform pause;

        public void Start()
        {
            GameManager.Instance.onMenuEnter += OnMenuEnter;
            GameManager.Instance.onGameStart += OnGameStart;
            GameManager.Instance.onGamePause += OnGamePause;
            GameManager.Instance.onGameOver += OnGameOver;

            gameStartButton.onClick.AddListener(GameManager.Instance.StartGame);
            returnMenuButton.onClick.AddListener(GameManager.Instance.ReturnMenu);

            SwitchUI(GameState.Menu);
        }

        public void OnDestroy()
        {
            GameManager.Instance.onMenuEnter -= OnMenuEnter;
            GameManager.Instance.onGameStart -= OnGameStart;
            GameManager.Instance.onGamePause -= OnGamePause;
            GameManager.Instance.onGameOver -= OnGameOver;

            gameStartButton.onClick.RemoveListener(GameManager.Instance.StartGame);
        }

        private void SwitchUI(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    mainMenu.gameObject.SetActive(false);
                    inGame.gameObject.SetActive(true);
                    result.gameObject.SetActive(false);
                    pause.gameObject.SetActive(false);
                    break;
                case GameState.Paused:
                    pause.gameObject.SetActive(true);
                    break;
                case GameState.GameOver:
                    result.gameObject.SetActive(true);
                    mainMenu.gameObject.SetActive(false);
                    inGame.gameObject.SetActive(false);
                    pause.gameObject.SetActive(false);
                    break;
                case GameState.Menu:
                default:
                    mainMenu.gameObject.SetActive(true);
                    inGame.gameObject.SetActive(false);
                    result.gameObject.SetActive(false);
                    pause.gameObject.SetActive(false);
                    break;
            }
        }

        private void OnMenuEnter()
        {
            SwitchUI(GameState.Menu);
        }

        private void OnGameStart()
        {
            SwitchUI(GameState.Playing);
        }

        private void OnGamePause()
        {
            SwitchUI(GameState.Paused);
        }

        private void OnGameOver()
        {
            SwitchUI(GameState.GameOver);
        }


        public void UpdateTimer(float time)
        {
            timerText.text = time.ToString("F2");
        }

        public void UpdateScore(int score)
        {
            scoreText.text = score.ToString();
        }

        public void SetCountdownText(string text, bool isActive)
        {
            if (!countdownText) return;

            countdownText.gameObject.SetActive(isActive);
            countdownText.text = text;
        }
    }
}