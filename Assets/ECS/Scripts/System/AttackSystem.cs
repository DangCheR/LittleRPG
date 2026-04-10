using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace LittleRPG.Combat
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct AttackSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. 先把所有敌人收集起来（假设敌人都有 EnemyTag 和 Health 组件）
            var enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag, LocalTransform, HealthData>().Build();
            var enemyTransforms = enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var enemyEntities = enemyQuery.ToEntityArray(Allocator.Temp);

            // 2. 遍历所有玩家
            foreach (var (combatState, transform, entity) in
                     SystemAPI.Query<RefRW<PlayerCombatState>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                // 没扣动扳机？跳过！
                if (!combatState.ValueRO.TriggerAttackHit) continue;
                Debug.Log("杀人了！！");
                // 【核心重置】：立刻把扳机松开！防止下一帧重复扣血！
                combatState.ValueRW.TriggerAttackHit = false;

                // --- 扇形索敌逻辑开始 ---
                float3 playerPos = transform.ValueRO.Position;
                float3 playerForward = transform.ValueRO.Forward();
                float attackRangeSq = combatState.ValueRO.AttackRange * combatState.ValueRO.AttackRange;

                for (int i = 0; i < enemyTransforms.Length; i++)
                {
                    float3 enemyPos = enemyTransforms[i].Position;
                    float3 dirToEnemy = enemyPos - playerPos;

                    // 1. 距离检查：在不在攻击范围内？
                    if (math.lengthsq(dirToEnemy) > attackRangeSq) continue;

                    // 2. 角度检查 (Dot Product 点乘)
                    // normalize: 变成长度为 1 的方向向量
                    float3 dirNorm = math.normalize(dirToEnemy);
                    float dotProduct = math.dot(playerForward, dirNorm);

                    // 点乘结果: 1=正前方, 0=正侧方, -1=正后方
                    // 0.5f 大概是前方 120 度夹角 (cos(60度) = 0.5)
                    if (dotProduct < 0.5f) continue;

                    // 打中了！！！
                    Entity hitEnemy = enemyEntities[i];
                    Debug.Log($"砍中了敌人 {hitEnemy.Index}！");
                    if (SystemAPI.HasBuffer<DamageBufferElement>(hitEnemy))
                    {
                        var damageBuffer = SystemAPI.GetBuffer<DamageBufferElement>(hitEnemy);

                        // 投递 25 点伤害账单！
                        float dam = combatState.ValueRW.AttackDamage;
                        damageBuffer.Add(new DamageBufferElement
                        {
                            Value = dam,
                            Attacker = entity
                        });
                    }
                }
            }
        }
    }
}