using Unity.Entities;
using Unity.Rendering;

namespace LittleRPG.Combat
{
    public struct HealthData : IComponentData
    {
        public float lastHP;
        public float Current;
        public float Max;
    }

    /// <summary>
    /// 处理回血
    /// </summary>
    public struct HealBufferElement : IBufferElementData
    {
        public float Value;
        // public Entity Attacker; // (谁打的，用来算击杀奖励)
    }

    /// <summary>
    /// 处理伤害，使用数组
    /// </summary>
    public struct DamageBufferElement : IBufferElementData
    {
        public float Value;
        public Entity Attacker; // (谁打的，用来算击杀奖励)
        // 你还可以扩展：public int DamageType; (物理还是魔法，用来算抗性)
    }

    [MaterialProperty("_HealthPct")]
    public struct HealthBarMaterialProperty : IComponentData
    {
        public float Value; // 存 0.0 到 1.0 的血量百分比
    }

    /// <summary>
    /// 记录血条
    /// </summary>
    public struct HealthBarReference : IComponentData
    {
        public Entity BarEntity;
    }
    
    /// <summary>
    /// 死亡标记开始清算
    /// 但是动画还要播放死亡动画
    /// </summary>
    public struct DeadTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// 动画也播放完了，彻底寄了，需要收尸
    /// </summary>
    public struct DeceasedTag : IComponentData, IEnableableComponent { }
}