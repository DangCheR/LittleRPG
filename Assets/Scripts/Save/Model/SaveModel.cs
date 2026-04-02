using System.Collections.Generic;
using QFramework;
using System;

namespace LittleRPG
{
    // 这个结构体用来在主菜单的存档槽位上显示信息
    [Serializable]
    public struct SaveSlotMeta
    {
        public int SlotID;
        public bool IsEmpty;
        public string SaveTime;   // 上次存档时间
        public string HasTime;   // 已经游玩时间
        public int PlayerLevel;   // 展示用等级
        // ... 其他你想在读档界面展示的信息
    }

    public interface ISaveModel : IModel
    {
        int CurrentSlotID { get; set; } // 玩家当前正在玩的档
        Dictionary<int, SaveSlotMeta> SlotMetas { get; }
    }

    public class SaveModel : AbstractModel, ISaveModel
    {
        public int CurrentSlotID { get; set; } = -1;
        public Dictionary<int, SaveSlotMeta> SlotMetas { get; private set; }

        protected override void OnInit()
        {
            var saveUtil = this.GetUtility<ISaveUtility>();

            SlotMetas = new Dictionary<int, SaveSlotMeta>();
            // for (int i = 0; i < 3; i++)
            // {
            //     // 这里我们先假装读了三个档位的 Meta 信息，实际应该从存储里读
            //     SlotMetas[i] = saveUtil.Load($"Meta_{i}", new SaveSlotMeta { IsEmpty = true }, slotFile);
            // }
        }
    }
}