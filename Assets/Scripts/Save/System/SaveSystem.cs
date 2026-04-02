using UnityEngine;
using QFramework;
using System;
using System.Collections.Generic;

namespace LittleRPG
{
    public interface ISaveSystem : ISystem
    {
        void LoadGlobalMeta(); // 主菜单调用：加载所有的存档信息
        void SaveGame();       // 游戏中调用：保存当前游戏
        void NewGame();        // 主菜单调用：新建档位，进入游戏
        void LoadGame(int slotID); // 主菜单调用：读取某个档并进入游戏
        void DeleteGame(int slotID); // 主菜单调用：删除某个档
        public void RegisterSaveHandler(ISaveHandler handler); // 存档系统需要知道有哪些模块需要被存档，所以它提供一个接口让模块来注册自己
    }

    public class SaveSystem : AbstractSystem, ISaveSystem
    {
        private string GetSlotFolder(int slotID) => $"SaveSlot_{slotID}/";

        private string GetIndexFile(int slotID) => $"{GetSlotFolder(slotID)}index.es3";

        private ISaveModel mSaveModel; // 主存档Index
        private ISaveUtility mSaveUtil;

        private List<ISaveHandler> mSaveHandlers = new List<ISaveHandler>(); // 存档处理器列表

        protected override void OnInit()
        {
            LoadGlobalMeta();
        }

        /// <summary>
        /// 存档系统需要知道有哪些模块需要被存档，所以它提供一个接口让模块来注册自己
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterSaveHandler(ISaveHandler handler)
        {
            if (!mSaveHandlers.Contains(handler))
            {
                mSaveHandlers.Add(handler);
            }
        }


        // --- 1. 主菜单：读取存档列表 ---
        public void LoadGlobalMeta()
        {
            mSaveUtil = this.GetUtility<ISaveUtility>();
            mSaveModel = this.GetModel<ISaveModel>();

            for (int i = 0; i < 3; i++)
            {
                string slotFile = GetIndexFile(i);
                if (mSaveUtil.HasFile(slotFile))
                {
                    // 如果有这个档，从全局文件里把它的 Meta 信息读出来
                    mSaveModel.SlotMetas[i] = mSaveUtil.Load($"Meta", new SaveSlotMeta { IsEmpty = true }, slotFile);
                }
                else
                {
                    // 没有这个档，说明它是空的，我们就塞一个默认的 Meta 进去
                    mSaveModel.SlotMetas[i] = new SaveSlotMeta { SlotID = i, IsEmpty = true };
                }
            }
            // 读完之后，主菜单 UI 就可以刷新存档列表了！
        }

        /// <summary>
        /// 新建档位：在主菜单点击“新游戏”时调用。它会：
        /// </summary>
        public void NewGame()
        {
            // 初始化Index文件
            var meta = new SaveSlotMeta
            {
                SlotID = mSaveModel.CurrentSlotID,
                IsEmpty = false,
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                PlayerLevel = 1 // TODO: 从 PlayerModel 取
            };
            mSaveUtil.Save("Meta", meta, GetIndexFile(mSaveModel.CurrentSlotID));

            foreach (var handler in mSaveHandlers)
            {
                handler.NewSave(mSaveUtil, GetSlotFolder(mSaveModel.CurrentSlotID));
            }
            this.SendEvent(new SaveSlotDeletedEvent { SlotID = mSaveModel.CurrentSlotID }); // 发送事件通知 UI 刷新

        }

        // --- 2. 游戏中：保存当前进度 ---
        public void SaveGame()
        {
            // 1. 先让各个模块把自己的数据保存到对应的文件里
            foreach (var handler in mSaveHandlers)
            {
                handler.OnSave(mSaveUtil, GetSlotFolder(mSaveModel.CurrentSlotID));
            }

            // 2. 更新一下全局的 Meta 信息（比如上次存档时间、玩家等级等），让它在主菜单里能被看到
            var meta = new SaveSlotMeta
            {
                SlotID = mSaveModel.CurrentSlotID,
                IsEmpty = false,
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                PlayerLevel = 1 // TODO: 从 PlayerModel 取
            };
            mSaveUtil.Save("Meta", meta, GetIndexFile(mSaveModel.CurrentSlotID));
            this.SendEvent(new SaveSlotDeletedEvent { SlotID = mSaveModel.CurrentSlotID }); // 发送事件通知 UI 刷新
        }

        // --- 3. 主菜单/游戏中：读取进度 ---
        public void LoadGame(int slotID)
        {
            mSaveModel.CurrentSlotID = slotID; // 先把当前档位ID记下来，等各个模块的 OnLoad 被调用时，它们就知道要读哪个档了

            // 1. 先让各个模块把自己的数据从对应的文件里读出来，覆盖掉当前内存里的数据
            foreach (var handler in mSaveHandlers)
            {
                handler.OnLoad(mSaveUtil, GetSlotFolder(slotID));
            }


            // this.SendEvent(new LoadSceneEvent { SceneName = "BeginnerVillage" });
        }

        public void DeleteGame(int slotID)
        {
            // 1. 调用各个模块的删除方法
            foreach (var handler in mSaveHandlers)
            {
                handler.OnDelete(mSaveUtil, GetSlotFolder(slotID));
            }

            // 2. 删除索引文件
            string indexFile = GetIndexFile(slotID);
            if (mSaveUtil.HasFile(indexFile))
            {
                mSaveUtil.DeleteFile(indexFile);
            }

            this.GetModel<ISaveModel>().SlotMetas[slotID] = new SaveSlotMeta { SlotID = slotID, IsEmpty = true }; // 更新 Model 里的 Meta 信息
            // 发送事件通知 UI 刷新
            this.SendEvent(new SaveSlotDeletedEvent { SlotID = slotID });
        }
    }
}