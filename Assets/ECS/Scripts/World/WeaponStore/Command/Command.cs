using QFramework;
using Unity.Entities;
using UnityEngine;

namespace LittleRPG.Combat
{
        // 1. 定义武器 ID 枚举
    public enum WeaponType
    {
        None = 0,
        Box = 1,    // 拳套
        Sword = 2   // 剑
    }

    public class SelectWeaponCommand : AbstractCommand
    {
        private WeaponType mWeaponType;

        public SelectWeaponCommand(WeaponType type)
        {
            mWeaponType = type;
        }

        protected override void OnExecute()
        {
            // 1. 获取 ECS 的上帝之手
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 2. 找到玩家实体 (假设玩家有 PlayerTag)
            var query = em.CreateEntityQuery(typeof(PlayerInputData), typeof(PlayerEquipData));
            if (query.IsEmpty) return;

            var playerEntity = query.GetSingletonEntity();
            var equipData = em.GetComponentData<PlayerEquipData>(playerEntity);

            // 3. 【核心】：修改 ECS 数据！告诉玩家：“你该换武器了！”
            equipData.PendingWeapon = mWeaponType;
            em.SetComponentData(playerEntity, equipData);

            Debug.Log($"[Command] 已通知 ECS，玩家请求装备武器：{mWeaponType}");
        }
    }
}