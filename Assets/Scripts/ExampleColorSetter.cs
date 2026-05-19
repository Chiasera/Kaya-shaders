using UnityEngine;

public class ExampleColorSetter : MonoBehaviour
{

    [Header("Material properties")]
    [SerializeField] private Color materialColor;
    [SerializeField] private Material materialRef;

    static readonly int ColorProp = Shader.PropertyToID("_BaseColor");


    private void Awake()
    {
        materialRef.SetColor(ColorProp, materialColor);

    }
}
