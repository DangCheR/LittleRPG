using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LittleRPG.Physics
{
    public partial struct RandomSpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // 使用 BeginSimulation 的 ECB，确保在物理和渲染系统运行前实体已经实例化完成
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // 遍历所有带有 RandomSpawner 的生成器实体
            foreach (var (spawner, entity) in SystemAPI.Query<RefRW<RandomSpawner>>().WithEntityAccess())
            {
                // 初始化随机发生器。注意：种子不能为0
                var random = new Unity.Mathematics.Random(math.max(1, spawner.ValueRO.Seed));


                for (int i = 0; i < spawner.ValueRO.SpawnCount; i++)
                {
                    var templateShape = state.EntityManager.GetComponentData<ShapeData>(spawner.ValueRO.Prefab);

                    // 1. 实例化预制体
                    Entity newEntity = ecb.Instantiate(spawner.ValueRO.Prefab);

                    // 2. 核心算法：圆形内均匀分布
                    float angle = random.NextFloat(0, math.PI * 2);
                    // 使用平方根修正半径，防止物体向圆心堆积
                    float r = spawner.ValueRO.radius * math.sqrt(random.NextFloat(0f, 1f));

                    float x = spawner.ValueRO.point.x + r * math.cos(angle);
                    float z = spawner.ValueRO.point.y + r * math.sin(angle);

                    // 3. 设置 LocalTransform (渲染层位置)
                    // 假设你的地面高度在 Y = 0，或者保持和生成器一致
                    ecb.SetComponent(newEntity, LocalTransform.FromPosition(new float3(x, 0, z)));

                    // 4. 【重要】初始化物理数据 (ShapeData)
                    // 必须手动同步物理坐标，否则会出现“模型在 A，物理盒在 B”的问题
                    // 他妈的生成的时候不要Get
                    // var shapeData = SystemAPI.GetComponent<ShapeData>(newEntity);

                    templateShape.Position = new float2(x, z);
                    templateShape.PreviousPosition = new float2(x, z);

                    ecb.SetComponent(newEntity, templateShape);
                }

                // 更新种子，防止下一波生成时位置重叠
                spawner.ValueRW.Seed = random.NextUInt();

                // 生成完成后销毁生成器实体，避免每帧不停地刷怪
                ecb.DestroyEntity(entity);
            }
        }
    }
}