using UnityEngine;

/// <summary>
/// Hover + slow spin for the NPC priest. Attach to the visual root (not the interaction root).
/// </summary>
public class PriestFloat : MonoBehaviour
{
    [SerializeField] float floatAmplitude = 0.3f;
    [SerializeField] float floatSpeed = 1.2f;
    [SerializeField] float rotateSpeed = 12f;

    Vector3 _basePos;

    void Start()
    {
        _basePos = transform.localPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = _basePos + Vector3.up * offsetY;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
    }
}
