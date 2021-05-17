using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;

[GenerateAuthoringComponent]
[MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
public struct MaterialColor : IComponentData
{
    public float4 Value;
}