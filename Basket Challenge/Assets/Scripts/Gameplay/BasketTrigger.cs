using UnityEngine;
using System.Collections;

public class BasketTrigger : MonoBehaviour
{
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private BallShooter shooter;

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            StartCoroutine(ResetAfterDelay(ball.gameObject));
        }
    }

    private IEnumerator ResetAfterDelay(GameObject ballObj)
    {
        yield return new WaitForSeconds(resetDelay);

        if (shooter != null)
        {
            shooter.ResetBall(ballObj);
        }
    }
}
