using UnityEngine;

/// <summary>
/// Raycast-based ground snapping for interactables and spawned props.
/// </summary>
public static class GroundSnapUtility
{
    const float DefaultRayStartHeight = 200f;
    const float DefaultMaxDistance = 400f;

    public static bool TryGetGroundPoint(
        Vector3 nearPosition,
        out Vector3 groundPoint,
        float rayStartHeight = DefaultRayStartHeight,
        float maxDistance = DefaultMaxDistance,
        LayerMask? layerMask = null)
    {
        Vector3 origin = nearPosition + Vector3.up * rayStartHeight;
        int mask = layerMask ?? Physics.DefaultRaycastLayers;

        if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                rayStartHeight + maxDistance,
                mask,
                QueryTriggerInteraction.Ignore))
        {
            groundPoint = hit.point;
            return true;
        }

        groundPoint = nearPosition;
        return false;
    }

    public static bool SnapTransform(Transform target, float heightOffset = 0f)
    {
        if (target == null)
        {
            return false;
        }

        Vector3 position = target.position;
        if (!TryGetGroundPoint(position, out Vector3 groundPoint))
        {
            return false;
        }

        target.position = new Vector3(position.x, groundPoint.y + heightOffset, position.z);
        return true;
    }

    public static bool TryGetHeightAboveGround(Vector3 worldPosition, out float heightAboveGround)
    {
        if (TryGetGroundPoint(worldPosition, out Vector3 groundPoint))
        {
            heightAboveGround = worldPosition.y - groundPoint.y;
            return true;
        }

        heightAboveGround = 0f;
        return false;
    }

    public static bool SnapLocalPositionOnParent(Transform target, float heightOffset = 0f)
    {
        if (target == null)
        {
            return false;
        }

        if (!SnapTransform(target, heightOffset))
        {
            return false;
        }

        if (target.parent != null)
        {
            target.localPosition = target.parent.InverseTransformPoint(target.position);
        }

        return true;
    }
}
