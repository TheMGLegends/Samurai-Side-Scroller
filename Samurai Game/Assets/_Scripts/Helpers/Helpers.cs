using UnityEngine;

public class Helpers
{
    public static bool IsInLayerMask(LayerMask layerMask, int layer)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
