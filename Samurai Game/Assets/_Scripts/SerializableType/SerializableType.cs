using System;
using UnityEngine;

[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    [SerializeField] string assemblyQualifiedName = string.Empty;

    public Type Type { get; private set; }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        assemblyQualifiedName = Type?.AssemblyQualifiedName ?? assemblyQualifiedName;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) { return; }

        if (!TryGetType(assemblyQualifiedName, out Type type))
        {
            Debug.LogError($"Type {assemblyQualifiedName} not found!");
            return;
        }

        Type = type;
    }

    public override bool Equals(object obj)
    {
        if (obj is Type type)
        {
            return Type == type;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Type?.GetHashCode() ?? 0;
    }

    static bool TryGetType(string typeString, out Type type)
    {
        type = Type.GetType(typeString);
        return type != null || !string.IsNullOrEmpty(typeString);
    }

    public static implicit operator Type(SerializableType serializableType)
    {
        return serializableType.Type;
    }

    public static implicit operator SerializableType(Type type)
    {
        return new SerializableType { Type = type };
    }
}
