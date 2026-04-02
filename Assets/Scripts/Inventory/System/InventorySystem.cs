using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace LittleRPG
{
    public interface IInventorySystem : ISystem
    {
        void AddItem(int itemID, int count);
        bool RemoveItem(int itemID, int count);
        int GetItemCount(int itemID);
        void MoveItem(int mFromIndex, int mToIndex);
    }

    public class InventorySystem : AbstractSystem, IInventorySystem, ISaveHandler
    {
        /// <summary>
        /// 用于存档路径
        /// </summary>
        /// <value></value>
        public string SaveFileName { get; } = "inventory.es3";

        private IInventoryModel mInventoryModel;

        protected override void OnInit()
        {
            this.GetSystem<ISaveSystem>().RegisterSaveHandler(this);

            // 初始化背包Model
            mInventoryModel = this.GetModel<IInventoryModel>();
            // 监听事件
            this.RegisterEvent<InventoryItemChangedEvent>(OnInventoryItemChangedEvent);
        }

        /// <summary>
        /// 从一个槽位移动到另一个槽位
        /// </summary>
        /// <param name="mFromIndex"></param>
        /// <param name="mToIndex"></param>
        public void MoveItem(int fromIndex, int toIndex)
        {
            var model = this.GetModel<IInventoryModel>();
            model.PlayerItems.TryGetValue(fromIndex, out var fromData);
            model.PlayerItems.TryGetValue(toIndex, out var toData);
            if (fromData == null || fromData.IsEmpty) return; // 容错：拖了一个空的东西

            // 情况 1：目标是空的 -> 【直接移动】
            if (toData == null || toData.IsEmpty)
            {
                if (!model.PlayerItems.ContainsKey(toIndex))
                {
                    var newData = SlotData.Empty.CopyFromOther(fromData);
                    model.PlayerItems.Add(toIndex, newData);
                }
                else
                {
                    model.PlayerItems[toIndex].CopyFromOther(fromData);
                }
                model.PlayerItems[fromIndex].SetNone();

            }
            // 情况 2：目标有东西，且是同一种物品 -> 【尝试堆叠】
            else if (fromData.ItemID == toData.ItemID)
            {
                // 去配置表查这个物品的最大堆叠数
                int maxStack = this.GetModel<IItemTableModel>().ItemDic[fromData.ItemID].MaxStack;
                int spaceLeft = maxStack - toData.ItemCount;

                if (spaceLeft > 0)
                {
                    // 能塞多少塞多少
                    int amountToMove = Mathf.Min(spaceLeft, fromData.ItemCount);

                    // 修改目标数据
                    toData.ItemCount += amountToMove;

                    // 修改来源数据
                    fromData.ItemCount -= amountToMove;
                }
                else
                {
                    // 目标虽然是同类，但是满了，那就变成【交换】
                    SwapData(model, fromIndex, toIndex, fromData, toData);
                }
            }
            // 情况 3：目标有东西，且是不同物品 -> 【直接交换】
            else
            {
                SwapData(model, fromIndex, toIndex, fromData, toData);
            }

            // 发送事件通知 UI 哪两个格子交换了,用于刷新UI
            this.SendEvent(new InventorySlotChangedEvent { SlotIndex = fromIndex });
            this.SendEvent(new InventorySlotChangedEvent { SlotIndex = toIndex });

            // 发送事件通知 UI 哪两个格子交换了,用于UI动画
            this.SendEvent(new InventorySlotSwappedEvent { FromIndex = fromIndex, ToIndex = toIndex });
        }

        /// <summary>
        /// 辅助方法：交换数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fromIdx"></param>
        /// <param name="toIdx"></param>
        /// <param name="fromData"></param>
        /// <param name="toData"></param>
        private void SwapData(IInventoryModel model,
         int fromIdx,
         int toIdx,
         SlotData fromData,
         SlotData toData)
        {
            fromData.ToSwapData(toData);
        }

        private void OnInventoryItemChangedEvent(InventoryItemChangedEvent e)
        {
            // InventoryItemChangedEvent itemChangedEvent = e;
            // //找到已存在的同类物品或第一个空位，尝试堆叠
            // int firstEmptySlot = GetFirstSlotIndex(itemChangedEvent.ItemID);

            // if (firstEmptySlot < 0) return; // 无法添加

            // while(itemChangedEvent.ItemCount > 0)
            // {
            //     int slotIndex = GetFirstSlotIndex(itemChangedEvent.ItemID);
            //     if (slotIndex < 0) break; // 没有空位了，无法继续添加

            //     AddItemToSlot(slotIndex, itemChangedEvent.ItemID, itemChangedEvent.ItemCount);

            //     // 更新剩余数量
            //     int currentCountInSlot = mInventoryModel.PlayerItems[slotIndex].ItemCount;
            //     int maxStack = this.GetModel<IItemTableModel>().ItemDic[itemChangedEvent.ItemID].MaxStack;
            //     int spaceLeft = maxStack - currentCountInSlot;

            //     if (spaceLeft >= itemChangedEvent.ItemCount)
            //     {
            //         itemChangedEvent.ItemCount = 0; // 全部放进去了
            //     }
            //     else
            //     {
            //         itemChangedEvent.ItemCount -= spaceLeft; // 还有剩余，继续下一轮
            //     }
            // }

            // this.SendEvent(new InventorySlotChangedEvent { SlotIndex = toIndex });

            // Debug.Log($"InventorySystem 收到 InventoryItemChangedEvent：物品ID {e.ItemID} 数量 {e.ItemCount}");
        }
        /// <summary>
        /// 直接放入物体与指定数量
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="count"></param>
        public void AddItem(int itemID, int count)
        {
            if (count <= 0) return;

            if (mInventoryModel.PlayerItems.ContainsKey(itemID))
            {
                mInventoryModel.PlayerItems[itemID].ItemCount += count;
            }
            else
            {
                // mInventoryModel.PlayerItems.Add(itemID, count);
            }

            // 发送事件，通知 UI 这个物品变了
            this.SendEvent(new InventoryItemChangedEvent { ItemID = itemID });
            Debug.Log($"获得了物品 ID:{itemID}, 数量:{count}");
        }

        public bool RemoveItem(int itemID, int count)
        {
            if (!mInventoryModel.PlayerItems.ContainsKey(itemID)) return false;

            // int currentCount = mInventoryModel.PlayerItems[itemID];
            // if (currentCount < count) return false; // 数量不够扣除

            // mInventoryModel.PlayerItems[itemID].ItemCount -= count;

            // 如果扣完没东西了，从字典彻底移除
            // if (mInventoryModel.PlayerItems[itemID] == 0)
            // {
            //     mInventoryModel.PlayerItems.Remove(itemID);
            // }

            this.SendEvent(new InventoryItemChangedEvent { ItemID = itemID });
            return true;
        }

        /// <summary>
        /// 获取背包中的第一个指定物品的索引
        /// 找不到返回第一个空位的索引，如果连空位都没有了就返回 -1
        /// </summary>
        /// <returns></returns>
        public int GetFirstSlotIndex(int itemID)
        {
            int EmptySlotIndex = -1; // 默认-1表示没找到
            int capacity = mInventoryModel.Capacity.Value;
            var itemTableModel = this.GetModel<IItemTableModel>();

            for (int i = 0; i < mInventoryModel.Capacity.Value; i++)
            {
                // 这个槽没东西
                if (!mInventoryModel.PlayerItems.ContainsKey(i))
                {
                    if (EmptySlotIndex == -1) EmptySlotIndex = i; // 记录第一个空位的索引
                    continue;
                }
                if (mInventoryModel.PlayerItems[i].ItemID == itemID
                    && mInventoryModel.PlayerItems[i].ItemCount > 0
                    && itemTableModel.ItemDic[itemID].MaxStack > mInventoryModel.PlayerItems[i].ItemCount)
                {
                    return i;
                }

            }
            return EmptySlotIndex;
        }

        public int GetItemCount(int itemID)
        {
            return 0;
        }

        public void OnSave(ISaveUtility saveUtil, string folderPath)
        {
            // 自己决定文件名：folderPath + SaveFileName
            saveUtil.Save("Items", mInventoryModel.PlayerItems, folderPath + SaveFileName);
            saveUtil.Save("Capacity", mInventoryModel.Capacity.Value, folderPath + SaveFileName);
        }

        public void OnLoad(ISaveUtility saveUtil, string folderPath)
        {
            string filePath = folderPath + SaveFileName;
            if (saveUtil.HasFile(filePath))
            {
                var savedItems = saveUtil.Load("Items", new Dictionary<int, SlotData>(), filePath);
                int savedCapacity = saveUtil.Load("Capacity", 0, filePath);

                mInventoryModel.Capacity.Value = savedCapacity;
                mInventoryModel.PlayerItems.Clear();

                foreach (var kvp in savedItems)
                {
                    mInventoryModel.PlayerItems[kvp.Key] = kvp.Value;
                }

                // 自己读完数据，自己发事件通知 UI 刷新！
                this.SendEvent(new InventorySlotChangedEvent { SlotIndex = -1 });
            }
            else
            {
                this.NewSave(saveUtil, folderPath); // 没有存档，创建一个新的存档
            }
        }

        public void NewSave(ISaveUtility saveUtil, string folderPath)
        {
            mInventoryModel.PlayerItems[1] = new SlotData(0, 20);
            mInventoryModel.PlayerItems[2] = new SlotData(0, 60);
            mInventoryModel.PlayerItems[3] = new SlotData(1, 2);
            mInventoryModel.PlayerItems[10] = new SlotData(2, 3);
            mInventoryModel.Capacity.Value = 21;

            string filePath = folderPath + SaveFileName;

            if (saveUtil.HasFile(filePath))
            {
                saveUtil.DeleteFile(filePath);
            }

            // 3. 执行一次强行存档，把新创建的文件夹盖下去
            this.OnSave(saveUtil, folderPath);
        }

        public void OnDelete(ISaveUtility saveUtil, string folderPath)
        {
            string filePath = folderPath + SaveFileName;
            if (saveUtil.HasFile(filePath))
            {
                saveUtil.DeleteFile(filePath);
            }
        }
    }
}