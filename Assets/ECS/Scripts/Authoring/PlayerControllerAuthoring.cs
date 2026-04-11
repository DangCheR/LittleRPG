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
                    PlayerRollSpeed = authoring.PlayerRollSpeed,
                    PlayerAttackSpeed = authoring.PlayerAttackSpeed,
                });

                AddComponent(entity, new MoveConfig
                {
                    MoveSpeed = authoring.PlayerMoveSpeed,
                    RotationSpeed = authoring.RotationSpeed
                });

                AddComponent<MoveComponent>(entity); // 添加移动组件，初始移动方向为 0

                AddComponent<PlayerInputData>(entity); // 初始化输入

                AddBuffer<DamageBufferElement>(entity); // 可受伤
                
                AddBuffer<HealBufferElement>(entity); // 可回血

                // 初始化攻击
                AddComponent(entity, new PlayerCombatState
                {
                    AttackRange = 1,
                    AttackDamage = 4
                });

                // 初始化交互组件
                AddComponent(entity, new Interactor
                {
                    Range = 2f // 交互范围
                });

                // 初始化状态
                AddComponent(entity, new PlayerState
                {
                    IsAttacking = false,
                    IsMounted = false,
                    IsInteracting = false,
                    IsMoving = false
                });
                
                // 给玩家打上一个“骑乘标签”，表示玩家可以骑乘
                AddComponent(entity, new RiderTag());
            }
        }
    }
}