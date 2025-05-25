using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // INFO: Disables the property field from being edited in the inspector
        GUI.enabled = false;

        EditorGUI.PropertyField(position, property, label);

        // INFO: Re-enables the property field for the next property
        GUI.enabled = true;
    }
}
#endif
