using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    /// <summary>
    /// 这个挂在武器的 GameObject 上，标记这个物体是一个武器
    /// </summary>
    public class WeaponAuthoring : MonoBehaviour
    {
        public int AttackDamage = 10;
        public float AttackRange = 1.5f;

        class Baker : Baker<WeaponAuthoring>
        {
            public override void Bake(WeaponAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new WeaponData
                {
                    AttackDamage = authoring.AttackDamage,
                    AttackRange = authoring.AttackRange,
                });
            }
        }
    }
}