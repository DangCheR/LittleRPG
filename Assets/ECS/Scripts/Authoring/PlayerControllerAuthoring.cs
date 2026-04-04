using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace LittleRPG.Combat
{
    public class PlayerControllerAuthoring : MonoBehaviour
    {
        public float PlayerMoveSpeed = 3.0f;
        public float PlayerRollSpeed = 1.0f;
        public float PlayerAttackSpeed = 1.0f;
        public float RotationSpeed = 15.0f; // 转身速度，越大转身越快


        class Baker : Baker<PlayerControllerAuthoring>
        {
            public override void Bake(PlayerControllerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                // 给玩家 Entity 添加输入组件，初始值为 0
                AddComponent(entity, new ControllerConfig
                {
                    PlayerMoveSpeed = authoring.PlayerMoveSpeed,
                    PlayerRollSpeed = authoring.PlayerRollSpeed,
                    PlayerAttackSpeed = authoring.PlayerAttackSpeed,
                    RotationSpeed = authoring.RotationSpeed
                });
                AddComponent<PlayerInputData>(entity); // 初始化输入
                AddComponent<PlayerCombatState>(entity); // 初始化攻击
            }
        }
    }

    public class InputSystemConfig : IComponentData
    {
        public InputSystem_Actions inputActions;
        public InputAction m_MoveAction;
        public InputAction m_AttackAction;
        public InputAction m_RollAction;
    }

    public class StaticInputSystem
    {
        public static InputSystem_Actions instance = new InputSystem_Actions();
        public InputAction m_MoveAction;

    }
    public struct ControllerConfig : IComponentData
    {
        public float PlayerMoveSpeed; // 移动速度
        public float RotationSpeed; // 转身速度，越大转身越快


        //动作的速度对应不同的动画播放速度
        public float PlayerRollSpeed; // 翻滚速度
        public float PlayerAttackSpeed; // 攻击速度
    }

}