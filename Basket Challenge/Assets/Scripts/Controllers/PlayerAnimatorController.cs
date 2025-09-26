
using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform lookAtTarget;

    private Animator animator;
    private Coroutine moveCoroutine;
    private Vector3 initPosition;
    private Quaternion initRotation;

    public event Action OnReadyToShootEvent;
    public event Action OnGameStarted;

    public bool IsInGame { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        initPosition = transform.position;
        initRotation = transform.rotation;
    }

    public void GoGame(Vector3 target)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        animator.SetTrigger("goGame");
        moveCoroutine = StartCoroutine(MoveToTarget(target));
    }

    public void ReadyToShoot()
    {
        OnReadyToShootEvent?.Invoke();
    }

    public void GoMenu()
    {
        IsInGame = false;
        transform.position = initPosition;
        transform.rotation = initRotation;
        animator.SetTrigger("goMenu");
    }

    public void Shoot()
    {
        animator.SetTrigger("shoot");
    }

    public void StopMoving()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        animator.SetBool("moving", false);
    }

    public void Teleport(Vector3 position)
    {
        transform.position = position;
        if (lookAtTarget != null) transform.LookAt(lookAtTarget.position);
    }

    private IEnumerator MoveToTarget(Vector3 target)
    {
        animator.SetBool("moving", true);
        target.y = transform.position.y;
        transform.LookAt(target);

        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            transform.position = newPos;
            yield return null;
        }

        animator.SetBool("moving", false);
        moveCoroutine = null;

        if (lookAtTarget != null) transform.LookAt(lookAtTarget.position);

        IsInGame = true;
        OnGameStarted?.Invoke();
    }
}