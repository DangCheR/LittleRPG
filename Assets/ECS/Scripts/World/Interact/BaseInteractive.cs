using UnityEngine;
using QFramework;
using Unity.Entities;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 主动交互的基类（比如：上马、开箱子、对话...）
    /// 目前什么也不传，自己去ecs里拿玩家实体，修改状态就行了
    /// 如果当前与此交互，需要添加一个交互结束的脚本，比如下马
    /// </summary>
    public abstract class BaseInteractive : MonoBehaviour, IController
    {
        public Canvas TipCanvas { get; private set; }

        public Entity OwnerEntity = Entity.Null; // 我是哪个 ECS 实体的皮囊？

        public Entity playerEntity = Entity.Null; // 玩家的实体（交互时需要修改玩家的状态）

        public EntityManager entityManager { get; private set; }

        public bool IsInteracting { get; private set; } = false; // 是否正在交互中（持续性交互用）

        protected virtual void Awake()
        {
            TipCanvas = GetComponentInChildren<Canvas>(true);
            if (TipCanvas) TipCanvas.gameObject.SetActive(false);

            GetPlayerEntity(); // 尝试获取玩家实体，后续交互需要用到
        }

        protected virtual void Start()
        {
            // 1. 监听雷达锁定事件 (控制 UI)
            this.RegisterEvent<EnterInteractAreaEvent>(e =>
            {
                if (e.TargetEntity == this.OwnerEntity)
                {
                    Debug.Log($"[{gameObject.name}] 听到主世界广播，显示交互提示！");
                    ShowInteractionTip();
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<ExitInteractAreaEvent>(e =>
            {
                if (e.TargetEntity == this.OwnerEntity)
                {
                    HideInteractionTip();
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 2. 监听真实交互事件 (玩家按键了)
            this.RegisterEvent<InteractEvent>(OnInteractEvent).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        public void ShowInteractionTip()
        {
            TipCanvas.gameObject.SetActive(true);
        }

        public void HideInteractionTip()
        {
            TipCanvas.gameObject.SetActive(false);
        }

        /// <summary>
        /// 获取玩家entity，传来传去的太累了
        /// </summary>
        /// <returns></returns>
        public bool GetPlayerEntity()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return false;

            var entityManager = world.EntityManager;
            this.entityManager = entityManager;

            // 创建查询并查找 Singleton
            var query = entityManager.CreateEntityQuery(typeof(PlayerInputData));

            // 确保查询到玩家实体
            if (!query.TryGetSingletonEntity<PlayerInputData>(out var _playerEntity))
            {
                Debug.LogError("没有找到玩家实体！");
                return false;
            }
            playerEntity = _playerEntity;

            return true;
        }

        public void OnInteractEvent(InteractEvent e)
        {
            if (e.TargetEntity != this.OwnerEntity) return;

            if (IsInteracting)
            {
                // 已经在交互了，再次交互就结束
                ExecuteEndInteract();
                IsInteracting = false;
                UptateEcsState(false); // 告诉 ECS 世界我结束交互了
            }
            else
            {
                // 没有在交互，执行交互逻辑
                IsInteracting = ExecuteInteract();

                HideInteractionTip();
                if (IsInteracting)
                {
                    UptateEcsState(true); // 告诉 ECS 世界开始持久化交互了
                }
            }
        }

        /// <summary>
        /// 通知ECS世界是否正在交互
        /// </summary>
        /// <param name="isInteracting"></param>
        public void UptateEcsState(bool isInteracting)
        {
            if (playerEntity == Entity.Null)
            {
                if (!GetPlayerEntity())
                {
                    Debug.LogError("玩家实体未找到，无法更新交互状态！");
                    return;
                }
            }

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var entityManager = world.EntityManager;

            if (!entityManager.HasComponent<Interactor>(playerEntity))
            {
                Debug.LogError("玩家实体没有 Interactor 组件，无法进行交互！");
                return;
            }

            var Interactor = entityManager.GetComponentData<Interactor>(playerEntity);

            Interactor.CurrentInteractiveTarget = isInteracting ? OwnerEntity : Entity.Null;

            entityManager.SetComponentData(playerEntity, Interactor);
        }
        
        
        /// <summary>
        /// 执行交互的逻辑，
        /// 持续性交互返回 true，瞬时性交互返回 false
        /// </summary>
        /// <returns></returns>
        public abstract bool ExecuteInteract();

        /// <summary>
        /// 持续性第二次交互的逻辑
        /// 比如：上马后再次交互就是下马
        /// 瞬时交互不必理会
        /// </summary>
        public virtual void ExecuteEndInteract() { }


        void OnDestroy()
        {
            playerEntity = Entity.Null;
            OwnerEntity = Entity.Null;
            entityManager = default;
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}