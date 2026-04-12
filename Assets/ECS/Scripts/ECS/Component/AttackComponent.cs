using Unity.Entities;

/// <summary>
/// 玩家状态
/// </summary>
namespace LittleRPG.Combat
{
    // 在 Baker 里把这个挂给玩家 Entity
    public struct PlayerCombatState : IComponentData
    {
        //开始攻击的帧，用与动画系统同步攻击状态，和攻击判定系统判断是否要进行攻击判定
        public bool StartAttack;

        // 攻击判定的帧
        public bool TriggerAttackHit;

        // 配一下攻击参数
        // public float AttackRange;
        // public float AttackDamage;
    }

}