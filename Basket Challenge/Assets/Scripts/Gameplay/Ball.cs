using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private Transform basketTarget;

    private ShotType shotType;
    private BallOwner ballOwner = BallOwner.Player;
    private Rigidbody rb;
    private int points = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init(ShotType type, Transform target, BallOwner owner = BallOwner.Player)
    {
        shotType = type;
        basketTarget = target;
        ballOwner = owner;

        switch (type)
        {
            case ShotType.Normal: points = 2; break;
            case ShotType.Perfect: points = 3; break;
            case ShotType.Backboard: points = 2; break;
        }
    }

    public ShotType ShotType => shotType;
    public BallOwner GetBallOwner() => ballOwner;
    public void OverridePoints(int newPoints)
    {
        points = newPoints;
    }
    public int GetBallScore() => points;
}
