using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float gameTime = 60f;
    [SerializeField] private ScoreManager scoreManager;

    private float currentTime;
    private bool isGameActive = false;

    private void Start()
    {
        currentTime = gameTime;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (isGameActive)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                EndGame();
            }

            UpdateTimerUI();
        }
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        isGameActive = true;
        currentTime = gameTime;

        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
    }

    public void EndGame()
    {
        isGameActive = false;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }
}