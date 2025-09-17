using UnityEngine;

public enum ShotType
{
    Normal,
    Perfect,
    Backboard
}

public class BallShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform shootingPosition;
    [SerializeField] private Transform basketTarget;
    [SerializeField] private Transform rimReference;

    [SerializeField] private GestureSliderController gestureController;
    [SerializeField] private PlayerAnimatorController playerAnimator;

    [Header("Physics")]
    [SerializeField, Range(20f, 80f)] private float launchAngle = 45f;
    [SerializeField, Range(0f, 0.005f)] private float rimMargin = 0.005f;
    [Header("Trajectory Preview")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool showTrajectory = true;
    [SerializeField, Range(10, 50)] private int trajectoryPoints = 30;

    private GameObject currentBall;

    private ShotType shotType;
    private bool hasPendingShot;

    void Start()
    {
        if (gestureController == null) gestureController = FindObjectOfType<GestureSliderController>(true);
        if (playerAnimator == null) playerAnimator = FindObjectOfType<PlayerAnimatorController>();

        if (gestureController != null)
            gestureController.OnShoot += CacheShot;

        if (playerAnimator != null)
            playerAnimator.OnReadyToShootEvent += OnReadyToShoot;
    }

    void OnDestroy()
    {
        if (gestureController != null)
            gestureController.OnShoot -= CacheShot;

        if (playerAnimator != null)
            playerAnimator.OnReadyToShootEvent -= OnReadyToShoot;
    }

    void CacheShot(ShotType type)
    {
        shotType = type;
        hasPendingShot = true;
    }

    void OnReadyToShoot()
    {
        if (!hasPendingShot) return;

        Shoot(shotType);
        hasPendingShot = false;
    }

    public void Shoot(ShotType shotType)
    {
        if (gestureController == null || shootingPosition == null) return;

        gestureController.DisableControls();

        CleanupBall();
        SpawnBall();
        if (currentBall == null) return;

        Vector3 velocity = Vector3.zero;

        Debug.Log("Shootinhg with shot type: " + shotType);

        if (basketTarget == null)
        {
            Debug.LogError("BallShooter: basketTarget no asignado.");
            Vector3 randomDir = shootingPosition.forward +
                                new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.3f), Random.Range(-0.2f, 0.2f));
            randomDir.Normalize();
            velocity = randomDir * Random.Range(5f, 10f);
        }
        else
        {
            switch (shotType)
            {
                case ShotType.Perfect:
                    velocity = CalculateParabolicVelocity();
                    break;

                case ShotType.Normal:
                    if (rimReference == null) { velocity = CalculateParabolicVelocity(); break; }
                    float rimRadius = Vector3.Distance(rimReference.position, basketTarget.position);
                    float ang = Random.Range(0f, Mathf.PI * 2f);
                    float delta = Random.Range(-rimMargin, rimMargin);
                    float r = Mathf.Max(0.01f, rimRadius + delta);
                    Vector3 ringOffset = new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
                    Vector3 ringTarget = new Vector3(
                        basketTarget.position.x + ringOffset.x,
                        basketTarget.position.y,
                        basketTarget.position.z + ringOffset.z
                    );
                    velocity = CalculateParabolicVelocityTo(ringTarget);
                    if (velocity == Vector3.zero) velocity = CalculateParabolicVelocity();
                    break;

                case ShotType.Backboard:
                    break;
            }
        }

        if (velocity == Vector3.zero) return;

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.velocity = velocity;
    }



    void SpawnBall()
    {
        currentBall = Instantiate(ballPrefab, shootingPosition.position, Quaternion.identity);
    }

    Vector3 CalculateParabolicVelocity()
    {
        return CalculateParabolicVelocityTo(basketTarget != null ? basketTarget.position : shootingPosition.position + shootingPosition.forward * 5f);
    }

    Vector3 CalculateParabolicVelocityTo(Vector3 targetPos)
    {
        Vector3 startPos = shootingPosition.position;
        Vector3 displacement = targetPos - startPos;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;
        float verticalDistance = displacement.y;
        float angle = launchAngle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y);

        float denom = 2 * Mathf.Cos(angle) * Mathf.Cos(angle) * (horizontalDistance * Mathf.Tan(angle) - verticalDistance);
        if (denom <= 0f) return Vector3.zero;

        float velocityMagnitude = Mathf.Sqrt((gravity * horizontalDistance * horizontalDistance) / denom);
        if (float.IsNaN(velocityMagnitude) || float.IsInfinity(velocityMagnitude)) return Vector3.zero;

        Vector3 horizontalDirection = horizontalDisplacement.normalized;
        Vector3 velocity = horizontalDirection * velocityMagnitude * Mathf.Cos(angle);
        velocity.y = velocityMagnitude * Mathf.Sin(angle);
        return velocity;
    }


    void CleanupBall()
    {
        if (currentBall != null) Destroy(currentBall);
    }

    public void ResetBall(GameObject ballObj)
    {
        if (ballObj != null) Destroy(ballObj);
        if (gestureController != null) gestureController.EnableControls();
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (shootingPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(shootingPosition.position, 0.2f);
        }

        if (basketTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(basketTarget.position, 0.3f);

            if (shootingPosition != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(shootingPosition.position, basketTarget.position);
            }
        }

        if (showTrajectory && basketTarget != null && shootingPosition != null)
        {
            DrawTrajectoryGizmo();
        }
    }

    void DrawTrajectoryGizmo()
    {
        Vector3 velocity = CalculateParabolicVelocity();
        if (velocity == Vector3.zero) return;

        Vector3 currentPos = shootingPosition.position;
        Vector3 currentVel = velocity;
        float timeStep = 0.1f;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            Vector3 nextPos = currentPos + currentVel * timeStep;
            nextPos.y += 0.5f * Physics.gravity.y * timeStep * timeStep;

            Gizmos.DrawLine(currentPos, nextPos);

            currentPos = nextPos;
            currentVel.y += Physics.gravity.y * timeStep;

            if (currentPos.y < basketTarget.position.y - 5f)
                break;
        }
    }
}
