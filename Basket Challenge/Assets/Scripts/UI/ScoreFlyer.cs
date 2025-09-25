using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreFlyer : MonoBehaviour
{
    [SerializeField] private float moveDistance = 20f;   
    [SerializeField] private float scaleTime = 0.5f;    
    [SerializeField] private float displayTime = 0.5f;   
    [SerializeField] private TextMeshProUGUI textMesh;

    private Vector3 startPos;
    private Vector3 targetPos;

    public void SetScore(int score)
    {
        textMesh.text = "+" + score;
    }

    private void OnEnable()
    {
        startPos = transform.localPosition;
        targetPos = startPos + new Vector3(0, 0, moveDistance);

        transform.localScale = Vector3.zero;

        StartCoroutine(AnimateFlyer());
    }

    // Coroutine to handle the animation first go up and then stay
    private IEnumerator AnimateFlyer()
    {
        float t = 0f;

        while (t < scaleTime)
        {
            t += Time.deltaTime;
            float fraction = t / scaleTime;

            transform.localPosition = Vector3.Lerp(startPos, targetPos, fraction);
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, fraction);

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localScale = Vector3.one;

        yield return new WaitForSeconds(displayTime);

        gameObject.SetActive(false);
    }
}
