using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 我再也不装逼了，InputSystem只能用SystemBase
/// </summary>
namespace LittleRPG.Combat
{
#if !UNITY_DISABLE_MANAGED_COMPONENTS
    // 【关键】：确保在所有游戏逻辑（Simulation）开始之前收集输入！
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class PlayerInputGatherSystem : SystemBase
    {
        private InputSystem_Actions inputActions;

        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction rollAction;

        // [BurstCompile]
        protected override void OnCreate()
        {
            // 确保接收输入的实体存在才运行
            RequireForUpdate<PlayerInputData>();

            inputActions = new InputSystem_Actions();

            moveAction = inputActions.Player.Move;
            attackAction = inputActions.Player.Attack;
            rollAction = inputActions.Player.Roll;

            moveAction.Enable();
            attackAction.Enable();
            rollAction.Enable();
        }

        // [BurstCompile]
        protected override void OnUpdate()
        {
            ref var inputState = ref SystemAPI.GetSingletonRW<PlayerInputData>().ValueRW;

            // 1. 从 New Input System 获取当前帧的值
            float2 moveInput = moveAction.ReadValue<Vector2>();

            // 注意：攻击和翻滚通常需要“按下的那一瞬间”触发，而不是一直按着
            bool isAttacking = attackAction.WasPressedThisFrame();
            bool isRolling = rollAction.WasPressedThisFrame();

            if (math.lengthsq(moveInput) > 1f)
            {
                moveInput = math.normalize(moveInput);
            }

            inputState.Move = moveInput;
            inputState.IsAttacking = isAttacking;
            inputState.IsRolling = isRolling;
        }
    }
#endif
}