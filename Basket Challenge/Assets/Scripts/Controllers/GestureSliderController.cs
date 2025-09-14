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
   
    [Header("Settings")]
    [SerializeField] private float gestureTimeLimit = 3f;
    [SerializeField] private float sensitivity = 0.001f;
    
    [Header("Perfect Shot Settings")]
    [SerializeField] private float perfectShotRangeSize = 0.15f;
    
    [Header("Visual Perfect Zone")]
    [SerializeField] private GameObject perfectZoneIndicator;
   
    private InputAction touchPressAction;
    private InputAction touchDeltaAction;
    private InputActionMap touchScreenMap;
   
    private bool isGestureActive = false;
    private float currentPower = 0f;
    private Coroutine gestureTimerCoroutine;
    
    private float perfectShotCenter;
    private float perfectShotMin;
    private float perfectShotMax;
    private RectTransform perfectZoneRect;
   
    private float sliderWidth;

    public System.Action<float, bool> OnShoot;
   
    void Start()
    {
        SetupInput();
        
        if (perfectZoneIndicator != null)
        {
            perfectZoneRect = perfectZoneIndicator.GetComponent<RectTransform>();
            perfectZoneIndicator.SetActive(false);
        }

        sliderWidth = GetComponent<RectTransform>().rect.width;
    }
    
    void UpdatePerfectZoneVisual()
    {
        if (perfectZoneRect == null || sliderBackground == null) return;
        
        float startX = perfectShotMin * sliderWidth;
        
        perfectZoneRect.anchorMin = new Vector2(0, 0.5f);
        perfectZoneRect.anchorMax = new Vector2(0, 0.5f);
        perfectZoneRect.pivot = new Vector2(0, 0.5f);
        
        perfectZoneRect.anchoredPosition = new Vector2(startX, 0);
        perfectZoneRect.sizeDelta = new Vector2(sliderWidth * perfectShotRangeSize, perfectZoneRect.sizeDelta.y);
        
        perfectZoneIndicator.SetActive(true);
    }
   
    void SetupInput()
    {
        touchScreenMap = playerControls.FindActionMap("TouchScreen");
        touchPressAction = touchScreenMap.FindAction("TouchPress");
        touchDeltaAction = touchScreenMap.FindAction("TouchDelta");
       
        touchPressAction.started += OnTouchStart;
        touchPressAction.canceled += OnTouchEnd;
       
        touchScreenMap.Enable();
    }
   
    void OnEnable()
    {
        playerControls?.Enable();
    }
   
    void OnDisable()
    {
        playerControls?.Disable();
        if (gestureTimerCoroutine != null)
            StopCoroutine(gestureTimerCoroutine);
    }
   
    private void OnTouchStart(InputAction.CallbackContext context)
    {
        currentPower = 0f;
        powerSlider.value = 0f;
        isGestureActive = true;
        
        GenerateRandomPerfectZone();
        UpdatePerfectZoneVisual();
       
        if (gestureTimerCoroutine != null)
            StopCoroutine(gestureTimerCoroutine);
       
        gestureTimerCoroutine = StartCoroutine(GestureTimer());
    }
    
    private void GenerateRandomPerfectZone()
    {
        float margin = perfectShotRangeSize / 2f;
        perfectShotCenter = Random.Range(margin, 1f - margin);
        
        perfectShotMin = perfectShotCenter - (perfectShotRangeSize / 2f);
        perfectShotMax = perfectShotCenter + (perfectShotRangeSize / 2f);
        
        perfectShotMin = Mathf.Clamp01(perfectShotMin);
        perfectShotMax = Mathf.Clamp01(perfectShotMax);
    }
    
    private bool IsPerfectShot(float power)
    {
        return power >= perfectShotMin && power <= perfectShotMax;
    }
   
    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        FinishGesture();
    }
   
    private void FinishGesture()
    {
        if (!isGestureActive) return;
       
        isGestureActive = false;
       
        if (gestureTimerCoroutine != null)
        {
            StopCoroutine(gestureTimerCoroutine);
            gestureTimerCoroutine = null;
        }
        
        bool isPerfect = IsPerfectShot(currentPower);
        
        OnShoot?.Invoke(currentPower, isPerfect);
        
        StartCoroutine(ResetAfterShot());
    }
   
    IEnumerator ResetAfterShot()
    {
        yield return new WaitForSeconds(0.5f);
        powerSlider.value = 0f;
        currentPower = 0f;
            
        if (perfectZoneIndicator != null)
            perfectZoneIndicator.SetActive(false);
    }
   
    private IEnumerator GestureTimer()
    {
        yield return new WaitForSeconds(gestureTimeLimit);
       
        if (isGestureActive)
        {
            FinishGesture();
        }
    }
   
    void Update()
    {
        if (isGestureActive)
        {
            Vector2 delta = touchDeltaAction.ReadValue<Vector2>();
            float verticalMovement = delta.y;
           
            if (verticalMovement > 0.1f)
            {
                currentPower = Mathf.Clamp01(currentPower + verticalMovement * sensitivity);
                powerSlider.value = currentPower;
            }
        }
    }
    
    public void GetPerfectZoneInfo(out float min, out float max, out float center)
    {
        min = perfectShotMin;
        max = perfectShotMax;
        center = perfectShotCenter;
    }
}