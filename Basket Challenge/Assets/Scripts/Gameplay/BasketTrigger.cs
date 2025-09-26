using UnityEngine;
using System.Collections;
using System;

public enum BallOwner
{
    Player,
    Enemy
}

public class BasketTrigger : MonoBehaviour
{
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private BallShooter playerShooter;
    [SerializeField] private AIEnemyShooter enemyShooter;
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private ScoreFlyer scoreFlyer;
    [SerializeField] private CameraDirector cameraDirector;
    [SerializeField] private FireballController fireballController;

    public static event Action<int> OnScored;
    public static event Action<int> OnPlayerScored;

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            int score = ball.GetBallScore();
            BallOwner owner = ball.GetBallOwner();
            OnScored?.Invoke(score);

            if (fireballController != null && fireballController.IsDoublePointsActive() && owner == BallOwner.Player)
            {
                score *= 2;
            }

            if (owner == BallOwner.Player)
            {
                scoreManager.AddPlayerScore(score);
                OnPlayerScored?.Invoke(score);
            }
            else
            {
                scoreManager.AddEnemyScore(score);
            }

            cameraDirector.OnScoreMade();   

            if (owner == BallOwner.Player)
            {
                scoreFlyer.SetScore(score);
                scoreFlyer.gameObject.SetActive(true);
            }

            StartCoroutine(ResetAfterDelay(ball.gameObject, owner));
        }
    }
    
    private IEnumerator ResetAfterDelay(GameObject ballObj, BallOwner owner)
    {
        yield return new WaitForSeconds(resetDelay);
        
        if (owner == BallOwner.Player && playerShooter != null)
        {
            playerShooter.ResetBall(ballObj, true);
        }
        else if (owner == BallOwner.Enemy && enemyShooter != null)
        {
            enemyShooter.ResetBall(ballObj);
        }
    }
}