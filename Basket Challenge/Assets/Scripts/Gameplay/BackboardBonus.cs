using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

[Serializable]
public class BackboardBonusOption
{
    [Range(0f, 1f)] public float probability = 0.33f;
    public Color color = Color.white;
    public int points = 1;
    public float activeDuration = 5f;
}

public class BackboardBonus : MonoBehaviour
{
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private float minActivationDelay = 5f;
    [SerializeField] private float maxActivationDelay = 20f;
    [SerializeField] private List<BackboardBonusOption> options = new List<BackboardBonusOption>();
   
    [SerializeField] private List<Image> imagesToTint = new List<Image>();
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private ParticleSystem particles;

    private bool hasTriggered;
    private bool isActive;
    private int activePoints;

    public bool IsActive => isActive;
    public int ActiveBonusPoints => isActive ? activePoints : 0;

    private void OnEnable()
    {
        if (gameTimer != null)
        {
            gameTimer.OnGameStarted += HandleGameStarted;
            gameTimer.OnGameEnded += HandleGameEnded;
        }
        SetVisualsActive(false);
    }

    private void OnDisable()
    {
        if (gameTimer != null)
        {
            gameTimer.OnGameStarted -= HandleGameStarted;
            gameTimer.OnGameEnded -= HandleGameEnded;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null)
        {
            ball.OverridePoints(activePoints);
        }
    }

    private void HandleGameStarted()
    {
        StopAllCoroutines();
        hasTriggered = false;
        isActive = false;
        activePoints = 0;
        SetVisualsActive(false);
        StartCoroutine(DelayedTryActivate(Random.Range(minActivationDelay, maxActivationDelay)));
    }

    private void HandleGameEnded()
    {
        StopAllCoroutines();
        Deactivate();
    }

    private IEnumerator DelayedTryActivate(float delay)
    {
        Debug.Log("BackboardBonus will try to activate in " + delay + " seconds.");
        if (hasTriggered) yield break;
        yield return new WaitForSeconds(delay);
        if (!hasTriggered) ActivateOnce();
    }

    private void ActivateOnce()
    {
        if (hasTriggered) return;
        var opt = ChooseWeightedOption();
        if (opt == null) return;

        hasTriggered = true;
        isActive = true;
        activePoints = Mathf.Max(0, opt.points);

        ApplyVisuals(opt.color, activePoints);
        StartCoroutine(ActiveWindow(opt.activeDuration));
    }

    private IEnumerator ActiveWindow(float duration)
    {
        yield return new WaitForSeconds(duration);
        Deactivate();
    }

    private void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        activePoints = 0;
        SetVisualsActive(false);
    }

    private BackboardBonusOption ChooseWeightedOption()
    {
        if (options == null || options.Count == 0) return null;
        float total = 0f;
        for (int i = 0; i < options.Count; i++) total += Mathf.Max(0f, options[i].probability);
        if (total <= 0f) return options[Random.Range(0, options.Count)];
        float roll = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < options.Count; i++)
        {
            acc += Mathf.Max(0f, options[i].probability);
            if (roll <= acc) return options[i];
        }
        return options[options.Count - 1];
    }

    private void ApplyVisuals(Color c, int points)
    {
        for (int i = 0; i < imagesToTint.Count; i++)
        {
            if (imagesToTint[i] != null)
            {
                imagesToTint[i].color = c;
            }
        }

        if (bonusText != null)
        {
            bonusText.color = c;
            bonusText.text = "+" + points.ToString();
        }

        if (particles != null)
        {
            var main = particles.main;
            main.startColor = c;
            particles.Play();
        }

        SetVisualsActive(true);
    }

    private void SetVisualsActive(bool on)
    {
        for (int i = 0; i < imagesToTint.Count; i++)
        {
            if (imagesToTint[i] != null) imagesToTint[i].gameObject.SetActive(on);
        }
        if (bonusText != null) bonusText.gameObject.SetActive(on);
        if (particles != null)
        {
            if (on)
            {
                if (!particles.isPlaying) particles.Play();
                particles.gameObject.SetActive(true);
            }
            else
            {
                if (particles.isPlaying) particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                particles.gameObject.SetActive(false);
            }
        }
    }
}
