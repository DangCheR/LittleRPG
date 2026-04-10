using Unity.Entities;
using UnityEngine;

/// <summary>
/// 玩家进入交互范围的事件
/// </summary>
public struct EnterInteractAreaEvent
{
    // public InteractiveComponent TargetOOPComponent; // 目标
    public Entity target;                     // 玩家
}

/// <summary>
/// 玩家离开交互范围的事件
/// </summary>
public struct ExitInteractAreaEvent
{
    // public InteractiveComponent TargetOOPComponent; // 目标
    public Entity target;                     // 玩家
}