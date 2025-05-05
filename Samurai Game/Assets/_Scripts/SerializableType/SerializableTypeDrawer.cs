using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableType))]
public class SerializableTypeDrawer : PropertyDrawer
{
    TypeFilterAttribute typeFilterAttribute;
    string[] typeNames, typeFullNames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Initialise();
        SerializedProperty typeProperty = property.FindPropertyRelative("assemblyQualifiedName");

        if (string.IsNullOrEmpty(typeProperty.stringValue) && typeFullNames.Length > 0)
        {
            typeProperty.stringValue = typeFullNames.FirstOrDefault() ?? "";
            property.serializedObject.ApplyModifiedProperties();
        }

        Int32 currentIndex = Array.IndexOf(typeFullNames, typeProperty.stringValue);

        if (IsInArray(property)) { label = GUIContent.none; }
        Int32 selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames);

        if (selectedIndex >= 0 && selectedIndex != currentIndex)
        {
            typeProperty.stringValue = typeFullNames[selectedIndex];
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void Initialise()
    {
        if (typeFullNames != null) { return; }

        typeFilterAttribute = (TypeFilterAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(TypeFilterAttribute));

        Type[] filteredTypes = AppDomain.CurrentDomain.GetAssemblies()
                              .SelectMany(assembly => assembly.GetTypes())
                              .Where(type => typeFilterAttribute == null ? DefaultFilter(type) : typeFilterAttribute.Filter(type))
                              .ToArray();

        typeNames = filteredTypes.Select(type => type.ReflectedType == null ? type.Name : $"{type.ReflectedType.Name}.{type.Name}").ToArray();
        typeFullNames = filteredTypes.Select(type => type.AssemblyQualifiedName).ToArray();
    }

    private static bool DefaultFilter(Type type)
    {
        return !type.IsAbstract && !type.IsInterface && !type.IsGenericType;
    }

    private static bool IsInArray(SerializedProperty property)
    {
        return property.propertyPath.Contains(".Array.data[");
    }
}
