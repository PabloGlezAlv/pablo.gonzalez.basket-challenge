using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultController : MonoBehaviour
{
    [Header("Victory Panel")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryScoreText;

    [Header("Defeat Panel")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TextMeshProUGUI defeatScoreText;

    [Header("Stars")]
    [SerializeField] private Image[] stars;

    [Header("Settings")]
    [SerializeField] private int pointsPerStar = 20;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private float delayBetweenStars = 0.5f;

    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;

    private bool isAnimating = false;

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        InitializeStars();

        ShowResults();
    }

    private void InitializeStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].transform.localScale = Vector3.zero;
            }
        }
    }

    public void ShowResults()
    {
        if (isAnimating) return;

        int currentScore = GetCurrentScore();
        bool isVictory = currentScore > 0;

        if (isVictory)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            if (victoryScoreText != null)
            {
                victoryScoreText.text = currentScore.ToString();
            }

            StartCoroutine(AnimateStarsBasedOnScore(currentScore));
        }
        else
        {
            if (defeatPanel != null) defeatPanel.SetActive(true);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            if (defeatScoreText != null)
            {
                defeatScoreText.text = currentScore.ToString();
            }
        }
    }

    private int GetCurrentScore()
    {
        return scoreManager.GetTotalScore();
    }

    private IEnumerator AnimateStarsBasedOnScore(int totalScore)
    {
        isAnimating = true;

        int fullStars = totalScore / pointsPerStar;
        int remainingPoints = totalScore % pointsPerStar;

        for (int i = 0; i < fullStars && i < stars.Length; i++)
        {
            yield return StartCoroutine(AnimateStarToScale(stars[i], 1f));
            yield return new WaitForSeconds(delayBetweenStars);
        }

        if (fullStars < stars.Length && remainingPoints > 0)
        {
            float partialScale = (float)remainingPoints / pointsPerStar;
            yield return StartCoroutine(AnimateStarToScale(stars[fullStars], partialScale));
        }

        isAnimating = false;
    }

    private IEnumerator AnimateStarToScale(Image star, float targetScale)
    {
        if (star == null) yield break;

        Vector3 startScale = star.transform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsedTime = 0f;
        float duration = 1f / animationSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            star.transform.localScale = Vector3.Lerp(startScale, endScale, smoothProgress);

            yield return null;
        }

        star.transform.localScale = endScale;
    }

    public void ResetStars()
    {
        StopAllCoroutines();
        isAnimating = false;
        InitializeStars();

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }
}