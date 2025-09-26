using UnityEngine;
using System.Collections;
using System;

public class FloorTrigger : MonoBehaviour
{
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private BallShooter shooter;

    public static event Action OnMissed;

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            OnMissed?.Invoke();
            
            AudioManager.Instance.PlaySound(SoundType.SFX_Miss);
                
            StartCoroutine(ResetAfterDelay(ball.gameObject));
        }
    }

    private IEnumerator ResetAfterDelay(GameObject ballObj)
    {
        yield return new WaitForSeconds(resetDelay);

        if (shooter != null)
        {
            shooter.ResetBall(ballObj, false);
        }
    }
}
