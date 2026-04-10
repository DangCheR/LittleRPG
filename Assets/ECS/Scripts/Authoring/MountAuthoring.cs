using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    public class MountAuthoring : MonoBehaviour
    {
        class bake:Baker<MountAuthoring>
        {
            public override void Bake(MountAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new MountTag());
            }
        }
    }
}