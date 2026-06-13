using UnityEngine;

/// <summary>
/// Compatibility placeholder kept so older scenes or cached script lists do not
/// reference a missing type. External asset-pack scene dressing has been removed.
/// </summary>
public class SceneBeautyDirector : MonoBehaviour
{
    [ContextMenu("Apply Scene Beauty")]
    public void Apply()
    {
        Debug.Log("SceneBeautyDirector is disabled; external scene beauty assets were removed.");
    }
}
