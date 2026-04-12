using Unity.Entities;
using UnityEngine;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 武器组件，标记玩家当前装备的武器实体
    /// </summary>
    public struct TakeWeapon : IComponentData
    {
        public Entity EquippedWeapon; // 当前装备的武器实体
    }

    // public struct WeaponData : ISharedComponentData
    // {
    //     public int AttackDamage; // 武器的攻击力
    //     public float AttackRange; // 武器的攻击范围
    // }

    /// <summary>
    /// 同时记录所属信息，用于位置同步
    /// </summary>
    public struct WeaponBelong : IComponentData
    {
        public Entity OwnerPlayer; // 拥有这个武器的玩家实体
    }

    public class WeaponBelongBone : IComponentData
    {
        public Transform WeaponHoldPoint; // 右手骨骼节点
    }

    public struct WeaponData : IComponentData
    {
        public float AttackDamage; // 武器的攻击力

        public float AttackRange; // 武器的攻击范围
        // 这个组件用来标记一个实体是武器，可以被玩家拿起
    }

    // 4. 给玩家加一个组件，记录他想装备什么
    public struct PlayerEquipData : IComponentData
    {
        public WeaponType CurrentWeapon;
        public WeaponType PendingWeapon; // 待装备的武器（用来触发换武器逻辑）
    }

    // 2. 纯数据：ECS 里的军械库单例
    public struct WeaponRegistryData : IComponentData
    {
        public Entity BoxPrefab;
        public Entity SwordPrefab;
    }
}
