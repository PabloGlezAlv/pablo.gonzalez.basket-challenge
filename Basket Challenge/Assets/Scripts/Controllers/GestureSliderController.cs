using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class GestureSliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionAsset playerControls;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private RectTransform sliderBackground;
    [SerializeField] private PlayerAnimatorController playerAnimator;

    [Header("Settings")]
    [SerializeField] private float gestureTimeLimit = 3f;
    [SerializeField] private float sensitivity = 0.001f;

    [Header("Perfect Shot Settings")]
    [SerializeField] private float perfectShotRangeSize = 0.15f;

    [Header("Backboard Shot Settings")]
    [SerializeField] private float backboardShotRangeSize = 0.15f;

    [Header("Visual Zones")]
    [SerializeField] private GameObject perfectZoneIndicator;
    [SerializeField] private GameObject backboardZoneIndicator;

    private InputAction touchPressAction;
    private InputAction touchDeltaAction;
    private InputActionMap touchScreenMap;

    private bool isGestureActive = false;
    private bool shotInFlight = false;
    private float currentPower = 0f;
    private Coroutine gestureTimerCoroutine;

    private float perfectShotCenter;
    private float perfectShotMin;
    private float perfectShotMax;

    private float backboardShotCenter;
    private float backboardShotMin;
    private float backboardShotMax;

    private RectTransform perfectZoneRect;
    private RectTransform backboardZoneRect;

    private float sliderWidth;

    public System.Action<ShotType> OnShoot;

    void Awake()
    {
        touchScreenMap = playerControls.FindActionMap("TouchScreen");
        touchPressAction = touchScreenMap.FindAction("TouchPress");
        touchDeltaAction = touchScreenMap.FindAction("TouchDelta");
        touchPressAction.started += OnTouchStart;
        touchPressAction.canceled += OnTouchEnd;
    }

    void OnEnable()
    {
        playerControls?.Enable();
        if (playerAnimator != null) playerAnimator.OnGameStarted += EnableControls;
        DisableControls();
    }

    void OnDisable()
    {
        if (playerAnimator != null) playerAnimator.OnGameStarted -= EnableControls;
        touchScreenMap.Disable();
        playerControls?.Disable();
        if (gestureTimerCoroutine != null) StopCoroutine(gestureTimerCoroutine);
        isGestureActive = false;
    }

    void Start()
    {
        if (perfectZoneIndicator != null)
        {
            perfectZoneRect = perfectZoneIndicator.GetComponent<RectTransform>();
            perfectZoneIndicator.SetActive(false);
        }
        if (backboardZoneIndicator != null)
        {
            backboardZoneRect = backboardZoneIndicator.GetComponent<RectTransform>();
            backboardZoneIndicator.SetActive(false);
        }
        sliderWidth = sliderBackground.rect.width;
        powerSlider.value = 0f;
    }
    void OnDestroy()
    {
        if (touchPressAction != null)
        {
            touchPressAction.started -= OnTouchStart;
            touchPressAction.canceled -= OnTouchEnd;
        }
    }

    public void EnableControls()
    {
        touchScreenMap.Enable();
        ResetAfterShotImmediate();
    }

    public void DisableControls()
    {
        touchScreenMap.Disable();
        isGestureActive = false;
        ResetAfterShotImmediate();
    }

    void OnTouchStart(InputAction.CallbackContext context)
    {
        if (!touchScreenMap.enabled) return;
        if (isGestureActive) return;
        if (shotInFlight) return;
        
        currentPower = 0f;
        powerSlider.value = 0f;
        isGestureActive = true;
        shotInFlight = true;

        GenerateRandomPerfectZone();
        GenerateRandomBackboardZone();
        UpdateZonesVisual();

        if (gestureTimerCoroutine != null) StopCoroutine(gestureTimerCoroutine);
        gestureTimerCoroutine = StartCoroutine(GestureTimeout());

        playerAnimator.Shoot();
    }

    void OnTouchEnd(InputAction.CallbackContext context)
    {
        FinishGesture();
    }

    IEnumerator GestureTimeout()
    {
        yield return new WaitForSeconds(gestureTimeLimit);
        FinishGesture();
    }

    void FinishGesture()
    {
        if (!isGestureActive) return;
        isGestureActive = false;

        ShotType type = ResolveShotType(currentPower);
        Debug.Log($"Shot with power {currentPower:F2}, type: {type}");
        OnShoot?.Invoke(type);

        if (gestureTimerCoroutine != null) StopCoroutine(gestureTimerCoroutine);
        StartCoroutine(ResetAfterShot());
    }

    IEnumerator ResetAfterShot()
    {
        yield return new WaitForSeconds(0.5f);
        ResetAfterShotImmediate();
    }

    void ResetAfterShotImmediate()
    {
        powerSlider.value = 0f;
        currentPower = 0f;
        if (perfectZoneIndicator != null) perfectZoneIndicator.SetActive(false);
        if (backboardZoneIndicator != null) backboardZoneIndicator.SetActive(false);
    }

    void Update()
    {
        if (!isGestureActive || !touchScreenMap.enabled) return;

        Vector2 delta = touchDeltaAction.ReadValue<Vector2>();
        float verticalMovement = delta.y;

        if (verticalMovement > 0.1f)
        {
            currentPower = Mathf.Clamp01(currentPower + verticalMovement * sensitivity);
            powerSlider.value = currentPower;
        }
    }
    void GenerateRandomPerfectZone()
    {
        float size = Mathf.Clamp01(perfectShotRangeSize);
        //Make it on top but with space for backboard on top
        float bandMin = 0.5f;
        float bandMax = 0.8f;

        size = Mathf.Min(size, Mathf.Max(0f, bandMax - bandMin));
        if (size <= 0f)
        {
            perfectShotMin = perfectShotMax = perfectShotCenter = -1f;
            if (perfectZoneIndicator) perfectZoneIndicator.SetActive(false);
            return;
        }

        float centerMin = bandMin + size * 0.5f;
        float centerMax = bandMax - size * 0.5f;

        perfectShotCenter = Random.Range(centerMin, centerMax);
        perfectShotMin = perfectShotCenter - size * 0.5f;
        perfectShotMax = perfectShotCenter + size * 0.5f;

        perfectShotMin = Mathf.Clamp01(perfectShotMin);
        perfectShotMax = Mathf.Clamp01(perfectShotMax);
    }

    void GenerateRandomBackboardZone()
    {
        float size = Mathf.Clamp01(backboardShotRangeSize);
        const float eps = 0.0001f;

        if (perfectShotMin < 0f || perfectShotMax < 0f)
        {
            float bandMin = 0.8f;
            float bandMax = 1f;
            size = Mathf.Min(size, Mathf.Max(0f, bandMax - bandMin));
            if (size <= 0f)
            {
                backboardShotMin = backboardShotMax = backboardShotCenter = -1f;
                if (backboardZoneIndicator) backboardZoneIndicator.SetActive(false);
                return;
            }

            //Center
            float cMin = bandMin + size * 0.5f;
            float cMax = bandMax - size * 0.5f;
            backboardShotCenter = Random.Range(cMin, cMax);
            backboardShotMin = backboardShotCenter - size * 0.5f;
            backboardShotMax = backboardShotCenter + size * 0.5f;

            backboardShotMin = Mathf.Clamp01(backboardShotMin);
            backboardShotMax = Mathf.Clamp01(backboardShotMax);
            return;
        }

        //Make sure it does not overlap with perfect shot zone and its above it
        float allowedMin = Mathf.Clamp01(perfectShotMax + eps);
        float allowedMax = 1f;

        size = Mathf.Min(size, Mathf.Max(0f, allowedMax - allowedMin));
        if (size <= 0f)
        {
            backboardShotMin = backboardShotMax = backboardShotCenter = -1f;
            if (backboardZoneIndicator) backboardZoneIndicator.SetActive(false);
            return;
        }

        float centerMin = allowedMin + size * 0.5f;
        float centerMax = allowedMax - size * 0.5f;

        backboardShotCenter = Random.Range(centerMin, centerMax);
        backboardShotMin = backboardShotCenter - size * 0.5f;
        backboardShotMax = backboardShotCenter + size * 0.5f;

        backboardShotMin = Mathf.Clamp01(backboardShotMin);
        backboardShotMax = Mathf.Clamp01(backboardShotMax);
    }

    void UpdateZonesVisual()
    {
        bool hasPerfect = perfectShotMin >= 0f && perfectShotMax >= 0f;
        bool hasBack = backboardShotMin >= 0f && backboardShotMax >= 0f;

        if (perfectZoneRect != null)
        {
            if (hasPerfect) PlaceZone(perfectZoneRect, perfectShotMin, perfectShotMax - perfectShotMin, perfectZoneIndicator);
            else if (perfectZoneIndicator) perfectZoneIndicator.SetActive(false);
        }

        if (backboardZoneRect != null)
        {
            if (hasBack) PlaceZone(backboardZoneRect, backboardShotMin, backboardShotMax - backboardShotMin, backboardZoneIndicator);
            else if (backboardZoneIndicator) backboardZoneIndicator.SetActive(false);
        }
    }

    void PlaceZone(RectTransform rect, float startNorm, float sizeNorm, GameObject go)
    {
        float startX = startNorm * sliderWidth;
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(startX, 0);
        rect.sizeDelta = new Vector2(sliderWidth * sizeNorm, rect.sizeDelta.y);
        if (go != null) go.SetActive(true);
    }

    ShotType ResolveShotType(float power)
    {
        bool isPerfect = power >= perfectShotMin && power <= perfectShotMax;
        bool isBackboard = power >= backboardShotMin && power <= backboardShotMax;

        if (isPerfect && isBackboard)
        {
            float distPerfect = Mathf.Abs(power - perfectShotCenter);
            float distBack = Mathf.Abs(power - backboardShotCenter);
            return distPerfect <= distBack ? ShotType.Perfect : ShotType.Backboard;
        }
        if (isPerfect) return ShotType.Perfect;
        if (isBackboard) return ShotType.Backboard;
        return ShotType.Normal;
    }

    public void SetShotFlying(bool set)
    {
        shotInFlight = set;
    }
}
