using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using LittleRPG.Physics;

namespace LittleRPG.Combat
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, moveComponent, moveConfig, entity) in
                     SystemAPI.Query<RefRW<LocalTransform>,
                     RefRW<MoveComponent>,
                     RefRO<MoveConfig>>().WithEntityAccess())
            {
                if (SystemAPI.HasComponent<MountTag>(entity))
                {
                    var riderTag = SystemAPI.GetComponent<MountTag>(entity);

                    // 移动时下马导致马还在走，直接把移动方向清零，坐骑立刻停下来
                    if (riderTag.RiderEntity == Entity.Null)
                    {
                        moveComponent.ValueRW.MoveDirection = float2.zero;
                        continue;
                    }
                }

                // 键盘输入时让shape与transform同步
                var shape = SystemAPI.GetComponent<ShapeData>(entity);


                // Move around with WASD (世界坐标系下的移动，不受旋转影响)
                // float3 moveDir = new float3(moveComponent.ValueRO.MoveDirection.x, 0, moveComponent.ValueRO.MoveDirection.y);
                // float3 move = moveDir * moveConfig.ValueRO.MoveSpeed * SystemAPI.Time.DeltaTime;
                float2 moveDir = moveComponent.ValueRO.MoveDirection;
                Vector2 move = moveDir * moveConfig.ValueRO.MoveSpeed * SystemAPI.Time.DeltaTime;

                // transform.ValueRW.Position += move;
                if (transform.ValueRO.Position.y < 0)
                {
                    transform.ValueRW.Position *= new float3(1, 0, 1);
                }
                shape.Position += move;

                // 根据移动方向转向（只在有移动输入时）
                // if (math.lengthsq(moveComponent.ValueRO.MoveDirection) > 0.01f)
                // {
                //     // 计算目标方向
                //     float3 targetDir = new float3(moveComponent.ValueRO.MoveDirection.x, 0, moveComponent.ValueRO.MoveDirection.y);
                //     targetDir = math.normalize(targetDir);

                //     // 从目标方向创建四元数（向上为 Y 轴）
                //     quaternion targetRotation = quaternion.LookRotationSafe(targetDir, math.up());

                //     quaternion newRotation = math.slerp(transform.ValueRO.Rotation, targetRotation,
                //         moveConfig.ValueRO.RotationSpeed * SystemAPI.Time.DeltaTime);

                //     transform.ValueRW.Rotation = newRotation;

                //     // 测试用
                //     float3 euler = math.Euler(transform.ValueRO.Rotation);

                //     // 获取 Y 轴的弧度值
                //     shape.Rotation = euler.y;

                // }
                SystemAPI.SetComponent(entity, shape);

            }
        }
    }
}