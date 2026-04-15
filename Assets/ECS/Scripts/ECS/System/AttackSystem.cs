using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using LittleRPG.Physics;
using Unity.Jobs;
using AABB = LittleRPG.Physics.AABB;
using System.Linq;

namespace LittleRPG.Combat
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct AttackSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // 1. 先把所有敌人收集起来（假设敌人都有 EnemyTag 和 Health 组件）
            // var enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag, LocalTransform, HealthData>().Build();

            // 获取碰撞树
            var DynamicAABBtree = TreeInsersionSystem.DynamicBodiesAABBtree;

            // 获取shape
            var CirclesLookUp = SystemAPI.GetComponentLookup<CircleShapeData>(true);
            var BoxLookUp = SystemAPI.GetComponentLookup<BoxShapeData>(true);

            var ShapesLookUp = SystemAPI.GetComponentLookup<ShapeData>(false);
            // var attackLookup = SystemAPI.GetComponentLookup<AttackInfo>(false);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            /// <summary>
            /// 创建碰撞盒
            /// </summary>
            /// <param name="(combatState"></param>
            /// <param name="SystemAPI.Query<RefRW<AttackSate>"></param>
            /// <param name="RefRO<LocalTransform>"></param>
            /// <param name="RefRO<ShapeData>"></param>
            /// <param name="RefRO<TakeWeapon>"></param>
            /// <param name="RefRW<PlayerEquipData>>("></param>
            /// <returns></returns>
            foreach (var (combatState, transform, shape, takeWeapon, equipData, entity) in
                     SystemAPI.Query<RefRW<AttackSate>,
                      RefRO<LocalTransform>,
                      RefRO<ShapeData>,
                      RefRO<TakeWeapon>,
                      RefRW<PlayerEquipData>>()
                      .WithEntityAccess())
            {
                // 没有武器，跳过攻击逻辑
                if (equipData.ValueRO.CurrentWeapon == WeaponType.None)
                {
                    continue;
                }

                // 没扣动扳机？跳过！
                if (!combatState.ValueRO.TriggerAttackHit) continue;

                // 获取当前武器的数据（比如攻击范围和伤害）
                var weaponData = SystemAPI.GetComponent<WeaponData>(takeWeapon.ValueRO.EquippedWeapon);

                Debug.Log("发起攻击，开始创建攻击查询");


                // 创建碰撞盒
                Entity attackPacket = ecb.CreateEntity();

                float attackerRot = shape.ValueRO.Rotation;

                // 2. spawnPos (攻击盒生成的中心点)
                // 通常不是在人的脚底下，而是在人的正前方一点点
                // transform.ValueRO.Forward() 获取正前方向量
                // 1.5f 是偏移距离，保证攻击盒在武器挥动的位置
                float3 spawnPos = transform.ValueRO.Position + (transform.ValueRO.Forward() * 1.5f);

                // 2. 赋予空间属性 (LocalTransform) 
                // 可能没用
                ecb.AddComponent(attackPacket, LocalTransform.FromPosition(spawnPos));

                // 3. 赋予物理属性 (ShapeData)
                // 这里的 360 度角度值和你算好的尺寸直接塞进去
                ecb.AddComponent(attackPacket, new ShapeData
                {
                    Position = new float2(spawnPos.x, spawnPos.z),
                    Rotation = attackerRot, // 你那个 0~360 的角度值
                    shapeType = ShapeType.Box,
                    collisionLayer = CollisionLayer.PlayerLayer // 给这个包打上“玩家攻击”的标签
                });

                ecb.AddComponent(entity, new BoxShapeData
                {
                    dimensions = new Vector2(weaponData.AttackRange, weaponData.AttackRange),
                });

                // 4. 赋予业务属性 (AttackTrigger)
                ecb.AddComponent(attackPacket, new AttackInfo
                {
                    damage = weaponData.AttackDamage,
                    attacker = entity // 记录是谁打的，防止自残
                });


                // 【核心重置】：立刻把扳机松开！防止下一帧重复扣血！
                combatState.ValueRW.TriggerAttackHit = false;
            }

            /// <summary>
            /// 查询所有攻击的碰撞盒
            /// </summary>
            /// <param name="(transform"></param>
            /// <param name="SystemAPI.Query<RefRO<LocalTransform>"></param>
            /// <returns></returns>
            foreach (var (transform, shape, attackInfo, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>,
                     RefRO<ShapeData>,
                     RefRO<AttackInfo>>()
                     .WithEntityAccess())
            {

                // 记录攻击到的敌人的Entity
                NativeList<Entity> ColEntitys = new NativeList<Entity>(8, Allocator.Temp);

                // 暂时只攻击敌人
                AABB newAabb;
                switch (shape.ValueRO.shapeType)
                {
                    case ShapeType.Circle:

                        CircleShapeData circle = CirclesLookUp.GetRefRO(entity).ValueRO;

                        newAabb = new AABB
                        {
                            UpperBound = new Vector2(shape.ValueRO.Position.x + circle.radius, shape.ValueRO.Position.y + circle.radius),
                            LowerBound = new Vector2(shape.ValueRO.Position.x - circle.radius, shape.ValueRO.Position.y - circle.radius)
                        };
                        DynamicAABBtree.QueryAABBCollisions(in newAabb, CollisionLayer.MonsterLayer, ref ColEntitys);

                        break;
                    case ShapeType.Box:
                        BoxShapeData box = BoxLookUp.GetRefRO(entity).ValueRO;
                        newAabb = PhysicsUtilities.AABBfromShape(shape.ValueRO.Position, shape.ValueRO.Rotation, box);
                        DynamicAABBtree.QueryAABBCollisions(in newAabb, CollisionLayer.MonsterLayer, ref ColEntitys);

                        break;
                }
                var hitResults = new NativeList<Entity>(Allocator.TempJob);
                var hitList = new NativeList<Entity>(Allocator.TempJob);

                if(ColEntitys.IsEmpty)
                {
                    Debug.Log("没有砍到东西");
                }
                var job = new AttackAllJob
                {
                    AttackEntity = entity,
                    Candidates = ColEntitys.AsArray(),

                    CircleShapes = CirclesLookUp,
                    BoxShapes = BoxLookUp,
                    Shapes = ShapesLookUp,

                    HitResults = hitList.AsParallelWriter()
                };

                var handle = job.Schedule(ColEntitys.Length, 32);
                handle.Complete();


                for (int i = 0; i < hitList.Length; i++)
                {
                    Entity victim = hitList[i];

                    if (SystemAPI.HasBuffer<DamageBufferElement>(victim))
                    {
                        ecb.AppendToBuffer(victim, new DamageBufferElement
                        {
                            Value = attackInfo.ValueRO.damage,
                            Attacker = attackInfo.ValueRO.attacker
                        });
                    }
                }

                // 5. 攻击处理完，移除攻击标记，防止重复触发
                ecb.DestroyEntity(entity);
            }
        }
    }


    [BurstCompile]
    public partial struct AttackAllJob : IJobParallelFor
    {
        [ReadOnly] public Entity AttackEntity;
        [ReadOnly] public NativeArray<Entity> Candidates;

        [ReadOnly] public ComponentLookup<CircleShapeData> CircleShapes;
        [ReadOnly] public ComponentLookup<BoxShapeData> BoxShapes;
        [ReadOnly] public ComponentLookup<ShapeData> Shapes;

        public NativeList<Entity>.ParallelWriter HitResults;

        public void Execute(int i)
        {
            Entity victim = Candidates[i];

            if (victim == AttackEntity) return;

            bool attackIsCircle = CircleShapes.HasComponent(AttackEntity);
            bool attackIsBox = BoxShapes.HasComponent(AttackEntity);

            bool victimIsCircle = CircleShapes.HasComponent(victim);
            bool victimIsBox = BoxShapes.HasComponent(victim);

            var shapeA = Shapes[AttackEntity];
            var shapeB = Shapes[victim];

            // 🔥 分支处理
            if (attackIsCircle && victimIsCircle)
            {
                // CVC
                var circleA = CircleShapes[AttackEntity];
                var circleB = CircleShapes[victim];

                float2 delta = shapeB.Position - shapeA.Position;
                float distSq = math.lengthsq(delta);
                float radii = circleA.radius + circleB.radius;

                if (distSq <= radii * radii)
                    HitResults.AddNoResize(victim);
            }
            else if (attackIsCircle && victimIsBox)
            {
                // CVB
                var circle = CircleShapes[AttackEntity];
                var box = BoxShapes[victim];

                if (CircleVsBox(shapeA.Position, shapeB, circle, box))
                    HitResults.AddNoResize(victim);
            }
            else if (attackIsBox && victimIsCircle)
            {
                // CVB（反过来）
                var box = BoxShapes[AttackEntity];
                var circle = CircleShapes[victim];

                if (CircleVsBox(shapeB.Position, shapeA, circle, box))
                    HitResults.AddNoResize(victim);
            }
            else if (attackIsBox && victimIsBox)
            {
                // BVB
                var boxA = BoxShapes[AttackEntity];
                var boxB = BoxShapes[victim];

                float2 posA = shapeA.Position;
                float2 posB = shapeB.Position;

                float2x2 rotA = RotationMatrix(shapeA.Rotation * Mathf.Deg2Rad);
                float2x2 rotB = RotationMatrix(shapeB.Rotation * Mathf.Deg2Rad);

                float2 halfA = boxA.dimensions * 0.5f;
                float2 halfB = boxB.dimensions * 0.5f;

                if (!SATIntersection(posA, rotA, halfA, posB, rotB, halfB))
                    return;

                HitResults.AddNoResize(victim);
            }
        }

        private static bool CircleVsBox(
                float2 circlePos,
                ShapeData boxShape,
                CircleShapeData circle,
                BoxShapeData box)
        {
            float2 boxPos = boxShape.Position;

            float2x2 rot = RotationMatrix(boxShape.Rotation * Mathf.Deg2Rad);
            float2x2 invRot = math.transpose(rot);

            float2 half = box.dimensions * 0.5f;

            float2 rel = circlePos - boxPos;
            float2 local = math.mul(invRot, rel);

            float2 closest = math.clamp(local, -half, half);
            float2 world = boxPos + math.mul(rot, closest);

            float2 diff = circlePos - world;
            return math.lengthsq(diff) <= circle.radius * circle.radius;
        }

        private static float2x2 AngleToMatrix(float radians)
        {
            float c = math.cos(radians);
            float s = math.sin(radians);
            return new float2x2(c, -s, s, c);
        }

        private static float2x2 RotationMatrix(float radians)
        {
            float c = math.cos(radians);
            float s = math.sin(radians);
            return new float2x2(c, -s, s, c);
        }

        private static bool SATIntersection(
            float2 posA, float2x2 rotA, float2 halfA,
            float2 posB, float2x2 rotB, float2 halfB)
        {
            float2 rightA = rotA.c0;
            float2 upA = rotA.c1;
            float2 rightB = rotB.c0;
            float2 upB = rotB.c1;

            float2 delta = posB - posA;

            // 4轴检测
            return TestAxis(rightA) &&
                   TestAxis(upA) &&
                   TestAxis(rightB) &&
                   TestAxis(upB);

            bool TestAxis(float2 axis)
            {
                float dist = math.dot(delta, axis);

                float projA =
                    math.abs(math.dot(rightA * halfA.x, axis)) +
                    math.abs(math.dot(upA * halfA.y, axis));

                float projB =
                    math.abs(math.dot(rightB * halfB.x, axis)) +
                    math.abs(math.dot(upB * halfB.y, axis));

                return math.abs(dist) <= (projA + projB);
            }
        }
    }
}