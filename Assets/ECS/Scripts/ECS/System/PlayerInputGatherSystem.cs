using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using QFramework;
/// <summary>
/// 我再也不装逼了，InputSystem只能用SystemBase
/// </summary>
namespace LittleRPG.Combat
{
#if !UNITY_DISABLE_MANAGED_COMPONENTS
    // 【关键】：确保在所有游戏逻辑（Simulation）开始之前收集输入！
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class PlayerInputGatherSystem : SystemBase, ICanSendEvent
    {
        private InputSystem_Actions inputActions;

        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction rollAction;
        private InputAction InteractAction;

        // [BurstCompile]
        protected override void OnCreate()
        {
            // 确保接收输入的实体存在才运行
            RequireForUpdate<PlayerInputData>();

            inputActions = new InputSystem_Actions();

            moveAction = inputActions.Player.Move;
            attackAction = inputActions.Player.Attack;
            rollAction = inputActions.Player.Roll;
            InteractAction = inputActions.Player.Interact;

            moveAction.Enable();
            attackAction.Enable();
            rollAction.Enable();
            InteractAction.Enable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            moveAction.Disable();
            moveAction.Dispose();

            attackAction.Disable();
            attackAction.Dispose();

            rollAction.Disable();
            rollAction.Dispose();

            inputActions.Disable();
            inputActions.Dispose();
        }

        // [BurstCompile]
        protected override void OnUpdate()
        {
            ref var inputState = ref SystemAPI.GetSingletonRW<PlayerInputData>().ValueRW;

            // 把输入分割为移动、攻击、翻滚和交互四个部分，分别存到 PlayerInputData 里
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerInputData>();

            var moveComponent = SystemAPI.GetComponentRW<MoveComponent>(playerEntity); // 先拿到当前输入状态，准备修改它
            var PlayerState = SystemAPI.GetComponentRW<PlayerState>(playerEntity); // 先拿到当前输入状态，准备修改它
            var AttackSate = SystemAPI.GetComponentRW<AttackSate>(playerEntity); // 先拿到当前输入状态，准备修改它
            var RiderState = SystemAPI.GetComponentRW<RiderTag>(playerEntity); // 先拿到当前输入状态，准备修改它

            // 1. 从 New Input System 获取当前帧的值
            float2 moveInput = moveAction.ReadValue<Vector2>();

            // 注意：攻击和翻滚通常需要“按下的那一瞬间”触发，而不是一直按着
            bool isAttacking = attackAction.WasPressedThisFrame();
            bool isRolling = rollAction.WasPressedThisFrame();
            bool isInteracting = InteractAction.WasPressedThisFrame();

            if (math.lengthsq(moveInput) > 1f)
            {
                moveInput = math.normalize(moveInput);
            }

            // 如果玩家正在骑乘，输入应该控制坐骑而不是玩家自己
            if (RiderState.ValueRO.MountEntity != Entity.Null)
            {
                var MountMoveComponent = SystemAPI.GetComponentRW<MoveComponent>(RiderState.ValueRO.MountEntity); // 先拿到当前输入状态，准备修改它
                MountMoveComponent.ValueRW.MoveDirection = moveInput; // 设置移动方向
                moveComponent.ValueRW.MoveDirection = default; // 设置移动方向
            }
            else
            {
                moveComponent.ValueRW.MoveDirection = moveInput; // 设置移动方向
            }

            if (isAttacking)
            {
                AttackSate.ValueRW.StartAttack = true;
            }
            else
            {
                AttackSate.ValueRW.StartAttack = false;
            }
            // AttackSate.ValueRW.StartAttack = isAttacking;
            // PlayerState.ValueRW.IsAttacking = isAttacking;

            inputState.IsRolling = isRolling;

            // 交互事件需要发给主世界的交互系统
            // 所以我们通过 ICanSendEvent 的接口发一个事件告诉玩家交互控制器
            if (isInteracting)
            {
                this.SendEvent<PlayerPressInteractEvent>(new());
            }

            // PlayerState.ValueRW.IsInteracting = isInteracting;
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;

    }
#endif
}