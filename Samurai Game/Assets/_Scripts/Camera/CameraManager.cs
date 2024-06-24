using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Camera References:")]
    [SerializeField] private List<CinemachineVirtualCamera> virtualCameras;

    [Header("Y Damping Settings:")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;
    [SerializeField] private float fallSpeedYDampingChangeThreshold = -15f;

    public bool IsSLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    public float GetFallSpeedYDampingChangeThreshold => fallSpeedYDampingChangeThreshold;

    private Coroutine lerpYPanCoroutine;
    private Coroutine panCameraCoroutine;

    private CinemachineVirtualCamera currentVirtualCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;

    private Vector2 startingTrackedObjectOffset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        for (int i = 0; i < virtualCameras.Count; i++)
        {
            if (virtualCameras[i].enabled)
            {
                currentVirtualCamera = virtualCameras[i];
                framingTransposer = currentVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        normYPanAmount = framingTransposer.m_YDamping;

        startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
    }

    #region LerpYDamping
    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsSLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount;

        // INFO: Determine end damping amount
        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        // INFO: Lerp the pan amount
        float elapsedTime = 0.0f;
        while (elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            framingTransposer.m_YDamping = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / fallYPanTime);
            
            yield return null;
        }

        IsSLerpingYDamping = false;
    }
    #endregion LerpYDamping

    #region PanCamera
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        // INFO: Handle Pan to End Position
        if (!panToStartingPos)
        {
            // INFO: Set Direction and Distance
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.left;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.right;
                    break;
                case PanDirection.None:
                default:
                    break;
            }

            endPos *= panDistance;

            startingPos = startingTrackedObjectOffset;

            endPos += startingPos;
        }
        // INFO: Handle Pan to Starting Position
        else
        {
            startingPos = framingTransposer.m_TrackedObjectOffset;
            endPos = startingTrackedObjectOffset;
        }

        // INFO: Perform the Panning
        float elapsedTime = 0.0f;

        while (elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, elapsedTime / panTime);
            framingTransposer.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }
    #endregion PanCamera
}
