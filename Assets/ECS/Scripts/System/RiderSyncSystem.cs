using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 由于玩家模型的坐标是由骑乘坐骑控制的
    /// 所以需要一个系统把坐骑的坐标同步给玩家模型
    /// </summary>
    /// [UpdateInGroup(typeof(SimulationSystemGroup))][UpdateBefore(typeof(TransformSystemGroup))] // 在底层计算坐标之前执行
    public partial struct RiderSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RiderTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 查表工具：用来根据小猪的 Entity ID 获取小猪的坐标
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

            // 遍历所有正在骑马的玩家
            foreach (var (playerTransform, riderTag) in 
                     SystemAPI.Query<RefRW<LocalTransform>,
                      RefRO<RiderTag>>())
            {
                if (riderTag.ValueRO.MountEntity == Entity.Null)
                {
                    continue; // 没有骑乘坐骑，跳过
                }

                // 坐标锁定在坐骑上
                if (transformLookup.TryGetComponent(riderTag.ValueRO.MountEntity, out var pigTransform))
                {
                    playerTransform.ValueRW.Position.x = pigTransform.Position.x;
                    playerTransform.ValueRW.Position.z = pigTransform.Position.z;
                    playerTransform.ValueRW.Rotation = pigTransform.Rotation;
                }
            }
        }
    }
}