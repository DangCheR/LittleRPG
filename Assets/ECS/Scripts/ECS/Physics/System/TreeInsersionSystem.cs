
using System.Linq;
using LittleRPG.Combat;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using Color = UnityEngine.Color;

namespace LittleRPG.Physics
{

    /// <summary>
    /// 构建BVH树
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    public partial struct TreeInsersionSystem : ISystem//, ISystemStartStop
    {

        public static EntityQuery CirclesShapesQuery;
        public static DynamicAABBTree DynamicBodiesAABBtree;
        public static DynamicAABBTree StaticBodiesAABBtree;

        // 缓冲区
        public static float AABBfat; // 缓冲区


        /// <summary>
        /// 初始化BVH树
        /// </summary>
        /// <param name="state"></param>
        public void OnCreate(ref SystemState state)
        {
            /*Arbitratry fat on AABB to reduce de Insert/remove each frame*/
            AABBfat = 0.2f;
            /*arbitrary alocator lenght OPTI!*/
            DynamicBodiesAABBtree = new DynamicAABBTree(128);
            StaticBodiesAABBtree = new DynamicAABBTree(48);
            //AABBtree.nodes = new NativeArray<AABBTreeNode>(500, Allocator.Persistent);

        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        /// <param name="state"></param>
        public void OnDestroy(ref SystemState state)
        {
            DynamicBodiesAABBtree.DisposeAABBTree();
            StaticBodiesAABBtree.DisposeAABBTree();
        }

        public void OnUpdate(ref SystemState state)
        {
            var esECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = esECB.CreateCommandBuffer(state.WorldUnmanaged);

            #region SAFETY
            //if (StaticBodiesAABBtree.rootIndex != StaticBodiesAABBtree.nodes[StaticBodiesAABBtree.rootIndex].parentIndex)
            //{
            //    Debug.LogError("root is not parent of itself");
            //}
            //if (StaticBodiesAABBtree.nodes.Length > 1)
            //{
            //    if (StaticBodiesAABBtree.nodes[StaticBodiesAABBtree.nodes.Length - 2].parentIndex != StaticBodiesAABBtree.nodes.Length - 1)
            //    {
            //        Debug.LogError("last node is not child of last parent ");
            //        Debug.LogError(StaticBodiesAABBtree.nodes[StaticBodiesAABBtree.nodes.Length - 2].parentIndex);
            //        Debug.Break();
            //    }
            //}
            #endregion

            var CirclesLookUp = SystemAPI.GetComponentLookup<CircleShapeData>(true);
            var BoxLookUp = SystemAPI.GetComponentLookup<BoxShapeData>(true);

            #region insert at start AABB body
            /// <summary>
            /// 需要加入BVH树的都挂载TreeInsersionData
            /// </summary>
            /// <param name="(shapes"></param>
            /// <param name="SystemAPI.Query<RefRW<ShapeData>"></param>
            /// <returns></returns>
            foreach (var (shapes, insertionData, ltw, entity) in SystemAPI.Query<RefRW<ShapeData>, RefRO<TreeInsersionData>, RefRO<LocalToWorld>>().WithEntityAccess())
            {
                //shapes.ValueRW.Position  = ltw.ValueRO.Position.xy;
                //shapes.ValueRW.PreviousPosition = ltw.ValueRO.Position.xy;
                /// rotation also ?

                switch (shapes.ValueRO.shapeType)
                {
                    case ShapeType.Circle:

                        CircleShapeData circle = CirclesLookUp.GetRefRO(entity).ValueRO;
                        /// insert in static tree
                        if (insertionData.ValueRO.IsStaticBody)
                        {
                            StaticBodiesAABBtree.AddEntity(entity,
                            new AABB
                            {
                                UpperBound = new Vector2(shapes.ValueRO.Position.x + circle.radius, shapes.ValueRO.Position.y + circle.radius),
                                LowerBound = new Vector2(shapes.ValueRO.Position.x - circle.radius, shapes.ValueRO.Position.y - circle.radius)
                            },
                            shapes.ValueRO.collisionLayer
                            );
                        }
                        /// insert in dynamic tree
                        else
                        {
                            DynamicBodiesAABBtree.AddEntity(entity,
                            new AABB
                            {
                                UpperBound = new Vector2(shapes.ValueRO.Position.x + circle.radius + AABBfat, shapes.ValueRO.Position.y + circle.radius + AABBfat),
                                LowerBound = new Vector2(shapes.ValueRO.Position.x - circle.radius - AABBfat, shapes.ValueRO.Position.y - circle.radius - AABBfat)
                            },
                            shapes.ValueRO.collisionLayer
                            );
                        }
                        break;
                    case ShapeType.Box:

                        BoxShapeData box = BoxLookUp.GetRefRO(entity).ValueRO;
                        AABB boxAABB = PhysicsUtilities.AABBfromShape(shapes.ValueRO.Position, shapes.ValueRO.Rotation, box);

                        //DrawQuad(boxAABB.LowerBound, boxAABB.UpperBound, Color.red);

                        /// insert in static tree
                        if (insertionData.ValueRO.IsStaticBody)
                        {
                            StaticBodiesAABBtree.AddEntity(entity,
                            boxAABB,
                            shapes.ValueRO.collisionLayer
                            );
                        }
                        /// insert in dynamic tree
                        else
                        {
                            DynamicBodiesAABBtree.AddEntity(entity,
                            boxAABB,
                            shapes.ValueRO.collisionLayer
                            );
                        }
                        break;
                }

                ecb.RemoveComponent<TreeInsersionData>(entity);

            }

            #endregion

            ///put in job ?
            #region update the AABBtree

            for (int i = 0; i < DynamicBodiesAABBtree.nodes.Length; i++)
            {
                if (DynamicBodiesAABBtree.nodes[i].isLeaf && DynamicBodiesAABBtree.nodes[i].entity == Entity.Null)
                    Debug.LogError("树乱了");
            }

            var player = SystemAPI.GetSingletonEntity<PlayerInputData>();
            /// 遍历动态树
            foreach (int leafIndex in DynamicBodiesAABBtree.leafIndices)
            {
                AABBTreeNode newNode = DynamicBodiesAABBtree.nodes[leafIndex];

                // Debug.Log($"{newNode.entity}的父级{newNode.parentIndex}");
                
                var shape = SystemAPI.GetComponent<ShapeData>(newNode.entity);

                // 当前位置的紧闭包围盒
                AABB tight_AABB = new AABB();

                //Debug.Log(" ShapeData:rotation Quaternion->float ? et reprend box case");

                switch (shape.shapeType)
                {
                    case ShapeType.Circle:
                        tight_AABB = PhysicsUtilities.AABBfromShape(shape.Position, CirclesLookUp.GetRefRO(newNode.entity).ValueRO);
                        break;
                    case ShapeType.Box:
                        tight_AABB = PhysicsUtilities.AABBfromShape(shape.Position, shape.Rotation, BoxLookUp.GetRefRO(newNode.entity).ValueRO);
                        break;
                }

                /// <summary>
                /// 计算当前碰撞盒与原碰撞盒的差别是否超出缓冲区
                /// </summary>
                /// <param name="DynamicBodiesAABBtree.Area(DynamicBodiesAABBtree.nodes.box)"></param>
                /// <returns></returns>
                if (DynamicBodiesAABBtree.Area(DynamicBodiesAABBtree.Union(DynamicBodiesAABBtree.nodes[leafIndex].box, tight_AABB)) > DynamicBodiesAABBtree.Area(DynamicBodiesAABBtree.nodes[leafIndex].box))
                {

                    //PhysicsUtilities.CollisionLayer colLayer = AABBtree.nodes[i].layerMask;
                    // Debug.Log($"{DynamicBodiesAABBtree.nodes[leafIndex].entity}超出缓冲区");

                    newNode.box = new AABB
                    {
                        UpperBound = tight_AABB.UpperBound + new Vector2(AABBfat, AABBfat),
                        LowerBound = tight_AABB.LowerBound - new Vector2(AABBfat, AABBfat)
                    };
                    DynamicBodiesAABBtree.nodes[leafIndex] = newNode;

                    int parentIdx = DynamicBodiesAABBtree.nodes[leafIndex].parentIndex;
                    // Debug.Log($"叶子{leafIndex}的父级索引: {parentIdx}");
                    
                    if (parentIdx != -1)
                    {
                        DynamicBodiesAABBtree.Refit(parentIdx);
                    }
                }
                else
                {
                    // Debug.Log("没有超出缓冲区");
                }
                
            }

            // #if UNITY_EDITOR
            // 显示碰撞箱
            // entity显示red，父级显示Green
            foreach (var node in DynamicBodiesAABBtree.nodes)
            {
                if (node.isLeaf)
                {
                    DrawQuad(node.box.LowerBound, node.box.UpperBound, Color.red);
                }
                else
                {
                    // Debug.Log($"集合碰撞盒{node.Id}，{node.box.LowerBound}, {node.box.UpperBound}");
                    DrawQuad(node.box.LowerBound, node.box.UpperBound, Color.green);
                }
            }


            for (int i = 0; i < StaticBodiesAABBtree.nodes.Length; i++)
            {
                if (StaticBodiesAABBtree.nodes[i].isLeaf == true)
                {
                    DrawQuad(StaticBodiesAABBtree.nodes[i].box.LowerBound, StaticBodiesAABBtree.nodes[i].box.UpperBound, Color.red);
                }
            }

            // for (int i = 0; i < DynamicBodiesAABBtree.nodes.Length; i++)
            // {
            //     if (DynamicBodiesAABBtree.nodes[i].isLeaf == true)
            //     {
            //         if (DynamicBodiesAABBtree.leafIndices.Contains(i))
            //         { DrawQuad(DynamicBodiesAABBtree.nodes[i].box.LowerBound, DynamicBodiesAABBtree.nodes[i].box.UpperBound, Color.yellow); }
            //         else
            //         { DrawQuad(DynamicBodiesAABBtree.nodes[i].box.LowerBound, DynamicBodiesAABBtree.nodes[i].box.UpperBound, Color.red); }
            //     }
            //     else
            //     { DrawQuad(DynamicBodiesAABBtree.nodes[i].box.LowerBound, DynamicBodiesAABBtree.nodes[i].box.UpperBound, Color.green); }
            // }

            // for (int i = 0; i < StaticBodiesAABBtree.nodes.Length; i++)
            // {
            //     if (StaticBodiesAABBtree.nodes[i].isLeaf == true)
            //     {
            //         //if (StaticBodiesAABBtree.leafIndices.Contains(i))
            //         //{ DrawQuad(StaticBodiesAABBtree.nodes[i].box.LowerBound, StaticBodiesAABBtree.nodes[i].box.UpperBound, Color.yellow); }
            //         //else
            //         //{ DrawQuad(StaticBodiesAABBtree.nodes[i].box.LowerBound, StaticBodiesAABBtree.nodes[i].box.UpperBound, Color.red); }
            //     }
            //     else
            //     { DrawQuad(StaticBodiesAABBtree.nodes[i].box.LowerBound, StaticBodiesAABBtree.nodes[i].box.UpperBound, Color.red); }
            // }
            // #endif

            #endregion

            /// temp before box texture
            foreach (var (boxShapes, shape, body, entity) in SystemAPI.Query<RefRO<BoxShapeData>, RefRO<ShapeData>, RefRO<PhyBodyData>>().WithEntityAccess())
            {

                float halfWidth = boxShapes.ValueRO.dimensions.x * 0.5f;
                float halfHeight = boxShapes.ValueRO.dimensions.y * 0.5f;

                // Define local corners
                Vector2[] localCorners = new Vector2[4]
                {
                    new Vector2(-halfWidth, -halfHeight),
                    new Vector2(-halfWidth,  halfHeight),
                    new Vector2( halfWidth,  halfHeight),
                    new Vector2( halfWidth, -halfHeight)
                };

                // Build rotation matrix around Z
                Quaternion rotation = Quaternion.Euler(0f, 0f, shape.ValueRO.Rotation);

                // Transform to world space
                Vector3[] worldCorners = new Vector3[4];
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rotated = rotation * localCorners[i];
                    worldCorners[i] = shape.ValueRO.Position + new Vector2(rotated.x, rotated.y);
                }

                // Draw lines between corners
                for (int i = 0; i < 4; i++)
                {
                    Vector3 start = worldCorners[i];
                    Vector3 end = worldCorners[(i + 1) % 4];
                    Color color = body.ValueRO.Mass == 0 ? Color.blue : Color.green;
                    Debug.DrawLine(start, end, color);
                }
            }
        }

        public static void DrawQuad(Vector2 lowerbounds, Vector2 upperbounds, Color color)
        {
            Debug.DrawLine(new Vector3(lowerbounds.x, 1, lowerbounds.y), new Vector3(lowerbounds.x, 1, upperbounds.y), color);
            Debug.DrawLine(new Vector3(upperbounds.x, 1, upperbounds.y), new Vector3(lowerbounds.x, 1, upperbounds.y), color);
            Debug.DrawLine(new Vector3(upperbounds.x, 1, lowerbounds.y), new Vector3(upperbounds.x, 1, upperbounds.y), color);
            Debug.DrawLine(new Vector3(lowerbounds.x, 1, lowerbounds.y), new Vector3(upperbounds.x, 1, lowerbounds.y), color);
        }

    }
}