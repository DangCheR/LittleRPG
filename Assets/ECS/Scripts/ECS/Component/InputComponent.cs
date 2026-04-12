using System.Buffers.Text;
using Unity.Entities;
using Unity.Mathematics;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 玩家输入组件
    /// </summary>
    public struct PlayerInputData : IComponentData
    {
        public float2 Move;        // 移动输入 (X, Y)
        public bool IsAttacking;   // 是否按下了攻击键
        public bool IsRolling;     // 是否按下了翻滚键

        public bool IsInteracting; // 是否按下了交互键
        // 进阶提示：在真实的动作游戏中，攻击通常用 bool 记录 "WasPressedThisFrame" (这一帧是否刚按下)
    }

    /// <summary>
    /// 挂给敌人的 AI 输入组件
    /// </summary>
    public struct ActionComponent : IComponentData
    {
        public float2 Move;        // 移动输入 (X, Y)
        public bool IsAttacking;   // 是否按下了攻击键
    }
}