using System;
using System.Linq;

public static class TypeExtensions
{
    /// <summary>
    /// Checks if the type inherits from or implements the specified base type
    /// </summary>
    /// <param name="type">The type to be checked</param>
    /// <param name="baseType">The base type/inteface which is expected to be inherited/implement by the 'type'</param>
    /// <returns>Return true if 'type' inherits/implements 'baseType, otherwise false</returns>
    public static bool InheritsOrImplements(this Type type, Type baseType)
    {
        type = ResolveGenericType(type);
        baseType = ResolveGenericType(baseType);

        while (type != typeof(object))
        {
            if (baseType == type || HasAnyInterfaces(type, baseType)) { return true; }

            type = ResolveGenericType(type.BaseType);
            if (type == null) { break; }
        }

        return false;
    }

    private static Type ResolveGenericType(Type type)
    {
        if (type is not { IsGenericType: true }) { return type; }

        Type genericType = type.GetGenericTypeDefinition();
        return genericType != type ? genericType : type;
    }

    private static bool HasAnyInterfaces(Type type, Type interfaceType)
    {
        return type.GetInterfaces().Any(t => ResolveGenericType(t) == interfaceType);
    }
}
