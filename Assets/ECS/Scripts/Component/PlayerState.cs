using Unity.Entities;

/// <summary>
/// 玩家状态
/// </summary>
namespace LittleRPG.Combat
{

    /// <summary>
    /// 玩家总状态组件，包含玩家的各种状态标记和参数
    /// </summary>
    public struct PlayerState : IComponentData
    {
        public bool IsAttacking; // 是否正在攻击
        public bool IsMounted;   // 是否正在骑乘
        public bool IsInteracting; // 是否正在交互
        public bool IsMoving; // 是否处于战斗状态
    }

    // 在 Baker 里把这个挂给玩家 Entity
    public struct PlayerCombatState : IComponentData
    {
        // 攻击判定的“扳机”
        public bool TriggerAttackHit;

        // 顺便可以配一下攻击参数
        public float AttackRange;
        public float AttackDamage;
    }

    public struct PlayerMountState : IComponentData
    {
        public Entity MountEntity; // 玩家正在骑乘的坐骑实体
    }
}