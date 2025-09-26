using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI enemyScoreText;
    
    private int playerScore = 0;
    private int enemyScore = 0;
    
    private void Start()
    {
        UpdateScoreUI();
    }
    
    public void AddPlayerScore(int pointsToAdd)
    {
        playerScore += pointsToAdd;
        UpdateScoreUI();
    }
    
    public void AddEnemyScore(int pointsToAdd)
    {
        enemyScore += pointsToAdd;
        UpdateScoreUI();
    }
    
    public void AddScore(int pointsToAdd)
    {
        AddPlayerScore(pointsToAdd);
    }
    
    private void UpdateScoreUI()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = playerScore.ToString();
        }
        
        if (enemyScoreText != null)
        {
            enemyScoreText.text = enemyScore.ToString();
        }
    }
    
    public void ResetScore()
    {
        playerScore = 0;
        enemyScore = 0;
        UpdateScoreUI();
    }

    public int GetPlayerScore()
    {
        return playerScore;
    }
    
    public int GetEnemyScore()
    {
        return enemyScore;
    }
    
    public int GetTotalScore()
    {
        return playerScore + enemyScore;
    }
}