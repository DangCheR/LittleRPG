using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

namespace LittleRPG.Physics
{
    public struct RandomSpawner : IComponentData
    {
        public Entity Prefab;        // 要生成的物体
        public float2 point;        // 生成的中心点
        public float radius;         // 半径
        public int SpawnCount;       // 这一波生成的数量
        public uint Seed;            // 随机种子
    }
}