using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMenuController : MonoBehaviour
{
    [System.Serializable]
    public class CameraSetup
    {
        [SerializeField] public CameraState state;
        [SerializeField] public Transform position;
        [SerializeField] public GameObject uiPanel;
    }

    [Header("Camera Setup")]
    [SerializeField] private List<CameraSetup> cameraSetups = new List<CameraSetup>();

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game Components")]
    [SerializeField] private GameTimer gameTimer;

    private bool isMoving = false;
    private CameraState currentState = CameraState.Menu;
    private Transform mainCamera;

    public enum CameraState
    {
        Menu,
        Gameplay,
        Reward,
        PlayAgain
    }

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    private void Start()
    {
        SetCameraState(CameraState.Menu);
    }

    public void MoveToMenu()
    {
        MoveTo(CameraState.Menu);
    }

    public void MoveToGameplay()
    {
        MoveTo(CameraState.Gameplay);
    }

    public void MoveToReward()
    {
        MoveTo(CameraState.Reward);
    }

    public void MoveToPlayAgain()
    {
        MoveTo(CameraState.PlayAgain);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void MoveTo(CameraState targetState)
    {
        if (!isMoving && currentState != targetState)
        {
            CameraSetup targetSetup = GetCameraSetup(targetState);
            if (targetSetup != null)
            {
                StartCoroutine(MoveCameraToPosition(targetSetup));
            }
        }
    }

    private IEnumerator MoveCameraToPosition(CameraSetup targetSetup)
    {
        isMoving = true;

        DeactivateCurrentUI();

        Vector3 startPos = mainCamera.position;
        Quaternion startRot = mainCamera.rotation;
        Vector3 targetPos = targetSetup.position.position;
        Quaternion targetRot = targetSetup.position.rotation;

        float elapsed = 0f;
        float duration = 1f / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = moveCurve.Evaluate(t);

            mainCamera.position = Vector3.Lerp(startPos, targetPos, curveValue);
            mainCamera.rotation = Quaternion.Lerp(startRot, targetRot, curveValue);

            yield return null;
        }

        mainCamera.position = targetPos;
        mainCamera.rotation = targetRot;

        currentState = targetSetup.state;
        ActivateTargetUI();

        if (currentState == CameraState.Gameplay && gameTimer != null)
        {
            gameTimer.StartGame();
        }

        isMoving = false;
    }


    private void DeactivateCurrentUI()
    {
        CameraSetup currentSetup = GetCameraSetup(currentState);
        if (currentSetup?.uiPanel != null)
        {
            currentSetup.uiPanel.SetActive(false);
        }
    }

    private void ActivateTargetUI()
    {
        CameraSetup currentSetup = GetCameraSetup(currentState);
        if (currentSetup?.uiPanel != null)
        {
            currentSetup.uiPanel.SetActive(true);
        }
    }

    private void SetCameraState(CameraState state)
    {
        currentState = state;

        CameraSetup setup = GetCameraSetup(state);
        if (setup?.position != null && mainCamera != null)
        {
            mainCamera.transform.position = setup.position.position;
            mainCamera.transform.rotation = setup.position.rotation;
        }

        DeactivateAllUI();
        ActivateTargetUI();
    }

    private void DeactivateAllUI()
    {
        foreach (var setup in cameraSetups)
        {
            if (setup.uiPanel != null)
            {
                setup.uiPanel.SetActive(false);
            }
        }
    }

    private CameraSetup GetCameraSetup(CameraState state)
    {
        foreach (var setup in cameraSetups)
        {
            if (setup.state == state)
            {
                return setup;
            }
        }
        return null;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public CameraState GetCurrentState()
    {
        return currentState;
    }
}