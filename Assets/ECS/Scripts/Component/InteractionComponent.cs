using Unity.Entities;
using UnityEngine;

namespace LittleRPG.Combat
{
    // 1. 玩家的探测器配置 (纯数据)
    public struct Interactor : IComponentData
    {
        public float Range;
    }

    // 2. 交互目标的标签 (纯数据，供 Burst 极速查询)
    public struct InteractableTag : IComponentData, IEnableableComponent
    {
        // public bool Enabled; // 这个标签默认是禁用的，只有真正可交互的物体才启用它
        public bool IsInteracting; // 是否正在被交互
        public bool IsInRange; // 是否在交互范围内
    }

    // 3. 【核心黑魔法】：托管组件 (Managed Component)
    // 它存了一个指向原本 GameObject 交互脚本的引用！

    // public class InteractableProxy : IComponentData
    // {
    //     // 这是我们之前写的那个“万能交互组件”！
    //     public InteractiveComponent OOPComponent;
    // }

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

                // 挂上托管引用（把自己的 GameObject 脚本传给 ECS）
                // AddComponent(entity, new InteractableProxy
                // {
                //     OOPComponent = authoring.GetComponent<InteractiveComponent>()
                // });
            }
        }
    }
}