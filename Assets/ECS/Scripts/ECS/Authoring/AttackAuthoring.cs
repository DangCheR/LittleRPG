using LittleRPG.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    /// <summary>
    /// 有必要存在吗
    /// </summary>
    public class AttackAuthoring : MonoBehaviour
    {
        class Baker : Baker<AttackAuthoring>
        {
            public override void Bake(AttackAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                // AddComponent(entity, ShapeData{
                //     Position
                // })
            }
        }
    }
}