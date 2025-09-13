using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class GestureSliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionAsset playerControls;
    [SerializeField] private Slider powerSlider;
   
    [Header("Settings")]
    [SerializeField] private float gestureTimeLimit = 3f;
    [SerializeField] private float sensitivity = 0.001f;
   
    private InputAction touchPressAction;
    private InputAction touchDeltaAction;
    private InputActionMap touchScreenMap;
    
    private bool isGestureActive = false;
    private float currentPower = 0f;
    private Coroutine gestureTimerCoroutine;
    
    public System.Action<float> OnShoot;
    
    void Start()
    {
        SetupInput();
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
       
        if (gestureTimerCoroutine != null)
            StopCoroutine(gestureTimerCoroutine);
       
        gestureTimerCoroutine = StartCoroutine(GestureTimer());
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
       
        OnShoot?.Invoke(currentPower);
        StartCoroutine(ResetAfterShot());
    }
   
    IEnumerator ResetAfterShot()
    {
        yield return new WaitForSeconds(0.5f);
        powerSlider.value = 0f;
        currentPower = 0f;
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
}
