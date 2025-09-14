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
        
        int collisionCount = ball.GetCollisionCount();
        int pointsToAdd = collisionCount > 0 ? 2 : 3;
        
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