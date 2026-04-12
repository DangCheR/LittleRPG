using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using QFramework;

namespace LittleRPG.Combat
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PlayerInteractionSystem : SystemBase, ICanSendEvent
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Interactor>();
            RequireForUpdate<InteractableTag>();

        }
        protected override void OnUpdate()
        {

            // 1. 找到玩家和他的输入
            foreach (var (playerTrans, interactor) in
                        SystemAPI.Query<RefRO<LocalTransform>,
                        RefRW<Interactor>>())
            {
                // 已经在交互了，就不需要再找目标了
                if (interactor.ValueRO.CurrentInteractiveTarget != Entity.Null)
                {
                    continue;
                }

                float3 playerPos = playerTrans.ValueRO.Position;
                float interactRangeSq = interactor.ValueRO.Range * interactor.ValueRO.Range;

                Entity closestEntity = Entity.Null;
                float minDistSq = float.MaxValue;

                // 2. 暴力 Query 寻找最近的可交互物体
                foreach (var (targetTrans, _inter, targetEntity) in
                         SystemAPI.Query<RefRO<LocalTransform>, RefRW<InteractableTag>>().WithEntityAccess())
                {
                    float distSq = math.distancesq(playerPos, targetTrans.ValueRO.Position);
                    if (distSq <= interactRangeSq && distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closestEntity = targetEntity;
                    }
                    else
                    {
                        if (_inter.ValueRW.IsInRange == true)
                        {
                            _inter.ValueRW.IsInRange = false;
                            this.SendEvent(new ExitInteractAreaEvent
                            {
                                TargetEntity = targetEntity
                            });
                        }
                        // 离得远了，确保它的交互状态被重置
                        _inter.ValueRW.IsInteracting = false;
                    }
                }

                // 没找到可交互的物体，发个事件告诉它们玩家离开了范围，然后结束
                if (closestEntity == Entity.Null)
                {
                    if (interactor.ValueRO.CurrentTarget == Entity.Null) return; // 本来就没有目标，那就不发什么事件了

                    this.SendEvent(new ExitInteractAreaEvent
                    {
                        TargetEntity = interactor.ValueRO.CurrentTarget
                    });

                    var oldInter = SystemAPI.GetComponent<InteractableTag>(interactor.ValueRO.CurrentTarget);
                    oldInter.IsInRange = false;
                    interactor.ValueRW.CurrentTarget = Entity.Null;

                    return;
                }

                // 还是这家伙，不发事件了
                if (interactor.ValueRO.CurrentTarget != Entity.Null
                    && interactor.ValueRO.CurrentTarget == closestEntity) return;

                // 切换目标了，先重置之前的目标状态，再设置新目标状态
                if (interactor.ValueRO.CurrentTarget != Entity.Null)
                {
                    var oldInter = SystemAPI.GetComponent<InteractableTag>(interactor.ValueRO.CurrentTarget);
                    oldInter.IsInRange = false;

                    this.SendEvent(new ExitInteractAreaEvent
                    {
                        TargetEntity = interactor.ValueRO.CurrentTarget
                    });
                }

                // 更新当前目标
                interactor.ValueRW.CurrentTarget = closestEntity;
                {
                    var inter = SystemAPI.GetComponent<InteractableTag>(closestEntity);
                    inter.IsInRange = true;
                    Debug.Log($"玩家进入了一个新的交互范围，目标 Entity: {closestEntity.Index}");

                    this.SendEvent(new EnterInteractAreaEvent
                    {
                        TargetEntity = closestEntity
                    });

                }
            }
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;

    }
}