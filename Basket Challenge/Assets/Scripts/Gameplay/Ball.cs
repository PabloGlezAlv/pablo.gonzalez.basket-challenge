using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private Transform basketTarget;

    private ShotType shotType;
    private Rigidbody rb;
    private int points = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init(ShotType type, Transform target)
    {
        shotType = type;
        basketTarget = target;

        switch (type)
        {
            case ShotType.Normal: points = 2; break;
            case ShotType.Perfect: points = 3; break;
            case ShotType.Backboard: points = 2; break;
        }
    }

    // ✅ único expuesto
    public ShotType ShotType => shotType;

    public int GetBallScore() => points;
}
