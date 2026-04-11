using Unity.Entities;
using UnityEngine;

/// <summary>
/// 上马的交互
/// </summary>
namespace LittleRPG.Combat
{
    // 这个挂在猪的 GameObject 上，并且挂上 InteractableAuthoring (Radius=3)
    public class MountInteractive : BaseInteractive
    {
        public Transform SeatPoint;
        public Transform LeftFootPoint;
        public Transform RightFootPoint;

        public override bool ExecuteInteract()
        {
            var CanRider = entityManager.GetComponentData<RiderTag>(playerEntity);
            var CanMount = entityManager.GetComponentData<MountTag>(OwnerEntity);

            // 获取玩家的世界GO
            var PlayerRunningAnimation = entityManager.GetComponentData<RunningAnimation>(playerEntity);
            GameObject PlayerModel = PlayerRunningAnimation.RunningModel;

            if (PlayerModel.TryGetComponent<AnimationRider>(out var riderIK))
            {
                Debug.Log("承太郎已经骑上去了！");
                riderIK.StartRiding(SeatPoint, LeftFootPoint, RightFootPoint);

                // 重置玩家状态
                CanRider.MountEntity = OwnerEntity; // 记录正在骑乘的坐骑实体
                entityManager.SetComponentData(playerEntity, CanRider);

                // 重置坐骑状态
                CanMount.RiderEntity = playerEntity; // 记录正在骑乘的骑手实体
                entityManager.SetComponentData(OwnerEntity, CanMount);
            }

            return true; // 这个交互是持续性的，需要玩家再次交互来结束  
        }

        public override void ExecuteEndInteract()
        {
            var CanRider = entityManager.GetComponentData<RiderTag>(playerEntity);
            var CanMount = entityManager.GetComponentData<MountTag>(OwnerEntity);
            // 获取玩家的世界GO
            var PlayerRunningAnimation = entityManager.GetComponentData<RunningAnimation>(playerEntity);
            GameObject PlayerModel = PlayerRunningAnimation.RunningModel;

            if (PlayerModel.TryGetComponent<AnimationRider>(out var riderIK))
            {
                Debug.Log("承太郎已经骑上去了！");
                // 先停止骑乘状态
                riderIK.StopRiding();

                // 重置玩家骑乘状态
                CanRider.MountEntity = Entity.Null;
                entityManager.SetComponentData(playerEntity, CanRider);

                // 重置坐骑状态
                CanMount.RiderEntity = Entity.Null;
                entityManager.SetComponentData(OwnerEntity, CanMount);
            }

            return; // 这个交互是持续性的，需要玩家再次交互来结束  
        }
    }
}