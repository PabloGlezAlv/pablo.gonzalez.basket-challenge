using UnityEngine;
using System.Collections;
using System;

public class BasketTrigger : MonoBehaviour
{
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private BallShooter shooter;
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private ScoreFlyer scoreFlyer;
    [SerializeField] private CameraDirector cameraDirector;
    [SerializeField] private FireballController fireballController;

    public static event Action<int> OnScored;

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            int score = ball.GetBallScore();
            OnScored?.Invoke(score);

            if (fireballController != null && fireballController.IsDoublePointsActive())
            {
                score *= 2;
            }

            scoreManager.AddScore(score);

            cameraDirector.OnScoreMade();   

            scoreFlyer.SetScore(score);
            scoreFlyer.gameObject.SetActive(true);

            StartCoroutine(ResetAfterDelay(ball.gameObject));
        }
    }
    
    private IEnumerator ResetAfterDelay(GameObject ballObj)
    {
        yield return new WaitForSeconds(resetDelay);
        if (shooter != null)
        {
            shooter.ResetBall(ballObj, true);
        }
    }
}