using Unity.Entities;
using UnityEngine;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 玩家进入交互范围的事件
    /// </summary>
    public struct EnterInteractAreaEvent
    {
        // public InteractiveComponent TargetOOPComponent; // 目标

        // 交互的目标
        public Entity TargetEntity;
    }

    /// <summary>
    /// 玩家按下交互键的事件，无需参数
    /// 按下后交给玩家的交互控制器（PlayerInteract）去广播真正的交互事件（InteractEvent）
    /// </summary>
    public struct PlayerPressInteractEvent{ }

    /// <summary>
    /// 真正的交互事件，玩家按下交互键后由 PlayerInteract 广播，携带交互目标的信息
    /// </summary>
    public struct InteractEvent
    {
        public Entity TargetEntity;
    }

    /// <summary>
    /// 玩家离开交互范围的事件
    /// </summary>
    public struct ExitInteractAreaEvent
    {
        // public InteractiveComponent TargetOOPComponent; // 目标
        public Entity TargetEntity;                     // 玩家
    }
}