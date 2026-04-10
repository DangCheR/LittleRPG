using Unity.Entities;
using QFramework;

namespace LittleRPG.Combat
{
    // 运行在表现层，确保物理和伤害结算（Simulation）已经跑完了
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UIBridgeSystem : SystemBase, ICanSendEvent
    {
        protected override void OnUpdate()
        {
            // 【天帝级魔法】：WithChangeFilter！
            // 这行代码意味着：如果这一帧 HealthData 没有被任何人修改过，
            // 这个循环直接跳过！连进都不会进去！性能开销绝对为 0！
            // 只有当 HealthSystem 扣了血，修改了 Chunk 版本号，这里才会被唤醒！
            foreach (var (health, entity) in SystemAPI.Query<RefRO<HealthData>>()
                     .WithAll<PlayerInputData>() // 只监听玩家的血量，不管怪物
                     .WithChangeFilter<HealthData>()
                     .WithEntityAccess())
            {
                float current = health.ValueRO.Current;
                float max = health.ValueRO.Max;

                // 桥梁在此打通！
                // ECS 系统直接调用 QFramework 的单例架构，发射全局事件！
                this.SendEvent(new PlayerHealthChangedEvent 
                { 
                    CurrentHP = current, 
                    MaxHP = max 
                });

                UnityEngine.Debug.Log($"[UI Bridge] 捕获到玩家血量变化，已通知 UI：{current}/{max}");
            }
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;

    }
}