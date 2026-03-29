namespace LittleRPG
{
    /// <summary>
    /// 背包Model修改事件，针对具体Item
    /// 应由世界事件发送
    /// 不直接操作UI的事件，例如拾取3 * Item_1
    /// </summary>
    public struct InventoryItemChangedEvent
    {
        public int ItemID; // 捡了什么
        public int ItemCount; // 捡了多少个
    }

    /// <summary>
    /// 背包Model修改事件，
    /// 针对具体Slot
    /// 可能用不上
    /// </summary>
    public struct InventorySlotChangedEvent
    {
        public int SlotIndex; // 哪个格子变了？(-1 代表需要全部刷新)
    }

    // 背包添加事件
    public struct InventoryItemAddEvent
    {
        SlotData inventoryModelData;
    }



    /// <summary>
    /// UI层面事件
    /// 直接操作背包UI拖动
    /// </summary>
    public struct SlotChangeEvent
    {
        public int FromIndex;
        public int ToIndex;

        // public InventoryData FromInventory;
        // public InventoryData ToInventory;

        public SlotChangeEvent(int fromIndex, int toIndex)
        {
            // FromInventory = fromInv;
            FromIndex = fromIndex;
            // ToInventory = toInv;
            ToIndex = toIndex;
        }
    }

    /// <summary>
    /// 请求打开装备面板事件
    /// 可能是打开了宝箱，需传入宝箱的Model
    /// </summary>
    public struct OpenInventoryEvent
    {
        public IInventoryModel InventoryModel; // 哪个格子变了？(-1 代表需要全部刷新)
    }
}
