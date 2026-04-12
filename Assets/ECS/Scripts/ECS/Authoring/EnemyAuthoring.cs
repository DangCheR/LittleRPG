using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public int health = 10;
        class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new HealthData
                {
                    Max = authoring.health,
                    Current = authoring.health,
                });
                
                AddBuffer<DamageBufferElement>(entity);
                
                AddComponent(entity, new EnemyTag());
            }
        }
    }
}