using UnityEngine;

public class PlayerCurvePositioner : MonoBehaviour
{
    [SerializeField] private PlayerAnimatorController player;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform controlPoint;
    [SerializeField] private bool lockYToPlayer = true;

    private Vector3 lastSampledPoint;

    public void ResetPlayerInstant()
    {
        if (player == null || pointA == null || pointB == null) return;
        float t = Random.Range(0, 1);
        lastSampledPoint = SampleBezier(pointA.position, controlPoint.position, pointB.position, t);
        if (lockYToPlayer) lastSampledPoint.y = player.transform.position.y;
        player.Teleport(lastSampledPoint);
    }

    public Vector3 GetLastSampledPoint()
    {
        return lastSampledPoint;
    }

    // Quadratic Bezier curve sampling
    private Vector3 SampleBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(ab, bc, t);
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;
        Vector3 cp = controlPoint.position;
        Vector3 prev = pointA.position;
        for (int i = 1; i <= 32; i++)
        {
            float t = i / 32f;
            Vector3 p = SampleBezier(pointA.position, cp, pointB.position, t);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointA.position, 0.08f);
        Gizmos.DrawSphere(pointB.position, 0.08f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(cp, 0.06f);
    }
}
