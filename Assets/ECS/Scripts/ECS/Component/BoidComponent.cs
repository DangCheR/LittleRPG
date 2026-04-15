using Unity.Entities;

namespace LittleRPG.Physics
{
    public struct BoidBrain : IComponentData
    {
        // 三大铁律权重
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;

        // 带领小弟的“忠诚度”
        public float LeaderFollowWeight;

        // 视野半径（雷达范围）
        public float ViewRadius;
    }
}
