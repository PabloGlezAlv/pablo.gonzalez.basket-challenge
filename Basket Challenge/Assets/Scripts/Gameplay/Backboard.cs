using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Backboard : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform basketTarget;

    [Header("Trajectory adjust")]
    [SerializeField] private float contactPushOff = 0.02f;
    [SerializeField] private float minFlightTime = 0.25f;
    [SerializeField] private float maxFlightTime = 1.5f;
    [SerializeField] private int solveIterations = 24;

    [Header("Direction restriction")]
    [SerializeField] private float minDownwardVy = 0.0f;

    private void OnCollisionEnter(Collision collision)
    {
        if (basketTarget == null) return;
        if (!collision.rigidbody) return;
        if (!collision.gameObject.TryGetComponent<Ball>(out var ball)) return;
        if (ball.ShotType != ShotType.Backboard) return;

        var rb = collision.rigidbody;
        float speed = rb.velocity.magnitude;
        if (speed < 1e-3f) return;

        var contact = collision.GetContact(0);
        Vector3 p0 = contact.point + contact.normal * contactPushOff;
        Vector3 pt = basketTarget.position;

        if (TrySolveDirectionKeepingSpeed_Downward(p0, pt, speed, out Vector3 vSolved))
        {
            rb.velocity = vSolved;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private IEnumerator ReenableCollision(Collider a, Collider b, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (a && b) Physics.IgnoreCollision(a, b, false);
    }

    // Uses the ballistic equation: |r - 0.5*g*t^2| = speed * t, solving for a downward direction (v.y <= -minDownwardVy).
    private bool TrySolveDirectionKeepingSpeed_Downward(Vector3 p0, Vector3 pt, float speed, out Vector3 vOut)
    {
        vOut = Vector3.zero;

        Vector3 r = pt - p0;
        Vector3 g = Physics.gravity;

        float F(float t)
        {
            Vector3 w = r - 0.5f * g * (t * t);
            return w.magnitude - speed * t;
        }

        bool BuildDownwardVelocity(float t, out Vector3 v)
        {
            v = Vector3.zero;
            if (t <= 1e-4f) return false;

            Vector3 w = r - 0.5f * g * (t * t);
            float denom = speed * t;
            if (denom <= 1e-4f) return false;

            Vector3 u = w / denom;
            float sq = u.sqrMagnitude;
            if (sq < 1e-6f) return false;

            u /= Mathf.Sqrt(sq);
            Vector3 vCandidate = u * speed;

            if (vCandidate.y <= -minDownwardVy)
            {
                v = vCandidate;
                return true;
            }
            return false;
        }

        float tMin = Mathf.Max(0.05f, minFlightTime);
        float tMax = Mathf.Max(tMin + 0.05f, maxFlightTime);

        const int coarseSamples = 20;
        float bestErr = float.MaxValue;
        Vector3 bestV = Vector3.zero;
        bool found = false;
        float bestT = tMin;

        for (int i = 0; i <= coarseSamples; i++)
        {
            float t = Mathf.Lerp(tMin, tMax, i / (float)coarseSamples);
            if (BuildDownwardVelocity(t, out Vector3 vCand))
            {
                float err = Mathf.Abs(F(t));
                if (err < bestErr)
                {
                    bestErr = err;
                    bestV = vCand;
                    bestT = t;
                    found = true;
                }
            }
        }
        if (!found) return false;

        float left = Mathf.Max(tMin, bestT * 0.5f);
        float right = Mathf.Min(tMax, bestT * 1.5f);

        for (int iter = 0; iter < solveIterations; iter++)
        {
            float tMid = 0.5f * (left + right);
            float t1 = 0.5f * (tMid + right);
            float t2 = 0.5f * (left + tMid);

            bool improved = false;

            if (BuildDownwardVelocity(t1, out Vector3 v1))
            {
                float e1 = Mathf.Abs(F(t1));
                if (e1 + 1e-5f < bestErr)
                {
                    bestErr = e1; bestV = v1; bestT = t1;
                    left = tMid;
                    improved = true;
                }
            }

            if (BuildDownwardVelocity(t2, out Vector3 v2))
            {
                float e2 = Mathf.Abs(F(t2));
                if (e2 + 1e-5f < bestErr)
                {
                    bestErr = e2; bestV = v2; bestT = t2;
                    right = tMid;
                    improved = true;
                }
            }

            if (!improved) break;
        }

        vOut = bestV;
        return true;
    }
}
