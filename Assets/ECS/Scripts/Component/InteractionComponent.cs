using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace LittleRPG.Combat
{
    // 1. 玩家的探测器配置 (纯数据)
    public struct Interactor : IComponentData
    {
        public float Range;

        // 当前可交互目标，但是还没交互，缓存下来不用每帧都查了
        public Entity CurrentTarget; 

        // 当前正在交互的目标，有值时就不去寻找了，直到交互结束才重置
        public Entity CurrentInteractiveTarget;
    }

    // 2. 交互目标的标签 (纯数据，供 Burst 极速查询)
    public struct InteractableTag : IComponentData, IEnableableComponent
    {
        // public bool Enabled; // 这个标签默认是禁用的，只有真正可交互的物体才启用它
        public bool IsInteracting; // 是否正在被交互
        public bool IsInRange; // 是否在交互范围内
    }
}