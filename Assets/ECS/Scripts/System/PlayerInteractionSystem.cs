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
            foreach (var (playerTrans, input, interactor, playerAnimGO) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerInputData>, RefRO<Interactor>, RunningAnimation>())
            {
                // 玩家没按交互键（比如 F 键或 E 键）？直接跳过！
                // 假设你把交互键绑定到了 IsRolling，或者加个 IsInteracting 字段
                if (!input.ValueRO.IsInteracting) continue;

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
                                target = targetEntity
                            });
                        }
                        // 离得远了，确保它的交互状态被重置
                        _inter.ValueRW.IsInteracting = false;
                    }
                }

                // 3. 找到了！触发它！
                if (closestEntity != Entity.Null)
                {
                    var inter = SystemAPI.GetComponent<InteractableTag>(closestEntity);
                    inter.IsInRange = true;

                    this.SendEvent(new EnterInteractAreaEvent
                    {
                        target = closestEntity
                    });

                    // 拿到挂在它身上的 OOP 组件引用
                    // var proxy = EntityManager.GetComponentData<InteractableProxy>(closestEntity);

                    // if (proxy.OOPComponent != null)
                    // {
                    //     Debug.Log($"[ECS] 玩家扫描到最近的物体：{proxy.OOPComponent.gameObject.name}，发起交互！");

                    //     // 4. 【高潮来了】：ECS 直接击穿次元壁，向 QFramework 发送指令！
                    //     LittleRPGArchitecture.Interface.SendEvent(new ECSInteractTriggerEvent
                    //     {
                    //         TargetOOPComponent = proxy.OOPComponent,
                    //         PlayerGO = playerAnimGO.RootGO // 传玩家的 3D 皮囊过去
                    //     });
                    // }
                }
            }
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;

    }
}