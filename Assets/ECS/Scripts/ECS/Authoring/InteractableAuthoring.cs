
using Unity.Entities;
using UnityEngine;

namespace LittleRPG.Combat
{
    // --- Baker 烘焙 ---
    public class InteractableAuthoring : MonoBehaviour
    {
        class Baker : Baker<InteractableAuthoring>
        {
            public override void Bake(InteractableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // 打上 ECS 标签
                AddComponent<InteractableTag>(entity);
            }
        }
    }
}