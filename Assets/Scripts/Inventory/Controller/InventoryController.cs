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

        // 玩家Model
        private IInventoryModel mModel;

        //对照表 提供Sprite与Name
        private ItemTableModel itemTableModel;

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
            if (SlotPrefab == null)
            {
                Debug.Log("槽位的预制件加载失败");
            }
            mModel = this.GetModel<IInventoryModel>();
            itemTableModel = this.GetModel<ItemTableModel>();

            // 1. 初始化所有傀儡
            InitSlotViews();

            // 2. 作为唯一大脑，监听底层数据变化
            this.RegisterEvent<InventoryItemChangedEvent>(e =>
            {
                Debug.Log($"主角捡到了{e.ItemCount}个{e.ItemID}");
            }).UnRegisterWhenGameObjectDestroyed(gameObject);


            this.RegisterEvent<SlotChangeEvent>(e =>
            {
                Debug.Log("槽位事件");
                // this.SendCommand<>
            });

            // 3. 首次展示
            RefreshAllViews();
        }

        /// <summary>
        /// 初始化槽的数据
        /// </summary>
        private void InitSlotViews()
        {
            foreach(var kpv in mModel.PlayerItems)
            {
                Debug.Log($"{kpv.Key}:{kpv.Value}");
            }

            for (int i = 0; i < mModel.Capacity.Value; i++)
            {
                var go = Instantiate(SlotPrefab, ContentParent);

                var view = go.GetComponent<SlotUIView>();

                view.Init(i); // 设置索引

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
                    Debug.Log($"{i}是无数据的");
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
        /// 更改UI层面
        /// </summary>
        /// <param name="index"></param>
        private void RefreshSlotView(SlotUIView slot)
        {

        }

        // --- 处理傀儡上报的交互意图 ---

        private void HandleSlotClick(int slotIndex)
        {
            Debug.Log($"大脑收到：点击了格子 {slotIndex}");
            // 比如点击使用物品的 Command
            // this.SendCommand(new UseItemCommand(slotIndex));
        }

        private void HandleSlotDrop(int sourceIndex, int targetIndex)
        {
            Debug.Log($"大脑收到：玩家把 {sourceIndex} 拖到了 {targetIndex}");

            // 大脑直接向底层系统下发“交换”指令！
            this.SendCommand(new SwapItemCommand(sourceIndex, targetIndex));
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