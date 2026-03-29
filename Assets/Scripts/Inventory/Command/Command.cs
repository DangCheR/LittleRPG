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
        private int mFromIndex;
        private int mToIndex;

        public SwapItemCommand(int fromIndex, int toIndex)
        {
            mFromIndex = fromIndex;
            mToIndex = toIndex;
        }

        protected override void OnInventoryExecute()
        {
            // mInventorySystem.SwapItems(mFromIndex, mToIndex);
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

    public class AddInventoryCapacity : InventoryCommand
    {
        public int AddCapacity;
        protected override void OnInventoryExecute()
        {
            mInventoryModel.Capacity.Value += AddCapacity;
            
        }
    }
}