using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 这个类有存在的意义吗
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HealthSystem))]
    public partial struct DeathSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 使用 EntityCommandBuffer (ECB) 来安全地添加/移除组件
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // 1. 检查所有刚死亡的实体（DeathTag）
            // 使用 WithNone<DeceasedTag> 确保只在刚进入死亡状态时执行一次打印
            foreach (var (tag, entity) in SystemAPI.Query<EnabledRefRW<DeadTag>>().WithNone<DeceasedTag>().WithEntityAccess())
            {
                
                UnityEngine.Debug.Log($"[ECS] 实体 {entity.Index} 死亡！");
                // ecb.SetComponentEnabled<DeadTag>(entity, false);
                // SystemAPI.SetComponentEnabled<DeadTag>(entity, false);
            }

            // DeceasedTag 彻底消失，交给动画系统处理
        }
    }
}