using QFramework;
using UnityEngine;

namespace LittleRPG
{
    public interface IInventorySystem : ISystem
    {
        void AddItem(int itemID, int count);
        bool RemoveItem(int itemID, int count);
        int GetItemCount(int itemID);
    }

    public class InventorySystem : AbstractSystem, IInventorySystem
    {
        private IInventoryModel mInventoryModel;

        protected override void OnInit()
        {
            // 初始化背包Model
            mInventoryModel = this.GetModel<IInventoryModel>();
        }

        //交换两个槽的物品
        public void SwapItems(int from, int to)
        {
            if (!mInventoryModel.PlayerItems.ContainsKey(to)) return;
            if (mInventoryModel.PlayerItems[to].ItemCount == 0)
            {
                SlotData toData = mInventoryModel.PlayerItems[from];
                mInventoryModel.PlayerItems[from] = mInventoryModel.PlayerItems[to];
                mInventoryModel.PlayerItems[to] = toData;
            }
            else
            {
                mInventoryModel.PlayerItems[to] = mInventoryModel.PlayerItems[from];
                mInventoryModel.PlayerItems[from] = new (0,0);
            }
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

        public int GetItemCount(int itemID)
        {
            return 0;
            // return mInventoryModel.PlayerItems.TryGetValue(itemID, out int count) ? count : 0;
        }
    }
}