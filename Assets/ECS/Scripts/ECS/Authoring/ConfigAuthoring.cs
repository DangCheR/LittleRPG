using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    public class ConfigAuthoring : MonoBehaviour
    {
        public GameObject BotInEcs; // 在ecs里的，只是一个方块
        public GameObject BotModel; // 在主世界的model

        class Baker : Baker<ConfigAuthoring>
        {
            public override void Bake(ConfigAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                AddComponent(entity, new ConfigComponent
                {
                    BotPrefab = GetEntity(authoring.BotInEcs, TransformUsageFlags.Dynamic)
                });
            }
        }

    }
    public struct ConfigComponent : IComponentData
    {
        public Entity BotPrefab;
    }
}