using Unity.Entities;
using UnityEngine;

namespace LittleRPG.Combat
{
    // 3. 烘焙器：把 GameObject 变成 Entity 存起来
    public class WeaponRegistryAuthoring : MonoBehaviour
    {
        public GameObject BoxWeaponGO;
        public GameObject SwordWeaponGO;

        class Baker : Baker<WeaponRegistryAuthoring>
        {
            public override void Bake(WeaponRegistryAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None); // 单例不需要 Transform

                AddComponent(entity, new WeaponRegistryData
                {
                    // 【核心魔法】：把 GameObject 预制体，烘焙成 ECS 的 Entity 预制体！
                    BoxPrefab = GetEntity(authoring.BoxWeaponGO, TransformUsageFlags.Dynamic),
                    SwordPrefab = GetEntity(authoring.SwordWeaponGO, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}