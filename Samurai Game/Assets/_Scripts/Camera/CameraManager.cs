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
    public float fallSpeedYDampingChangeThreshold = -15f;

    public bool IsSLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;

    private CinemachineVirtualCamera currentVirtualCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;

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
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsSLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount = 0.0f;

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
}
