using UnityEngine;

public class Ball : MonoBehaviour
{
    private int collisionCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        collisionCount++;
    }

    public int GetCollisionCount()
    {
        return collisionCount;
    }
}
