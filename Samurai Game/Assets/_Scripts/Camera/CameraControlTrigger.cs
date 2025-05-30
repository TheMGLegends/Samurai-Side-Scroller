using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PanDirection
{
    None = 0,

    Up,
    Down,
    Left,
    Right
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraControlTrigger))]
public class CameraControlTriggerEditor : Editor
{
    private CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("swapCameras"), new GUIContent("Swap Cameras"));

        if (cameraControlTrigger.swapCameras)
        {
            EditorGUI.indentLevel++;

            cameraControlTrigger.swapVerticalCameras = EditorGUILayout.Toggle("Swap Vertical Cameras", cameraControlTrigger.swapVerticalCameras);

            EditorGUI.indentLevel++;

            if (cameraControlTrigger.swapVerticalCameras)
            {
                cameraControlTrigger.cameraOnTop = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera On Top", cameraControlTrigger.cameraOnTop, typeof(CinemachineVirtualCamera), true);
                cameraControlTrigger.cameraOnBottom = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera On Bottom", cameraControlTrigger.cameraOnBottom, typeof(CinemachineVirtualCamera), true);
            }
            else
            {
                cameraControlTrigger.cameraOnLeft = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera On Left", cameraControlTrigger.cameraOnLeft, typeof(CinemachineVirtualCamera), true);
                cameraControlTrigger.cameraOnRight = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera On Right", cameraControlTrigger.cameraOnRight, typeof(CinemachineVirtualCamera), true);
            }

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel -= 2;
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("panCameraOnContact"), new GUIContent("Pan Camera On Contact"));

        if (cameraControlTrigger.panCameraOnContact)
        {
            EditorGUI.indentLevel++;

            cameraControlTrigger.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Pan Direction", cameraControlTrigger.panDirection);
            cameraControlTrigger.panDistance = EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.panDistance);
            cameraControlTrigger.panTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.panTime);

            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
            EditorUtility.SetDirty(cameraControlTrigger);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

public class CameraControlTrigger : MonoBehaviour
{
    [Tooltip("If set to true, variables for swapping cameras will appear")]
    public bool swapCameras;
    [HideInInspector] public bool swapVerticalCameras;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

    [HideInInspector] public CinemachineVirtualCamera cameraOnTop;
    [HideInInspector] public CinemachineVirtualCamera cameraOnBottom;

    [Tooltip("If set to true, variables for panning the camera will appear")]
    public bool panCameraOnContact;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance;
    [HideInInspector] public float panTime;


    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerCharacter _))
        {
            if (panCameraOnContact)
            {
                // INFO: Pan Camera
                CameraManager.Instance.PanCameraOnContact(panDistance, panTime, panDirection, false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerCharacter _))
        {
            if (panCameraOnContact)
            {
                // INFO: Pan Camera
                CameraManager.Instance.PanCameraOnContact(panDistance, panTime, panDirection, true);
            }

            if (swapCameras)
            {
                Vector2 exitDirection = (collision.transform.position - boxCollider2D.bounds.center).normalized;

                if (swapVerticalCameras && cameraOnBottom != null && cameraOnTop != null)
                {
                    // INFO: Swap Vertical Cameras
                    CameraManager.Instance.SwapVerticalCameras(cameraOnBottom, cameraOnTop, exitDirection);
                }
                else if (!swapVerticalCameras && cameraOnLeft != null && cameraOnRight != null)
                {
                    // INFO: Swap Horizontal Cameras
                    CameraManager.Instance.SwapHorizontalCameras(cameraOnLeft, cameraOnRight, exitDirection);
                }
            }
        }
    }
}
