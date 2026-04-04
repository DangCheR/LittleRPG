using Unity.Entities;

namespace LittleRPG.Combat
{
    // 在 Baker 里把这个挂给玩家 Entity
    public struct PlayerCombatState : IComponentData
    {
        // 攻击判定的“扳机”
        public bool TriggerAttackHit; 
        
        // 顺便可以配一下攻击参数
        public float AttackRange;
        public float AttackDamage;
    }
}