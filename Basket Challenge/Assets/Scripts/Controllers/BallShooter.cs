using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform shootingPosition;
    [SerializeField] private Transform basketTarget;
    [SerializeField] private GestureSliderController gestureController;
    [SerializeField] private PlayerAnimatorController playerAnimator;

    [Header("Physics")]
    [SerializeField, Range(20f, 80f)] private float launchAngle = 45f;

    [Header("Trajectory Preview")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool showTrajectory = true;
    [SerializeField, Range(10, 50)] private int trajectoryPoints = 30;

    private GameObject currentBall;

    private float pendingPower;
    private bool pendingPerfect;
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

    void CacheShot(float power, bool perfectShot)
    {
        pendingPower = power;
        pendingPerfect = perfectShot;
        hasPendingShot = true;
    }

    void OnReadyToShoot()
    {
        Debug.Log("BallShooter: Player ready, executing pending shot");
        if (!hasPendingShot) return;

        Shoot(pendingPower, pendingPerfect);
        hasPendingShot = false;
    }

    public void Shoot(float power, bool perfectShot)
    {
        if (basketTarget == null || gestureController == null) return;

        gestureController.DisableControls();

        CleanupBall();
        SpawnBall();
        if (currentBall == null) return;

        Vector3 velocity = CalculateParabolicVelocity(power);
        if (velocity == Vector3.zero) return;

        if (!perfectShot)
        {
            float powerVariation = Random.Range(0.85f, 1.2f);
            velocity *= powerVariation;
            float horizontalDeviation = Random.Range(-0.5f, 0.5f);
            Vector3 rightDirection = Vector3.Cross(Vector3.up, velocity.normalized);
            velocity += rightDirection * horizontalDeviation;
            float verticalDeviation = Random.Range(0.1f, 0.2f);
            velocity.y += verticalDeviation;
        }

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.velocity = velocity;
    }

    void SpawnBall()
    {
        currentBall = Instantiate(ballPrefab, shootingPosition.position, Quaternion.identity);
    }

    Vector3 CalculateParabolicVelocity(float power)
    {
        Vector3 startPos = shootingPosition.position;
        Vector3 targetPos = basketTarget.position;
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
            DrawTrajectoryGizmo(0.5f);
        }
    }
    
    void DrawTrajectoryGizmo(float power)
    {
        Vector3 velocity = CalculateParabolicVelocity(power);
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
