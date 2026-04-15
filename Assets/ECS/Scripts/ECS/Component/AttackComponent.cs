using Unity.Entities;

/// <summary>
/// 玩家状态
/// </summary>
namespace LittleRPG.Combat
{
    /// <summary>
    /// 拥有可攻击属性和动画，挂载
    /// </summary>
    public struct AttackSate : IComponentData
    {
        //开始攻击的帧，用与动画系统同步攻击状态，和攻击判定系统判断是否要进行攻击判定
        public bool StartAttack;

        // 攻击判定的帧
        public bool TriggerAttackHit;
    }

    /// <summary>
    /// 攻击碰撞盒附加信息
    /// </summary>
    public struct AttackInfo : IComponentData
    {
        // 伤害数值
        public float damage;

        // 攻击发起者
        public Entity attacker;
    }

    public struct AttackHit
    {
        public Entity Attacker;
        public Entity Victim;
        public float Damage;
    }

}