using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(MeshCollider))]
public class DamageMapPainter : MonoBehaviour
{
    [Header("Damage Map")]
    [SerializeField] private int damageMapResolution = 1024;
    [SerializeField] private float impactRadius = 0.05f; // in UV space 0-1

    [Header("Compute")]
    [SerializeField] private ComputeShader damageCompute;       // the .compute asset

    [Header("Material")]
    [SerializeField] private Material targetMaterial;           // mesh material

    private RenderTexture _damageMap;
    private int _kernel;
    private static readonly int DamageMapID = Shader.PropertyToID("_DamageMap");
    private static readonly int ImpactUVID = Shader.PropertyToID("_ImpactUV");
    private static readonly int ImpactRadiusID = Shader.PropertyToID("_ImpactRadius");
    private static readonly int ResolutionID = Shader.PropertyToID("_Resolution");

    void Awake()
    {
        if (damageCompute == null)
        {
            Debug.LogError("[DamageMapPainter] damageCompute is not assigned in the Inspector");
            return;
        }

        // enableRandomWrite is what allows a compute shader to write into this texture
        _damageMap = new RenderTexture(damageMapResolution, damageMapResolution, 0,
                                       RenderTextureFormat.RFloat)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = "DamageMap"
        };
        _damageMap.Create();

        _kernel = damageCompute.FindKernel("PaintSplat");
        

        // Set values that never change after init
        damageCompute.SetTexture(_kernel, DamageMapID, _damageMap);
        damageCompute.SetInt(ResolutionID, damageMapResolution);

        // Bind to mesh material — same RT reference throughout, no rebind needed
        targetMaterial.SetTexture(DamageMapID, _damageMap);
    }

    public void RegisterImpact(RaycastHit hit)
    {
        Vector2 uv = hit.textureCoord;

        // Pass impact position and radius
        damageCompute.SetVector(ImpactUVID, new Vector4(uv.x, uv.y, 0, 0));
        damageCompute.SetFloat(ImpactRadiusID, impactRadius);

        // Each thread group is 8x8 threads — dispatch enough groups to cover the texture
        int groups = Mathf.CeilToInt(damageMapResolution / 8f);
        damageCompute.Dispatch(_kernel, groups, groups, 1);

        // No ping-pong, no rebind — the compute shader wrote directly into _damageMap
    }

    void OnDestroy()
    {
        _damageMap.Release();
    }
}