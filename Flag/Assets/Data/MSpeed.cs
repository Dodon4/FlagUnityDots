using Unity.Entities;

[GenerateAuthoringComponent]
public struct MSpeed : IComponentData
{
    public float Value;
    public bool isMoving;

}