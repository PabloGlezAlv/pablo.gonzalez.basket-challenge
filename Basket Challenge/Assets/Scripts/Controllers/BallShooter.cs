using System;
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
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private Ball ballScript;
    [SerializeField] private Transform shootingPosition;
    [SerializeField] private Transform basketTarget;
    [SerializeField] private Transform rimReference;
    [SerializeField] private GestureSliderController gestureController;
    [SerializeField] private PlayerAnimatorController playerAnimator;
    [SerializeField] private PlayerCurvePositioner playerPositionManager;

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

    private ShotType shotType;
    private bool hasPendingShot;

    public event Action<GameObject> OnBallSpawned;
    public event Action OnTurnEnded;

    public GameObject CurrentBall => ballRb != null ? ballRb.gameObject : null;

    void Start()
    {
        if (gestureController == null) gestureController = FindObjectOfType<GestureSliderController>(true);
        if (playerAnimator == null) playerAnimator = FindObjectOfType<PlayerAnimatorController>();

        if (gestureController != null)
            gestureController.OnShoot += CacheShot;

        if (playerAnimator != null)
            playerAnimator.OnReadyToShootEvent += OnReadyToShoot;

        if (ballRb != null)
            OnBallSpawned?.Invoke(ballRb.gameObject);
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
        if (gestureController == null || shootingPosition == null || ballRb == null) return;

        gestureController.DisableControls();

        CleanupBall();
        if (ballScript != null) ballScript.Init(shotType, basketTarget);

        Vector3 velocity = Vector3.zero;

        if (basketTarget == null)
        {
            Vector3 randomDir = shootingPosition.forward +
                                new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f),
                                            UnityEngine.Random.Range(0.1f, 0.3f),
                                            UnityEngine.Random.Range(-0.2f, 0.2f));
            randomDir.Normalize();
            velocity = randomDir * UnityEngine.Random.Range(5f, 10f);
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
                        float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                        float delta = UnityEngine.Random.Range(-rimMargin, rimMargin);
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
        ballRb.velocity = velocity;
    }

    Vector3 CalculateParabolicVelocity()
    {
        return CalculateParabolicVelocityTo(basketTarget != null ? basketTarget.position : shootingPosition.position + shootingPosition.forward * 5f);
    }

    //Method to calculate the initial velocity needed to hit a target at a certain angle
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

    void CleanupBall()
    {
        ballRb.gameObject.SetActive(true);

        ballRb.position = shootingPosition.position;
        ballRb.rotation = shootingPosition.rotation;

        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Physics.SyncTransforms();

        ballRb.WakeUp();
    }


    public void ResetBall(GameObject ballObj)
    {
        if (playerPositionManager != null) playerPositionManager.ResetPlayerInstant();
        if (gestureController != null) gestureController.EnableControls();
        ballObj.SetActive(false);

        OnTurnEnded?.Invoke();
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
