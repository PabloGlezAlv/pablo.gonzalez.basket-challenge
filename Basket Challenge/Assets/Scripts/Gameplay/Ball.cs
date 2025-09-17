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
            case ShotType.Normal:
                points = 2;
                break;
            case ShotType.Perfect:
                points = 3;
                break;
            case ShotType.Backboard:
                points = 2;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (basketTarget == null || shotType != ShotType.Backboard) return;
        if (collision.collider.GetComponent<Backboard>() == null && collision.collider.GetComponentInParent<Backboard>() == null) return;

        Vector3 p0 = collision.GetContact(0).point;
        Vector3 n = collision.GetContact(0).normal.normalized;
        Vector3 vin = rb.velocity;
        if (vin.sqrMagnitude < 0.01f) return;

        Vector3 d = Vector3.Reflect(vin.normalized, n).normalized;
        float k = SolveBallisticSpeed(d, p0, basketTarget.position);
        if (k <= 0f)
        {
            Vector3 d2 = (basketTarget.position - p0).normalized;
            k = SolveBallisticSpeed(d2, p0, basketTarget.position);
            if (k <= 0f) return;
            d = d2;
        }
        rb.velocity = d * k;
    }
    
    // fake the backboard shot in case it hit the backbord the ball goes in
    float SolveBallisticSpeed(Vector3 dir, Vector3 p0, Vector3 pt)
    {
        Vector3 sh = new Vector3(pt.x - p0.x, 0f, pt.z - p0.z);
        float dh = sh.magnitude;
        if (dh < 0.01f) return 0f;
        float dy = pt.y - p0.y;
        float vrh = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
        if (vrh < 1e-3f) return 0f;
        float vry = dir.y;
        float g = Mathf.Abs(Physics.gravity.y);
        float denom = (vry / vrh) * dh - dy;
        if (denom <= 0f) return 0f;
        float k2 = (g * dh * dh) / (2f * vrh * vrh * denom);
        if (k2 <= 0f || float.IsNaN(k2) || float.IsInfinity(k2)) return 0f;
        return Mathf.Sqrt(k2);
    }

    public int GetBallScore()
    {
        return points;
    }
}