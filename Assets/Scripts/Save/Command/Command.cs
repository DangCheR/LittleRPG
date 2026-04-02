using QFramework;
using UnityEngine;

namespace LittleRPG
{
    // 读取游戏的指令
    public class LoadGameCommand : AbstractCommand
    {
        private int mSlotID;
        public LoadGameCommand(int slotID) => mSlotID = slotID;

        protected override void OnExecute()
        {
            Debug.Log($"[Command] 开始读取存档 {mSlotID}...");
            this.GetSystem<ISaveSystem>().LoadGame(mSlotID);

            // TODO: 在这里可以发送事件去关闭主菜单 UI，或者加载战斗场景
            // this.SendEvent(new LoadSceneEvent { SceneName = "MainCity" });
        }
    }

    // 创建新游戏的指令
    public class StartNewGameCommand : AbstractCommand
    {
        private int mSlotID;
        public StartNewGameCommand(int slotID) => mSlotID = slotID;

        protected override void OnExecute()
        {
            Debug.Log($"[Command] 在槽位 {mSlotID} 创建新游戏...");

            // 1. 设置当前游戏槽位
            this.GetModel<ISaveModel>().CurrentSlotID = mSlotID;

            // 2. 初始化全新的底层数据 (清空背包、重置金币等)
            // this.GetSystem<IInventorySystem>().ResetData();
            // this.GetSystem<IEconomySystem>().ResetData();

            // 3. 执行一次强行存档，把新创建的文件夹盖下去
            this.GetSystem<ISaveSystem>().NewGame();

            // 4. 进入游戏场景
            // this.SendEvent(new LoadSceneEvent { SceneName = "BeginnerVillage" });
        }
    }

    public class SaveGameCommand : AbstractCommand
    {
        private int mSlotID;
        public SaveGameCommand(int slotID) => mSlotID = slotID;

        protected override void OnExecute()
        {
            Debug.Log($"[Command] 在槽位 {mSlotID} 保存游戏...");

            // 1. 设置当前游戏槽位
            this.GetModel<ISaveModel>().CurrentSlotID = mSlotID;

            // 2. 初始化全新的底层数据 (清空背包、重置金币等)
            // this.GetSystem<IInventorySystem>().ResetData();
            // this.GetSystem<IEconomySystem>().ResetData();

            // 3. 执行一次强行存档，把新创建的文件夹盖下去
            this.GetSystem<ISaveSystem>().SaveGame();

            // 4. 进入游戏场景
            // this.SendEvent(new LoadSceneEvent { SceneName = "BeginnerVillage" });
        }
    }
    public class DeleteGameCommand : AbstractCommand
    {
        private int mSlotID;
        public DeleteGameCommand(int slotID) => mSlotID = slotID;

        protected override void OnExecute()
        {
            Debug.Log($"[Command] 删除存档 {mSlotID}...");

            // 1. 调用存档系统的删除方法
            this.GetSystem<ISaveSystem>().DeleteGame(mSlotID);

            // 2. 发送事件通知 UI 刷新（如果需要的话）
            // this.SendEvent(new SaveSlotDeletedEvent { SlotID = mSlotID });
        }
    }
}