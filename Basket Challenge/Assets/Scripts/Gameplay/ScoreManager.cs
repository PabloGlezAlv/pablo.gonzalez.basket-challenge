using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private int totalScore = 0;
    
    private void Start()
    {
        UpdateScoreUI();
    }
    
    public void AddScore(Ball ball)
    {
        if (ball == null) return;
        
        int pointsToAdd = ball.GetBallScore();
        
        totalScore += pointsToAdd;
        UpdateScoreUI();
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = totalScore.ToString();
        }
    }
    
    public void ResetScore()
    {
        totalScore = 0;
        UpdateScoreUI();
    }
}