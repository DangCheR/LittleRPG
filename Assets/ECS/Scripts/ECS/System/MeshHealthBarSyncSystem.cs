using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 手动绘制血条
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct MeshHealthBarSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 获取一个可以写入 MaterialProperty 的查表工具
            var hpBarLookup = SystemAPI.GetComponentLookup<HealthBarMaterialProperty>(false);

            // 开启多线程 Job！
            new SyncHealthBarJob
            {
                HpBarLookup = hpBarLookup
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct SyncHealthBarJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public ComponentLookup<HealthBarMaterialProperty> HpBarLookup;

        // 【性能核爆】：只遍历血量发生变化的实体！
        void Execute(in HealthData health, in HealthBarReference barRef)
        {
            if(health.lastHP == health.Current) return;
            // 如果血条实体存在
            if (HpBarLookup.HasComponent(barRef.BarEntity))
            {
                // 计算百分比
                float pct = health.Current / health.Max;

                // 【终极魔法】：直接修改 GPU 数据！
                // 这行代码执行完，Entities Graphics 底层会利用 SSBO 
                // 直接把这个 float 塞进 GPU 的显存里！完全没有 C# 对象的开销！
                HpBarLookup[barRef.BarEntity] = new HealthBarMaterialProperty { Value = pct };
            }
        }
    }
}