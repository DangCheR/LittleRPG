using Unity.Entities;
namespace LittleRPG.Combat
{
    public struct HealthData : IComponentData
    {
        public float Current;
        public float Max;
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

    public struct DeadTag : IComponentData, IEnableableComponent { }
}