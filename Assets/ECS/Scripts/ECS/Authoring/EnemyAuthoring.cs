using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public int health = 10;

        public GameObject HealthBarQuadPrefab; // 血条预制体
        
        class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new HealthData
                {
                    Max = authoring.health,
                    lastHP = authoring.health,
                    Current = authoring.health,
                });

                AddBuffer<DamageBufferElement>(entity);

                AddComponent(entity, new EnemyTag());

                var hpBarEntity = GetEntity(authoring.HealthBarQuadPrefab, TransformUsageFlags.Dynamic);

                AddComponent(entity, new HealthBarReference { BarEntity = hpBarEntity });
            }
        }
    }
}