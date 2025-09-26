using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Animator))]
public class AIEnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimatorController mainPlayer;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float offsetDistance = 3f;
    
    [Header("AI Settings")]
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float shootIntervalVariation = 0.5f;
    
    private Animator animator;
    private bool isGameActive = false;
    private Coroutine shootingCoroutine;
    private GameTimer gameTimer;
    
    public event Action OnEnemyShoot;
    public bool IsGameActive => isGameActive;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        if (mainPlayer != null)
        {
            mainPlayer.OnGameStarted += OnGameStarted;
        }
        
        gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            gameTimer.OnGameEnded += OnGameEnded;
        }
        
        gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        if (mainPlayer != null)
        {
            mainPlayer.OnGameStarted -= OnGameStarted;
        }
        
        if (gameTimer != null)
        {
            gameTimer.OnGameEnded -= OnGameEnded;
        }
    }
    
    void Update()
    {
        if (isGameActive && mainPlayer != null)
        {
            FollowPlayer();
        }
    }
    
    private void FollowPlayer()
    {
        Vector3 targetPosition = mainPlayer.transform.position + (mainPlayer.transform.right * offsetDistance);
        targetPosition.y = transform.position.y;
        transform.position = targetPosition;
        
        if (lookAtTarget != null)
        {
            transform.LookAt(lookAtTarget.position);
        }
    }
    
    private void OnGameStarted()
    {
        isGameActive = true;
        gameObject.SetActive(true);
        
        if (mainPlayer != null)
        {
            Vector3 initialPosition = mainPlayer.transform.position + (mainPlayer.transform.right * offsetDistance);
            initialPosition.y = transform.position.y;
            transform.position = initialPosition;
            
            if (lookAtTarget != null)
            {
                transform.LookAt(lookAtTarget.position);
            }
        }
        
        StartShooting();
    }
    
    private void OnGameEnded()
    {
        isGameActive = false;
        StopShooting();
        gameObject.SetActive(false);
    }
    
    private void StartShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
        }
        shootingCoroutine = StartCoroutine(ShootingLoop());
    }
    
    private void StopShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }
    
    private IEnumerator ShootingLoop()
    {
        while (isGameActive)
        {
            float waitTime = shootInterval + UnityEngine.Random.Range(-shootIntervalVariation, shootIntervalVariation);
            waitTime = Mathf.Max(0.5f, waitTime);
            
            yield return new WaitForSeconds(waitTime);
            
            if (isGameActive)
            {
                Shoot();
            }
        }
    }
    
    public void Shoot()
    {
        if (!isGameActive) return;
        
        animator.SetTrigger("shoot");
    }
    
    public void OnAnimationShoot()
    {
        OnEnemyShoot?.Invoke();
    }
    
}