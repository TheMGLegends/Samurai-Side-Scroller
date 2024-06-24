using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum PanDirection
{
    None = 0,

    Up,
    Down,
    Left,
    Right
}

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

            cameraControlTrigger.cameraOnLeft = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera On Left", cameraControlTrigger.cameraOnLeft, typeof(CinemachineVirtualCamera), true);
            cameraControlTrigger.cameraOnRight = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera On Right", cameraControlTrigger.cameraOnRight, typeof(CinemachineVirtualCamera), true);

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel--;
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

public class CameraControlTrigger : MonoBehaviour
{
    [Tooltip("If set to true, variables for swapping cameras will appear")]
    public bool swapCameras;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

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
        }
    }
}
