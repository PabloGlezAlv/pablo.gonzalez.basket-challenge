using System;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public event Action OnGameStarted;
    public event Action OnGameEnded;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float gameTime = 60f;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PlayerAnimatorController playerAnimatorController;

    private float currentTime;
    private bool isGameActive = false;

    private void Start()
    {
        currentTime = gameTime;
        UpdateTimerUI();
    }
    private void OnEnable()
    {
        if (playerAnimatorController != null)
        {
            playerAnimatorController.OnGameStarted += StartGame;
        }
    }

    private void OnDisable()
    {
        if (playerAnimatorController != null)
        {
            playerAnimatorController.OnGameStarted -= StartGame;
        }
    }

    private void Update()
    {
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            EndGame();
        }

        UpdateTimerUI();
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        isGameActive = true;
        currentTime = gameTime;

        if (scoreManager != null) scoreManager.ResetScore();

        OnGameStarted?.Invoke();
    }

    public void EndGame()
    {
        if (!isGameActive) return;
        isGameActive = false;
        OnGameEnded?.Invoke();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }
}
