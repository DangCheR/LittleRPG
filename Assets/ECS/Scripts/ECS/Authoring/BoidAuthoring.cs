using LittleRPG.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    /// <summary>
    /// 用于Boid集合
    /// </summary>
    public class BoidAuthoring : MonoBehaviour
    {
        class Baker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new BoidBrain
                {
                    SeparationWeight = 0.3f,
                    AlignmentWeight = 0.3f,
                    CohesionWeight = 0.3f,
                });
            }
        }
    }
}