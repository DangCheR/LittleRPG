using QFramework;
using Unity.Entities;

namespace LittleRPG.Combat
{
    public class UsePotionCommand : AbstractCommand
    {
        private float mHealAmount;

        public UsePotionCommand(float healAmount)
        {
            mHealAmount = healAmount;
        }

        protected override void OnExecute()
        {
            // 1. 获取 ECS 的上帝之手
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;

            // 2. 找到玩家实体 (ECS 1.0+ 提供了 SystemAPI，但在传统的 C# 类里我们需要用 Query)
            var query = em.CreateEntityQuery(typeof(PlayerInputData));
            if (query.IsEmpty) return;

            Entity playerEntity = query.GetSingletonEntity();

            // 3. 【核心】：不要直接加血！给玩家的“恢复信箱”里塞一张账单！
            // 假设你像之前做伤害那样，做了一个 HealBufferElement
            if (em.HasBuffer<HealBufferElement>(playerEntity))
            {
                var healBuffer = em.GetBuffer<HealBufferElement>(playerEntity);
                healBuffer.Add(new HealBufferElement { Value = mHealAmount });

                UnityEngine.Debug.Log($"[Command] 玩家使用血瓶，已向 ECS 投递 {mHealAmount} 点恢复请求！");
            }
        }
    }
}