using QFramework;

namespace LittleRPG
{
    /// <summary>
    /// 直接操作Model的方法放在Command中
    /// </summary>
    public abstract class InventoryCommand : AbstractCommand
    {
        // 缓存引用
        protected IInventorySystem mInventorySystem;
        protected IInventoryModel mInventoryModel;

        protected override void OnExecute()
        {
            mInventorySystem = this.GetSystem<IInventorySystem>();
            mInventoryModel = this.GetModel<IInventoryModel>();
            OnInventoryExecute();
        }

        protected abstract void OnInventoryExecute();
    }

    /// <summary>
    /// 具体指令1：交换物品
    /// </summary>
    public class SwapItemCommand : InventoryCommand
    {
        private SlotUIView sourceSlot;
        private SlotUIView targetSlot;

        public SwapItemCommand(SlotUIView fromSlot, SlotUIView toSlot)
        {
            sourceSlot = fromSlot;
            targetSlot = toSlot;
        }

        protected override void OnInventoryExecute()
        {
            if (sourceSlot == null || targetSlot == null)
            {
                // 可以发送全局 UI 弹窗事件，提示拖动无效
                // this.SendEvent<UIMessageEvent>(new UIMessageEvent("无效的拖动！"));
                return;
            }
            // mInventorySystem.SwapItems(mFromIndex, mToIndex);
        }
    }

    public class MoveItemCommand : AbstractCommand
    {
        private int mFromIndex;
        private int mToIndex;

        public MoveItemCommand(int from, int to)
        {
            mFromIndex = from;
            mToIndex = to;
        }

        protected override void OnExecute()
        {
            // 把脏活累活交给 System 这个大管家
            this.GetSystem<IInventorySystem>().MoveItem(mFromIndex, mToIndex);
        }
    }

    /// <summary>
    /// 具体指令2：捡起物品
    /// </summary>
    public class AddItemCommand : InventoryCommand
    {
        private ItemInfo mItem;
        private int mCount;

        public AddItemCommand(ItemInfo item, int count)
        {
            mItem = item;
            mCount = count;
        }

        protected override void OnInventoryExecute()
        {
            // if (mInventorySystem.CanAddItem(mItem, mCount))
            // {
            //     mInventorySystem.AddItemToModel(mItem, mCount);
            // }
            // else
            // {
            //     // 可以发送全局 UI 弹窗事件，提示背包已满
            //     // this.SendEvent<UIMessageEvent>(new UIMessageEvent("背包已满！"));
            // }
        }
    }
    public class UpdateModelCommand : InventoryCommand
    {
        private int SlotIndex;
        private int ItemID;
        private int ItemCount;

        public UpdateModelCommand(int slotIndex, int itemID = -1, int itemCount = 0)
        {
            SlotIndex = slotIndex;
            ItemID = itemID;
            ItemCount = itemCount;
        }

        protected override void OnInventoryExecute()
        {
            // mInventorySystem.UpdateModel(SlotIndex, ItemID, ItemCount);
        }
    }
    public class AddInventoryCapacity : InventoryCommand
    {
        public int AddCapacity;
        protected override void OnInventoryExecute()
        {
            mInventoryModel.Capacity.Value += AddCapacity;

        }
    }
}