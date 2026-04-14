using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LittleRPG.FlowFields
{
    /// <summary>
    /// 整体地图的 Authoring 组件，挂在一个空 GameObject 上，记录地图坐标和尺寸
    /// </summary>
    public class MapAuthoring : MonoBehaviour
    {
        // 地图坐标，记录地图在世界中的位置
        public int2 MapCoordinate;
        public int2 MapSize;

        class MapBaker : Baker<MapAuthoring>
        {
            public override void Bake(MapAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MapSector { Coordinate = authoring.MapCoordinate });
                AddComponent(entity, new MapSize { Value = authoring.MapSize });
            }
        }
    }
}