using QFramework;
using System;

namespace LittleRPG
{
    public struct SaveModelInitEvent
    {
        // 这个事件目前不携带任何数据，但你可以根据需要添加一些字段
        // 比如 public int TotalSlots; 来告诉外界总共有多少个存档槽位
    }
    public struct SaveMetaUpdatedEvent
    {
        public int SlotID; // 哪个槽位的 Meta 被更新了
        public SaveSlotMeta Meta; // 更新后的 Meta 数据
    }
    public struct SaveSlotDeletedEvent
    {
        public int SlotID; // 哪个槽位被删除了
    }
}