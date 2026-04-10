using Unity.Entities;
using Unity.Mathematics;
namespace LittleRPG.Combat
{
    public struct ControllerConfig : IComponentData
    {
        //动作的速度对应不同的动画播放速度
        public float PlayerRollSpeed; // 翻滚速度
        public float PlayerAttackSpeed; // 攻击速度
    }

    public struct MoveConfig : IComponentData
    {
        public float MoveSpeed;
        public float RotationSpeed;
    }

    public struct MoveComponent : IComponentData
    {
        public float2 MoveDirection; // 当前的移动输入方向，范围是 (-1, -1) 到 (1, 1)
    }
}