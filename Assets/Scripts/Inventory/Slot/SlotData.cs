using System;
using QFramework;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace LittleRPG
{
    /// <summary>
    /// 基础背包槽位字段
    /// 后续可能添加耐久
    /// </summary>
    public class SlotData
    {
        public int ItemID;
        public int ItemCount;

        // 方便判断这个格子是不是空的
        public bool IsEmpty => ItemID < 0 || ItemCount <= 0;

        // 构造函数
        public SlotData(int id, int count)
        {
            ItemID = id;
            ItemCount = count;
        }

        public void ToSwapData(SlotData other)
        {
            int fromID = ItemID;
            int fromCount = ItemCount;

            ItemID = other.ItemID;
            ItemCount = other.ItemCount;

            other.ItemID = fromID;
            other.ItemCount = fromCount;
        }
        public SlotData CopyFromOther(SlotData other)
        {
            ItemID = other.ItemID;
            ItemCount = other.ItemCount;
            return this;
        }

        // 一个静态的空数据，方便清空格子时使用
        public static SlotData Empty => new SlotData(-1, 0);
        public void SetNone()
        {
            ItemID = -1;
            ItemCount = 0;
        }
    }
}