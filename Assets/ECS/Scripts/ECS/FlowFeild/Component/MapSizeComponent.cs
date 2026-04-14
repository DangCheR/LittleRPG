using Unity.Entities;
using Unity.Mathematics;

namespace LittleRPG.FlowFields
{
    /// <summary>
    /// 记录地图尺寸的组件，挂在一个 Entity 上，记录地图的宽高信息
    /// </summary>
    public struct MapSize : IComponentData
    {
        public int2 Value;
    }
    
    public struct MapSector:IComponentData
    {
        public int2 Coordinate;
    }
}