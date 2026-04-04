using Unity.Entities;
namespace LittleRPG.Combat
{
    public struct HealthData : IComponentData
    {
        public int Value;
        public int Max;
    }
}