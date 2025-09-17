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

    [Header("Backboard")]
    [SerializeField] private Backboard backboard;
    [SerializeField] private BoxCollider backboardCollider;
    [SerializeField, Range(0f, 0.5f)] private float backboardMarginX = 0.05f;
    [SerializeField, Range(0f, 0.5f)] private float backboardMarginY = 0.05f;

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
        SpawnBall(shotType);
        if (currentBall == null) return;

        Vector3 velocity = Vector3.zero;

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
                    {
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
                    }

                case ShotType.Backboard:
                    {
                        if (backboardCollider == null) { velocity = CalculateParabolicVelocity(); break; }
                        Vector3 impact = GetBackboardImpactPoint();
                        velocity = CalculateParabolicVelocityTo(impact);
                        if (velocity == Vector3.zero) velocity = CalculateParabolicVelocity();
                        break;
                    }
            }
        }

        if (velocity == Vector3.zero) return;

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.velocity = velocity;
    }

    void SpawnBall(ShotType shotType)
    {
        currentBall = Instantiate(ballPrefab, shootingPosition.position, Quaternion.identity);
        var b = currentBall.GetComponent<Ball>();
        if (b != null) b.Init(shotType, basketTarget);
    }

    Vector3 ReflectPointAcrossPlane(Vector3 p, Vector3 planePoint, Vector3 planeNormal)
    {
        float d = Vector3.Dot(planeNormal, p - planePoint);
        return p - 2f * d * planeNormal;
    }

    Vector3 CalculateParabolicVelocity()
    {
        return CalculateParabolicVelocityTo(basketTarget != null ? basketTarget.position : shootingPosition.position + shootingPosition.forward * 5f);
    }

    //Methos that calculates the initial velocity needed to hit a target position with a parabolic arc
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

    Vector3 GetBackboardImpactPoint()
    {
        var t = backboardCollider.transform;
        var c = backboardCollider.center;
        var s = backboardCollider.size;
        Vector3 local = t.InverseTransformPoint(basketTarget.position);
        float minX = c.x - s.x * 0.5f + backboardMarginX;
        float maxX = c.x + s.x * 0.5f - backboardMarginX;
        float minY = c.y - s.y * 0.5f + backboardMarginY;
        float maxY = c.y + s.y * 0.5f - backboardMarginY;
        local.x = Mathf.Clamp(local.x, minX, maxX);
        local.y = Mathf.Clamp(local.y, minY, maxY);
        local.z = c.z + s.z * 0.5f - 0.01f;
        return t.TransformPoint(local);
    }

    public static Vector3 CalculateParabolicVelocityBetweenStatic(Vector3 startPos, Vector3 targetPos, float launchAngleDeg)
    {
        Vector3 displacement = targetPos - startPos;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;
        float verticalDistance = displacement.y;
        float angle = launchAngleDeg * Mathf.Deg2Rad;
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
