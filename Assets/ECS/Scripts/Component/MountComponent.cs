using Unity.Entities;
namespace LittleRPG.Combat
{
    // 这个组件是个标签，挂在玩家身上，表示玩家可以骑乘
    public struct RiderTag : IComponentData
    {
        public Entity MountEntity; // 记录正在骑乘的坐骑实体
    }

    // 这个组件是个标签，挂在玩家身上，表示玩家正在骑乘状态
    public struct MountTag : IComponentData
    {
        public Entity RiderEntity; // 记录正在骑乘的骑手实体
    }
}