using UnityEngine;
using QFramework;
using Unity.Entities;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 玩家的交互控制器
    /// 缓存交互目标并监听交互事件
    /// </summary>
    public class PlayerInteract : MonoBehaviour, ICanSendEvent, ICanRegisterEvent
    {
        private Entity playerEntity = Entity.Null; // 玩家的 ECS 实体
        private Entity targetEntity = Entity.Null; // 交互目标的 ECS 实体
        private Entity InteractingEntity = Entity.Null; // 当前正在交互的实体（如果有的话）

        private void Awake()
        {
            this.RegisterEvent<EnterInteractAreaEvent>(e =>
            {
                targetEntity = e.TargetEntity;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<ExitInteractAreaEvent>(e =>
            {
                targetEntity = Entity.Null;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<PlayerPressInteractEvent>(OnInteract).UnRegisterWhenGameObjectDestroyed(gameObject);
            // StartRiding(seat, lf, rf);
        }

        public void OnInteract(PlayerPressInteractEvent e)
        {
            if (targetEntity == Entity.Null)
            {
                Debug.Log("没有交互目标，无法交互！");
                return;
            }

            this.SendEvent(new InteractEvent
            {
                TargetEntity = targetEntity
            });
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;

    }
}