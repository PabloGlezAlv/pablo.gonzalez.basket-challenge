using UnityEngine;
using System;

public class AIEnemyShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private Ball ballScript;
    [SerializeField] private Transform shootingPosition;
    [SerializeField] private Transform basketTarget;
    [SerializeField] private AIEnemyController enemyController;
    
    [Header("Physics")]
    [SerializeField, Range(20f, 80f)] private float launchAngle = 45f;
    [SerializeField, Range(0f, 0.01f)] private float aimAccuracy = 0.005f;
    
    [Header("AI Shooting Behavior")]
    [SerializeField, Range(0f, 1f)] private float perfectShotChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float normalShotChance = 0.6f;
    
    public event Action<Rigidbody> OnEnemyBallShooted;
    public GameObject CurrentBall => ballRb != null ? ballRb.gameObject : null;
    
    void Start()
    {
        if (enemyController == null)
            enemyController = GetComponent<AIEnemyController>();
            
        if (enemyController != null)
            enemyController.OnEnemyShoot += OnEnemyShoot;
    }
    
    void OnDestroy()
    {
        if (enemyController != null)
            enemyController.OnEnemyShoot -= OnEnemyShoot;
    }
    
    private void OnEnemyShoot()
    {
        if (!enemyController.IsGameActive) return;
        
        ShotType shotType = DetermineAIShotType();
        Shoot(shotType);
    }
    
    private ShotType DetermineAIShotType()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        
        if (random <= perfectShotChance)
            return ShotType.Perfect;
        else if (random <= perfectShotChance + normalShotChance)
            return ShotType.Normal;
        else
            return ShotType.Backboard;
    }
    
    public void Shoot(ShotType shotType)
    {
        if (shootingPosition == null || ballRb == null || basketTarget == null) return;
        
        CleanupBall();
        if (ballScript != null) ballScript.Init(shotType, basketTarget, BallOwner.Enemy);
        
        Vector3 velocity = Vector3.zero;
        
        switch (shotType)
        {
            case ShotType.Perfect:
                velocity = CalculateParabolicVelocity();
                break;
                
            case ShotType.Normal:
                velocity = CalculateNormalShot();
                break;
                
            case ShotType.Backboard:
                velocity = CalculateBackboardShot();
                break;
        }
        
        if (velocity == Vector3.zero) return;
        
        OnEnemyBallShooted?.Invoke(ballRb);
        ballRb.velocity = velocity;
    }
    
    private Vector3 CalculateParabolicVelocity()
    {
        return CalculateParabolicVelocityTo(basketTarget.position);
    }
    
    private Vector3 CalculateNormalShot()
    {
        Vector3 targetPos = basketTarget.position;
        
        float randomOffset = UnityEngine.Random.Range(-aimAccuracy, aimAccuracy);
        targetPos.x += randomOffset;
        targetPos.z += randomOffset;
        
        return CalculateParabolicVelocityTo(targetPos);
    }
    
    private Vector3 CalculateBackboardShot()
    {
        Vector3 targetPos = basketTarget.position;
        targetPos.z += 1f;
        targetPos.y += UnityEngine.Random.Range(0f, 0.5f);
        
        return CalculateParabolicVelocityTo(targetPos);
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
        ballObj.SetActive(false);
    }
}