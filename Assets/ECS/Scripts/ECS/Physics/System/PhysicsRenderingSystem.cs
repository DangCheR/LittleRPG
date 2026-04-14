using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace LittleRPG.Physics
{
    /// <summary>
    /// Entity位置与ShapeData同步
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct PhysicsRenderingSystem : ISystem
    {

        // private Vector2 gravity;

        public void OnCreate(ref SystemState state)
        {
            // gravity = new Vector2(0f, -9.81f);

        }


        public void OnUpdate(ref SystemState state)
        {

            // foreach (var trans in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TargetData>().WithAll<PhyBodyData>())
            // {
            //     //Vector3 newCamXYpos = math.lerp(cam.transform.position, trans.ValueRO.Position, 8f * (1f / 60f));
            //     /// smoothing to the player cause visible stutter at high speed
            //     Vector3 newCamXYpos = trans.ValueRO.Position;
            //     cam.transform.position = new Vector3(newCamXYpos.x, newCamXYpos.y, cam.transform.position.z);

            // }

            foreach (var (shape, trans) in SystemAPI.Query<RefRW<ShapeData>, RefRW<LocalTransform>>().WithAny<PhyBodyData>())
            {
                float fixedDeltaTime = 1f / 60f; // Assuming Fixed Timestep = (60Hz physics update)
                float alpha = ((float)SystemAPI.Time.ElapsedTime % fixedDeltaTime) / fixedDeltaTime;
                alpha = math.saturate(alpha);
                // Ensure alpha is clamped between 0-1
                //Debug.Log(alpha);
                ////trans.ValueRW.Position = new float3(math.lerp(shape.ValueRO.PreviousPosition, shape.ValueRO.Position, alpha), trans.ValueRO.Position.z);
                ////trans.ValueRW.Rotation = Quaternion.Euler(0,0, math.lerp(shape.ValueRO.PreviousRotation, shape.ValueRO.Rotation, alpha));

                // 你锁y轴啊，锁我z轴干什么
                trans.ValueRW.Position = new float3(shape.ValueRO.Position.x, 
                                                    trans.ValueRO.Position.y, 
                                                    shape.ValueRO.Position.y);
                                                    
                trans.ValueRW.Rotation = Quaternion.Euler(0, shape.ValueRO.Rotation, 0);
            }
        }
    }
}
