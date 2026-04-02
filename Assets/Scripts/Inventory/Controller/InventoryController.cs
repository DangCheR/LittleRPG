using UnityEngine;
using QFramework;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LittleRPG
{
    /// <summary>
    /// 背包控制器
    /// </summary>
    public class InventoryController : MonoBehaviour, IController
    {
        [Header("UI 配置")]
        private GameObject SlotPrefab; // 槽位预制件
        public Transform ContentParent; // 槽位的面板
        public Button InventoryBtn; // 打开背包按键
        public Button CloseInventory; // 关闭背包按键
        public Transform InventoryPanel; // 背包面板
        // 手下所有的傀儡格子
        private Dictionary<int, SlotUIView> mSlotViews = new Dictionary<int, SlotUIView>();
        private ITweenUtility tweenUtility;
        // 玩家Model
        private IInventoryModel mModel;

        private ResLoader mLoader; // 资源管家，负责加载物品图标等资源 

        //对照表 提供Sprite与Name
        private IItemTableModel itemTableModel;

        private async void Start()
        {
            //面板，初始关闭
            var canvas = GameObject.Find("MyCanvas").transform;

            InventoryPanel = canvas.Find("InvenroryPanel");
            InventoryPanel.gameObject.SetActive(false);

            //加载父级
            InventoryBtn = canvas.Find("InventoryBtn").GetComponent<Button>();
            ContentParent = InventoryPanel.Find("Context");
            CloseInventory = InventoryPanel.Find("CloseInventory").GetComponent<Button>();

            tweenUtility = this.GetUtility<ITweenUtility>();

            //添加打开背包事件
            InventoryBtn.onClick.AddListener(() =>
            {
                InventoryPanel.gameObject.SetActive(!InventoryPanel.gameObject.activeSelf);
            });

            //关闭背包事件
            CloseInventory.onClick.AddListener(() =>
            {
                InventoryPanel.gameObject.SetActive(false);
            });
            mLoader = new ResLoader();

            // 加载槽位预制件
            // 初始化所有Slot
            mLoader.LoadAssetAsyncWithCallback<GameObject>("Assets/Resources_moved/Prefabs/InventorySlot.prefab", (slotP) =>
            {
                if (slotP == null)
                {
                    Debug.LogWarning("槽位预制件没找到");
                }
                else
                {
                    SlotPrefab = slotP;
                    InitSlotViews();
                }
            });
            mModel = this.GetModel<IInventoryModel>();
            itemTableModel = this.GetModel<IItemTableModel>();

            // 监听底层数据变化
            this.RegisterEvent<InventorySlotChangedEvent>(OnSlotDropEvent).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<InventorySlotSwappedEvent>(OnSlotSwappedEvent).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 首次展示
            RefreshAllViews();
        }

        /// <summary>
        /// 初始化槽的数据
        /// </summary>
        private async void InitSlotViews()
        {
            for (int i = 0; i < mModel.Capacity.Value; i++)
            {
                var go = Instantiate(SlotPrefab, ContentParent);

                var view = go.GetComponent<SlotUIView>();

                view.Init(i); // 设置索引
                view.OnSlotClickEvent += HandleSlotClick;
                view.OnSlotDroppedEvent += HandleSlotDrop;
                view.OnDragFailedEvent += HandleDragFailed;
                view.OnSlotHoverEnterEvent += HandleSlotHoverEnter;
                view.OnSlotHoverExitEvent += HandleSlotHoverExit;

                //如果model中记录该槽存在数据
                if (mModel.PlayerItems.ContainsKey(i))
                {
                    SlotData slotData = mModel.PlayerItems[i];

                    ItemInfo itemInfo = await GetItemInfo(slotData.ItemID);
                    if (itemInfo != null)
                    {
                        view.UpdateIcon(itemInfo.SpriteIcon);
                        view.UpdateCount(slotData.ItemCount);
                    }
                    else
                    {
                        view.UpdateIcon();
                        view.UpdateCount();
                    }
                }
                //如果没有直接初始化为空
                else
                {
                    view.UpdateIcon();
                    view.UpdateCount();
                }

                mSlotViews.Add(i, view);
            }
        }

        // --- 控制傀儡显示逻辑 ---

        private void RefreshAllViews()
        {
            for (int i = 0; i < mModel.Capacity.Value; i++)
            {
                RefreshSlotView(i);
            }
        }

        /// <summary>
        /// 处理槽位被拖动事件，发送交换指令
        /// </summary>
        /// <param name="e"></param>
        private void OnSlotDropEvent(InventorySlotChangedEvent e)
        {
            RefreshSlotView(e.SlotIndex);
        }

        /// <summary>
        /// 处理格子交换事件
        /// </summary>
        /// <param name="e"></param>
        private void OnSlotSwappedEvent(InventorySlotSwappedEvent e)
        {
            Debug.Log($"收到格子交换事件：{e.FromIndex} <-> {e.ToIndex}");
            mSlotViews.TryGetValue(e.FromIndex, out var fromView);
            mSlotViews.TryGetValue(e.ToIndex, out var toView);
            fromView.PlayFlyBackAnimation(tweenUtility, toView.GetDraggingWorldPosition());
            toView.PlayFlyBackAnimation(tweenUtility, fromView.GetDraggingWorldPosition());
        }


        /// <summary>
        /// 更改UI层面
        /// </summary>
        /// <param name="index"></param>
        private void RefreshSlotView(int slotIndex)
        {
            if (!mSlotViews.ContainsKey(slotIndex)) return;

            var view = mSlotViews[slotIndex];

            if (mModel.PlayerItems.ContainsKey(slotIndex))
            {
                SlotData slotData = mModel.PlayerItems[slotIndex];
                ItemInfo itemInfo = itemTableModel.ItemDic.ContainsKey(slotData.ItemID) ? itemTableModel.ItemDic[slotData.ItemID] : null;

                if (itemInfo != null)
                {
                    view.UpdateIcon(itemInfo.SpriteIcon);
                    view.UpdateCount(slotData.ItemCount);
                }
                else
                {
                    Debug.LogWarning($"物品ID {slotData.ItemID} 在表格中没有对应的ItemInfo");
                    view.UpdateIcon();
                    view.UpdateCount();
                }
            }
            else
            {
                view.UpdateIcon();
                view.UpdateCount();
            }
        }

        // --- 处理傀儡上报的交互意图 ---

        private void HandleSlotClick(int slotIndex)
        {
            Debug.Log($"大脑收到：点击了格子 {slotIndex}");
            // 比如点击使用物品的 Command
            // this.SendCommand(new UseItemCommand(slotIndex));
        }

        /// <summary>
        /// 监听小弟的拖放事件，发送交换物品的 Command
        /// </summary>
        /// <param name="sourceIndex"></param>
        /// <param name="targetIndex"></param>
        private void HandleSlotDrop(int sourceIndex, int targetIndex)
        {
            this.SendCommand(new MoveItemCommand(sourceIndex, targetIndex));
        }

        /// <summary>
        /// 监听小弟的拖放失败事件，发送物品飞回去的 Command，或者丢地上的 Command
        /// </summary>
        /// <param name="draggedItem"></param>
        private void HandleDragFailed(SlotUIView slot)
        {
            slot.PlayFlyBackAnimation(tweenUtility);
        }

        private void HandleSlotHoverEnter(SlotUIView view)
        {
            view.PlayPushRightTop(tweenUtility);
            // 播放被挤开的动画
        }

        private void HandleSlotHoverExit(SlotUIView view)
        {
            // 播放被挤开的动画
            view.PlayRecover(tweenUtility);
        }

        /// <summary>
        /// 获取物体信息
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        private async Task<ItemInfo> GetItemInfo(int ItemID)
        {
            if (!itemTableModel.ItemDic.ContainsKey(ItemID))
                return null;

            if (itemTableModel.ItemDic[ItemID].SpriteIcon == null)
            {
                itemTableModel.ItemDic[ItemID].SpriteIcon = await mLoader.LoadAssetAsync<Sprite>(itemTableModel.ItemDic[ItemID].SpriteIconPath);
            }
            return itemTableModel.ItemDic[ItemID];
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}