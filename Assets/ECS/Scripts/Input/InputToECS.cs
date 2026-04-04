using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.Mathematics;
namespace LittleRPG.Combat
{
    public class InputToECS : MonoBehaviour
    {
        private InputSystem_Actions inputActions;

        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction rollAction;

        private EntityManager entityManager;
        private EntityQuery playerQuery;

        void Awake()
        {
            // 初始化输入
            inputActions = new InputSystem_Actions();

            moveAction = inputActions.Player.Move;
            attackAction = inputActions.Player.Attack;
            rollAction = inputActions.Player.Roll;

            moveAction.Enable();
            attackAction.Enable();
            rollAction.Enable();

            // 获取 ECS 世界
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 查找所有玩家（有输入组件的）
            playerQuery = entityManager.CreateEntityQuery(typeof(PlayerInputData));
        }

        void OnDestroy()
        {
            moveAction.Dispose();
            attackAction.Dispose();
            rollAction.Dispose();
        }

        void Update()
        {
            if (playerQuery.IsEmpty) return;

            // 读取输入
            float2 move = moveAction.ReadValue<Vector2>();
            bool attack = attackAction.WasPressedThisFrame();
            bool roll = rollAction.WasPressedThisFrame();

            // 归一化（防止斜向更快）
            if (math.lengthsq(move) > 1f)
            {
                move = math.normalize(move);
            }

            // 写入 ECS（单人版）
            var entity = playerQuery.GetSingletonEntity();

            var inputData = entityManager.GetComponentData<PlayerInputData>(entity);
            inputData.Move = move;
            inputData.IsAttacking = attack;
            inputData.IsRolling = roll;

            entityManager.SetComponentData(entity, inputData);
        }
    }
}