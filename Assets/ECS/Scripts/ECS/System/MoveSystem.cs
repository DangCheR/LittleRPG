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

                // if (SystemAPI.HasComponent<ShapeData>(entity))
                // {

                // }

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
                if (math.lengthsq(moveComponent.ValueRO.MoveDirection) > 0.01f)
                {
                    float targetAngle = math.degrees(math.atan2(moveDir.x, moveDir.y));

                    // 2. 获取当前角度
                    float currentAngle = shape.Rotation;

                    // 3. 【关键】计算最短旋转差值
                    // 这个公式能保证 angleDiff 永远在 -180 到 180 之间
                    float angleDiff = (targetAngle - currentAngle + 540f) % 360f - 180f;

                    // 4. 平滑插值这个“差值”
                    // 这里的 10f 是平滑系数，越大转得越快
                    float smoothFactor = moveConfig.ValueRO.RotationSpeed;
                    float step = angleDiff * smoothFactor * SystemAPI.Time.DeltaTime;

                    // 5. 直接修改 shape，解决不丝滑问题
                    shape.Rotation = currentAngle + step;

                }

                // 如果马上骑得有人，给人的shape也动起来
                if (SystemAPI.HasComponent<MountTag>(entity))
                {
                    var riderTag = SystemAPI.GetComponent<MountTag>(entity);

                    // 移动时下马导致马还在走，直接把移动方向清零，坐骑立刻停下来
                    if (riderTag.RiderEntity != Entity.Null)
                    {
                        SystemAPI.SetComponent(riderTag.RiderEntity, shape);

                    }
                }
                SystemAPI.SetComponent(entity, shape);

            }
        }
    }
}