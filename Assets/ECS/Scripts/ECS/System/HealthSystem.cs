using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace LittleRPG.Combat
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AttackSystem))]
    [BurstCompile]
    public partial struct HealthSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HealthData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // 寻找有血量的，能收到伤害的，没有死亡的
            foreach (var (health, damageBuffer, entity) in
                SystemAPI.Query<RefRW<HealthData>, DynamicBuffer<DamageBufferElement>>()
                .WithNone<DeadTag>()
                .WithEntityAccess())
            {
                if (damageBuffer.IsEmpty)
                {
                    continue;
                }
                float totalDamage = .0f;

                foreach (var i in damageBuffer)
                {
                    totalDamage += i.Value;
                }

                health.ValueRW.Current -= totalDamage;

                damageBuffer.Clear();

                if (health.ValueRO.Current <= 0)
                {
                    health.ValueRW.Current = 0f; // 锁死血量防负数

                    // 贴上死亡标签！
                    // 不要在这里 DestroyEntity，因为其他系统（比如播放死亡动画、掉落金币）还需要用到它的尸体
                    ecb.AddComponent<DeadTag>(entity);

                    UnityEngine.Debug.Log($"实体 {entity.Index} 死亡！");
                }
            }

            // 寻找有血量的，没有死亡的
            foreach (var (health, healBuffer, entity) in
                SystemAPI.Query<RefRW<HealthData>, DynamicBuffer<HealBufferElement>>()
                .WithNone<DeadTag>()
                .WithEntityAccess())
            {

                if (healBuffer.IsEmpty)
                {
                    continue;
                }
                float totalHealth = .0f;

                foreach (var i in healBuffer)
                {
                    totalHealth += i.Value;
                }

                health.ValueRW.Current += totalHealth;

                healBuffer.Clear();

                if (health.ValueRO.Current > health.ValueRO.Max)
                {
                    health.ValueRW.Current = health.ValueRO.Max;

                    // 贴上死亡标签！
                    // 不要在这里 DestroyEntity，因为其他系统（比如播放死亡动画、掉落金币）还需要用到它的尸体
                    // ecb.AddComponent<DeadTag>(entity);

                    UnityEngine.Debug.Log($"实体 {entity.Index} 回了口血！");
                }
            }
        }
    }
}