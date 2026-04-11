using UnityEngine;
using QFramework;
using Unity.Entities;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 这个挂在玩家模型的 GameObject 上，标记玩家可以拿武器
    /// </summary>
    public class CanTakeWeapon : MonoBehaviour
    {
        public Transform WeaponHoldPoint;
    }
}