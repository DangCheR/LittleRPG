using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace LittleRPG.Combat
{
    public class MountAuthoring : MonoBehaviour
    {
        public float MountMoveSpeed = 3.0f;

        public float MountRotationSpeed = 15.0f; // 转身速度，越大转身越快
        
        class bake : Baker<MountAuthoring>
        {
            public override void Bake(MountAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                // 添加骑乘标签
                AddComponent(entity, new MountTag());

                // 添加交互组件
                AddComponent(entity, new InteractableTag
                {
                    IsInRange = false,
                    IsInteracting = false
                });

                // 添加移动配置组件，骑乘时可能需要不同的移动参数
                AddComponent(entity, new MoveConfig
                {
                    MoveSpeed = authoring.MountMoveSpeed,
                    RotationSpeed = authoring.MountRotationSpeed
                });

                AddComponent(entity, new MoveComponent());
            }
        }
    }
}