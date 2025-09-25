using UnityEngine;
using System.Collections;

public class BasketTrigger : MonoBehaviour
{
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private BallShooter shooter;
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private ScoreFlyer scoreFlyer;

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            int score = ball.GetBallScore();
            scoreManager.AddScore(score);

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