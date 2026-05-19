using UnityEngine;

[RequireComponent(typeof(DamageMapPainter))]
public class ImpactReceiver : MonoBehaviour
{
    [Tooltip("Offset along contact normal to start the re-raycast from, avoids self-intersection")]
    [SerializeField] private float raycastOffset = 0.1f;

    [Tooltip("Layer mask for the re-raycast — should only hit this object's layer")]
    [SerializeField] private LayerMask impactLayerMask = Physics.DefaultRaycastLayers;

    private DamageMapPainter _painter;
    private int _selfLayerMask;

    void Awake()
    {
        _painter = GetComponent<DamageMapPainter>();
        _selfLayerMask = 1 << gameObject.layer; // Raycast takes a layer mask, if layer is 6 , mask is (01000000)_2 = (64)_10
        Debug.Log($"[ImpactReceiver] Layer mask = {_selfLayerMask}, layer = {gameObject.layer}");
    }

    void OnCollisionEnter(Collision collision)
    {
        // contact point is the point of contact on the object colliding with us
        // therefore the contact normal points inwards this collider
        ContactPoint contact = collision.GetContact(0);
        // Slightly above surface and shoot towards surface
        Vector3 rayOrigin = contact.point - contact.normal * raycastOffset;
        Vector3 rayDirection = contact.normal;

        Debug.DrawRay(rayOrigin, rayDirection, Color.red, 50.0f);
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit,
                            raycastOffset * 3f, _selfLayerMask))
        {
            Debug.Log($"Collider hit: {hit.collider.gameObject.name}");
            Debug.Log($"triangleIndex: {hit.triangleIndex}");
            Debug.Log($"UV: {hit.textureCoord}");
            _painter.RegisterImpact(hit);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"[ImpactReceiver] Re-raycast missed on {gameObject.name}. " +
                             "Check: MeshCollider present, non-convex, Read/Write enabled on mesh.");
        }
#endif
    }
}