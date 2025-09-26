using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    public enum CamState { Idle, FollowBall, ScorePause }

    [SerializeField] private BallShooter shooter;
    [SerializeField] private Transform player;
    [SerializeField] private Transform basket;
    [SerializeField] private CameraMenuController menuController;

    [SerializeField, Range(2f, 15f)] private float behindDistance = 6f;
    [SerializeField, Range(1f, 6f)] private float behindHeight = 3f;
    [SerializeField] private Vector3 ballOffset = new Vector3(0f, 1.2f, -2.5f);

    [SerializeField, Range(0f, 5f)] private float activationDelay = 2f;
    [SerializeField, Range(0.5f, 10f)] private float followSmooth = 5f;
    [SerializeField, Range(0.5f, 10f)] private float idleSmooth = 4f;

    [SerializeField] private float scorePauseTime = 1.5f;

    private CamState state = CamState.Idle;
    private Transform ball;
    private bool activeControl;
    private float scorePauseTimer = 0f;

    private void Awake()
    {
        if (shooter != null)
        {
            shooter.OnBallShooted += HandleBallShooted;
            shooter.OnTurnEnded += HandleTurnEnded;
        }
        if (menuController != null)
        {
            menuController.OnStateChanged += HandleMenuStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (shooter != null)
        {
            shooter.OnBallShooted -= HandleBallShooted;
            shooter.OnTurnEnded -= HandleTurnEnded;
        }
        if (menuController != null)
        {
            menuController.OnStateChanged -= HandleMenuStateChanged;
        }
    }

    private void Start()
    {
        activeControl = menuController == null || menuController.GetCurrentState() == CameraMenuController.CameraState.Gameplay;
        if (activeControl)
        {
            transform.position = GetBehindPlayerPosition();
            LookAtFrame();
        }
    }

    private void LateUpdate()
    {
        if (!activeControl) return;
        switch (state)
        {
            case CamState.FollowBall:
                UpdateFollowBall();
                break;
            case CamState.Idle:
                UpdateIdle();
                break;
            case CamState.ScorePause:
                UpdateScorePause();
                break;
        }
    }

    public void BeginFollow(Transform t)
    {
        ball = t;
        state = CamState.FollowBall;
    }

    public void EndFollow()
    {
        ball = null;
        state = CamState.Idle;
    }

    public void OnScoreMade()
    {
        state = CamState.ScorePause;
        scorePauseTimer = 0f;
        ball = null;
    }

    private void UpdateScorePause()
    {
        scorePauseTimer += Time.deltaTime;
        LookAtBasket();
        if (scorePauseTimer >= scorePauseTime) state = CamState.Idle;
    }

    void HandleMenuStateChanged(CameraMenuController.CameraState st)
    {
        StopAllCoroutines();
        if (st == CameraMenuController.CameraState.Gameplay)
        {
            StartCoroutine(ActivateAfterDelay());
        }
        else
        {
            activeControl = false;
        }
    }

    System.Collections.IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        activeControl = true;
        state = CamState.Idle;
    }

    void HandleBallShooted(Rigidbody rb)
    {
        if (!activeControl) return;
        ball = rb != null ? rb.transform : null;
        if (ball != null) state = CamState.FollowBall;
    }

    void HandleTurnEnded()
    {
        if (!activeControl) return;
        ball = null;
        state = CamState.Idle;
    }

    void UpdateFollowBall()
    {
        if (ball == null || !ball.gameObject.activeInHierarchy)
        {
            state = CamState.Idle;
            return;
        }
        Vector3 targetPos = ball.position + ballOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.LookAt(ball.position, Vector3.up);
    }

    void UpdateIdle()
    {
        Vector3 targetPos = GetBehindPlayerPosition();
        transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-idleSmooth * Time.deltaTime));
        LookAtBasket();
    }

    Vector3 GetForwardToTarget()
    {
        if (player == null) return transform.forward;
        Vector3 target = (basket != null) ? basket.position : (player.position + player.forward);
        Vector3 dir = target - player.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) return transform.forward;
        return dir.normalized;
    }

    Vector3 GetBehindPlayerPosition()
    {
        if (player == null) return transform.position;
        Vector3 fwdToTarget = GetForwardToTarget();
        Vector3 p = player.position - fwdToTarget * behindDistance;
        p.y += behindHeight;
        return p;
    }

    void LookAtFrame()
    {
        if (player == null) return;
        Vector3 lookPoint = player.position;
        if (basket != null) lookPoint = Vector3.Lerp(player.position, basket.position, 0.5f);
        Quaternion r = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, r, 0.2f);
    }

    void LookAtBasket()
    {
        if (basket == null) return;
        Quaternion r = Quaternion.LookRotation(basket.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, r, 0.2f);
    }
}
