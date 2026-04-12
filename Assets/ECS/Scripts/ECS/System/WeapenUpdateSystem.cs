using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using System.Diagnostics;

namespace LittleRPG.Combat
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct WeapenUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // 必须等军械库烘焙好才能运行
            state.RequireForUpdate<WeaponRegistryData>();
        }

        // [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. 从单例中获取军械库数据
            var registry = SystemAPI.GetSingleton<WeaponRegistryData>();
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // 2. 遍历所有请求换武器的玩家
            foreach (var (equipData, entity) in SystemAPI.Query<RefRW<PlayerEquipData>>().WithEntityAccess())
            {
                // 如果没有待装备的武器，跳过
                if (equipData.ValueRO.PendingWeapon == WeaponType.None) continue;

                // --- 核心换装逻辑 ---
                WeaponType newWeaponType = equipData.ValueRO.PendingWeapon;
                Entity weaponPrefabToSpawn = Entity.Null;

                // 查字典：根据 ID 获取 Entity 预制体
                switch (newWeaponType)
                {
                    case WeaponType.Box:
                        weaponPrefabToSpawn = registry.BoxPrefab;
                        break;
                    case WeaponType.Sword:
                        weaponPrefabToSpawn = registry.SwordPrefab;
                        break;
                }

                if (weaponPrefabToSpawn != Entity.Null)
                {
                    // 1. 实例化武器实体
                    Entity newWeaponEntity = ecb.Instantiate(weaponPrefabToSpawn);

                    // 给武器添加所属关系
                    ecb.AddComponent(newWeaponEntity, new WeaponBelong
                    {
                        OwnerPlayer = entity
                    });
                    var takeWeapon = SystemAPI.GetComponent<TakeWeapon>(entity);

                    // 消除旧武器
                    if (takeWeapon.EquippedWeapon != Entity.Null)
                    {
                        ecb.DestroyEntity(takeWeapon.EquippedWeapon);
                    }

                    // 添加新武器
                    takeWeapon.EquippedWeapon = newWeaponEntity;
                    ecb.SetComponent(entity, takeWeapon);
                }

                // 更新当前武器，并清空请求，防止每帧重复生成
                equipData.ValueRW.CurrentWeapon = newWeaponType;
                UnityEngine.Debug.Log($"玩家 {entity.Index} 已装备武器：{newWeaponType}");
                equipData.ValueRW.PendingWeapon = WeaponType.None;
            }

            // 遍历所有带有追踪器的武器实体
            foreach (var (weaponTrans, followData) in
                     SystemAPI.Query<RefRW<LocalTransform>,
                     RefRO<WeaponBelong>>())
            {
                // 找到这个武器的主人
                Entity owner = followData.ValueRO.OwnerPlayer;

                // 检查主人是否还活着，并且是否有 3D 皮囊
                if (SystemAPI.ManagedAPI.HasComponent<RunningAnimation>(owner))
                {
                    // 拿到主人的皮囊
                    var animGO = SystemAPI.ManagedAPI.GetComponent<WeaponBelongBone>(owner);

                    if (animGO.WeaponHoldPoint != null)
                    {
                        // 【跨次元壁同步】：将 GameObject 骨骼的世界坐标，强行覆盖给 ECS 的武器！
                        weaponTrans.ValueRW.Position = animGO.WeaponHoldPoint.position;
                        weaponTrans.ValueRW.Rotation = animGO.WeaponHoldPoint.rotation;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}