using System.Numerics;
using LittleRPG.Combat;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LittleRPG.Physics
{
    // 执行在物理应用之前
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ApplyPhysicsSystem))] 
    public partial struct BoidSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. 获取大哥 (Gwen/Player) 的坐标，用来带头冲锋
            float3 leaderPos = float3.zero;
            if (SystemAPI.TryGetSingleton<PlayerInputData>(out var player))
            {
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerInputData>();
                leaderPos = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
            }

            // 2. 获取你构建好的 BVH 树 (如果是 Unity 自带的，就是 PhysicsWorldSingleton)
            // 这里假设你用自定义的 BVH，或者把所有 Boid 的位置写进了一个可查询的数据结构中
            // var bvhTree = SystemAPI.GetSingleton<MyBVHTree>(); 

            // 3. 开启多线程并行处理每个小弟的大脑
            new BoidBrainJob
            {
                LeaderPosition = new float2(leaderPos.x, leaderPos.z),
                // BVHTree = bvhTree
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct BoidBrainJob : IJobEntity
    {
        public float2 LeaderPosition;
        // [ReadOnly] public MyBVHTree BVHTree; // 你亲手做的 BVH 树！

        // 【多线程并发】：处理每一个小弟
        void Execute(ref PhyBodyData phyBody, in BoidBrain brain, in LocalTransform transform)
        {
            // 转换为2D
            float2 myPos = new float2(transform.Position.x, transform.Position.z);

            // 获取当前速度
            float2 myVel = phyBody.Velocity;

            // 分离，对齐，集合
            float2 separationForce = float2.zero;
            float2 alignmentForce = float2.zero;
            float2 cohesionCenter = float2.zero;

            // 获取邻居
            int neighborCount = 0;

            // ==========================================
            // 💥 天帝级优化：BVH 树查询代替双重 for 循环！
            // ==========================================
            
            // 1. 使用你的 BVH 树，进行 O(log N) 的球形重叠测试 (Sphere Overlap)
            // 找出我(myPos)周围 ViewRadius 范围内的邻居！
            // var neighbors = BVHTree.OverlapSphere(myPos, brain.ViewRadius);
            
            // 为了演示，假设 neighbors 是通过 BVH 返回的一个列表 (里面存了邻居的坐标和速度)
            /* 
            for (int i = 0; i < neighbors.Length; i++)
            {
                float3 otherPos = neighbors[i].Position;
                float3 otherVel = neighbors[i].Velocity;
                float distSq = math.distancesq(myPos, otherPos);

                // 分离 (Separation)
                float3 pushDir = myPos - otherPos;
                separationForce += pushDir / distSq; // 越近斥力越大

                // 对齐 (Alignment)
                alignmentForce += otherVel;

                // 凝聚 (Cohesion)
                cohesionCenter += otherPos;
                neighborCount++;
            }
            */

            float2 steerForce = float2.zero;

            // 2. 结算三大法则
            if (neighborCount > 0)
            {
                // 计算对齐力
                alignmentForce = (alignmentForce / neighborCount) - myVel;
                steerForce += alignmentForce * brain.AlignmentWeight;

                // 计算凝聚力
                cohesionCenter = (cohesionCenter / neighborCount) - myPos;
                steerForce += cohesionCenter * brain.CohesionWeight;

                // 计算分离力
                steerForce += separationForce * brain.SeparationWeight;
            }

            // 3. 【大哥的号召力】：跟随 Leader
            // (稍微给一个力，引导整个集群往前走)
            float2 toLeader = LeaderPosition - myPos;
            steerForce += math.normalizesafe(toLeader) * brain.LeaderFollowWeight;

            // 4. 【结果写入】：绝不直接改坐标！
            // 大脑只负责产生“意图”（力），把力塞进 PhyBody 的肌肉里！
            phyBody.Force += new UnityEngine.Vector2(steerForce.x, steerForce.y); 
        }
    }
}