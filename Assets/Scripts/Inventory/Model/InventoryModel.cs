using System;
using QFramework;
using System.Collections.Generic;

namespace LittleRPG
{
    /// <summary>
    /// 背包model，
    /// Capacity：槽位数量
    /// 维护一个槽位index与数据
    /// </summary>
    public interface IInventoryModel : IModel
    {
        BindableProperty<int> Capacity { get; }
        // 核心数据：Key=槽位index, Value=总数量
        Dictionary<int, SlotData> PlayerItems { get; set;}
    }

    public class InventoryModel : AbstractModel, IInventoryModel
    {
        public BindableProperty<int> Capacity { get; set; }
        public Dictionary<int, SlotData> PlayerItems { get; set; }

        protected override void OnInit()
        {
            //假装这里在读取存储
            PlayerItems = new Dictionary<int, SlotData>();
            Capacity = new BindableProperty<int>();
        }
    }
}