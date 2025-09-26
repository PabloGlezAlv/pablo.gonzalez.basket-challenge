using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FireballController : MonoBehaviour
{
    [Header("Ball")]
    [SerializeField] private GameObject fireball;
    [Header("UI")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private float increaseAmount = 0.2f;
    [SerializeField] private float decreaseSpeed = 0.1f;

    private bool doublePointsActive = false;
    private Coroutine decreaseRoutine;

    public bool IsDoublePointsActive()
    {
        return doublePointsActive;
    }   
    private void OnEnable()
    {
        powerSlider.value = 0;
        BasketTrigger.OnPlayerScored += HandleScored;
        FloorTrigger.OnMissed += HandleMissed;
    }

    private void OnDisable()
    {
        BasketTrigger.OnPlayerScored -= HandleScored;
        FloorTrigger.OnMissed -= HandleMissed;

        DeactivateDoublePoints();
    }

    private void HandleScored(int baseScore)
    {
        if(doublePointsActive) return;
        powerSlider.value += increaseAmount;

        if (powerSlider.value >= powerSlider.maxValue && !doublePointsActive)
        {
            ActivateDoublePoints();
        }
    }

    private void HandleMissed()
    {
        DeactivateDoublePoints();
        
        powerSlider.value = 0f;
    }

    private void ActivateDoublePoints()
    {
        doublePointsActive = true;
        fireball.SetActive(true);
        if (decreaseRoutine != null) StopCoroutine(decreaseRoutine);
        decreaseRoutine = StartCoroutine(DecreaseSlider());
    }

    private void DeactivateDoublePoints()
    {
        doublePointsActive = false;
        fireball.SetActive(false);
        if (decreaseRoutine != null)
        {
            StopCoroutine(decreaseRoutine);
            decreaseRoutine = null;
        }
    }

    private IEnumerator DecreaseSlider()
    {
        while (powerSlider.value > 0f)
        {
            powerSlider.value -= decreaseSpeed * Time.deltaTime;
            yield return null;
        }

        DeactivateDoublePoints();
    }
}
