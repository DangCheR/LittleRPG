using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace LittleRPG.Combat
{
    public partial struct MoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, moveComponent, moveConfig) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<MoveComponent>, RefRO<MoveConfig>>())
            {
                // Move around with WASD (世界坐标系下的移动，不受旋转影响)
                float3 moveDir = new float3(moveComponent.ValueRO.MoveDirection.x, 0, moveComponent.ValueRO.MoveDirection.y);
                float3 move = moveDir * moveConfig.ValueRO.MoveSpeed * SystemAPI.Time.DeltaTime;

                // 先不让他跳
                // controller.ValueRW.VerticalSpeed -= 10.0f * SystemAPI.Time.DeltaTime;
                // controller.ValueRW.VerticalSpeed = math.max(-10.0f, controller.ValueRO.VerticalSpeed);
                // move.y = controller.ValueRO.VerticalSpeed * SystemAPI.Time.DeltaTime;

                transform.ValueRW.Position += move;
                if (transform.ValueRO.Position.y < 0)
                {
                    transform.ValueRW.Position *= new float3(1, 0, 1);
                }

                // 根据移动方向转向（只在有移动输入时）
                if (math.lengthsq(moveComponent.ValueRO.MoveDirection) > 0.01f)
                {
                    // 计算目标方向
                    float3 targetDir = new float3(moveComponent.ValueRO.MoveDirection.x, 0, moveComponent.ValueRO.MoveDirection.y);
                    targetDir = math.normalize(targetDir);
                    
                    // 从目标方向创建四元数（向上为 Y 轴）
                    quaternion targetRotation = quaternion.LookRotationSafe(targetDir, math.up());
                    
                    quaternion newRotation = math.slerp(transform.ValueRO.Rotation, targetRotation, 
                        moveConfig.ValueRO.RotationSpeed * SystemAPI.Time.DeltaTime);
                    
                    transform.ValueRW.Rotation = newRotation;
                }

                // // Camera look up/down
                // var turnCam = -input.MouseY * controller.ValueRO.MouseSensitivity * SystemAPI.Time.DeltaTime;
                // controller.ValueRW.CameraPitch += turnCam;

                // // Jump
                // if (input.Space)
                // {
                //     controller.ValueRW.VerticalSpeed = controller.ValueRO.JumpSpeed;
                // }
            }
        }
    }
}