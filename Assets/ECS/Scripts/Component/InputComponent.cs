using System.Buffers.Text;
using Unity.Entities;
using Unity.Mathematics;

namespace LittleRPG.Combat
{
    // 这个组件挂在玩家 Entity 身上
    public struct PlayerInputData : IComponentData
    {
        public float2 Move;        // 移动输入 (X, Y)
        public bool IsAttacking;   // 是否按下了攻击键
        public bool IsRolling;     // 是否按下了翻滚键

        // 进阶提示：在真实的动作游戏中，攻击通常用 bool 记录 "WasPressedThisFrame" (这一帧是否刚按下)
    }

    public struct ActionComponent : IComponentData
    {
        public float2 Move;        // 移动输入 (X, Y)
        public bool IsAttacking;   // 是否按下了攻击键
    }
}