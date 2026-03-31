using UnityEngine;
using QFramework;
using System.Collections.Generic;
using UnityEngine.UI;
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
        private List<SlotUIView> mSlotViews = new List<SlotUIView>();
        private ITweenUtility tweenUtility;
        // 玩家Model
        private IInventoryModel mModel;

        //对照表 提供Sprite与Name
        private IItemTableModel itemTableModel;

        private void Start()
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

            //加载槽位预制件
            SlotPrefab = Resources.Load<GameObject>("Prefabs/InventorySlot");

            mModel = this.GetModel<IInventoryModel>();
            itemTableModel = this.GetModel<IItemTableModel>();

            // 初始化所有Slot
            InitSlotViews();

            // 监听底层数据变化
            this.RegisterEvent<EndDragEvent>(OnEndDragEvent).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<InventorySlotChangedEvent>(OnSlotDropEvent).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 3. 首次展示
            RefreshAllViews();
        }

        /// <summary>
        /// 初始化槽的数据
        /// </summary>
        private void InitSlotViews()
        {
            foreach (var kpv in mModel.PlayerItems)
            {
                Debug.Log($"{kpv.Key}:{kpv.Value}");
            }

            for (int i = 0; i < mModel.Capacity.Value; i++)
            {
                var go = Instantiate(SlotPrefab, ContentParent);

                var view = go.GetComponent<SlotUIView>();

                view.Init(i); // 设置索引
                view.OnSlotClickEvent += HandleSlotClick;
                view.OnSlotDropEvent += HandleSlotDrop;
                view.OnDragFailedEvent += HandleDragFailed;

                //如果model中记录该槽存在数据
                if (mModel.PlayerItems.ContainsKey(i))
                {
                    SlotData slotData = mModel.PlayerItems[i];

                    ItemInfo itemInfo = GetItemInfo(slotData.ItemID);
                    if (itemInfo != null)
                    {
                        view.UpdateIcon(itemInfo.SpriteIcon);
                        view.UpdateCount(slotData.ItemCount);
                    }
                }
                //如果没有直接初始化为空
                else
                {
                    view.UpdateIcon();
                    view.UpdateCount();
                }

                mSlotViews.Add(view);
            }
        }

        // --- 控制傀儡显示逻辑 ---

        private void RefreshAllViews()
        {

        }

        /// <summary>
        /// 处理拖动结束事件
        /// </summary>
        /// <param name="e"></param>
        private void OnEndDragEvent(EndDragEvent e)
        {
            e.eventData.pointerEnter.gameObject.TryGetComponent<SlotUIView>(out var targetSlot);
            e.eventData.pointerDrag.gameObject.TryGetComponent<SlotUIView>(out var sourceSlot);
            sourceSlot.DraggingToSlot();
            if (targetSlot == null)
            {
                Debug.Log("拖到了无效区域，物品归位");
                return;
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
        /// 更改UI层面
        /// </summary>
        /// <param name="index"></param>
        private void RefreshSlotView(int slotIndex)
        {
            var slot = mSlotViews[slotIndex];

            mModel.PlayerItems.TryGetValue(slot.SlotIndex, out var slotData);

            Debug.Log($"刷新了格子 {slotIndex}，数据是 {slotData.ItemID}:{slotData.ItemCount}");
            if (slotData.IsEmpty)
            {
                slot.UpdateIcon();
                slot.UpdateCount();
            }
            else
            {
                var itemInfo = GetItemInfo(slotData.ItemID);
                if (itemInfo != null)
                {
                    slot.UpdateIcon(itemInfo.SpriteIcon);
                    slot.UpdateCount(slotData.ItemCount);
                }
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

        /// <summary>
        /// 获取物体信息
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        private ItemInfo GetItemInfo(int ItemID)
        {
            if (itemTableModel.ItemDic.ContainsKey(ItemID))
            {
                return itemTableModel.ItemDic[ItemID];
            }
            return null;
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}