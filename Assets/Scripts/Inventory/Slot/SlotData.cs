using System;
using QFramework;
using System.Collections.Generic;

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

        // 一个静态的空数据，方便清空格子时使用
        public static SlotData Empty => new SlotData(-1, 0);
    }
}