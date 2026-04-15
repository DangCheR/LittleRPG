// using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LittleRPG.Physics
{
    /// <summary>
    /// 包围盒
    /// </summary>
    public struct AABB
    {
        public Vector2 LowerBound; // 包围盒的最小点
        public Vector2 UpperBound; // 包围盒的最大点

        public AABB(float2 lowerBound, float2 upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }
    }

    /// <summary>
    /// AABB树的节点
    /// </summary>
    public struct AABBTreeNode
    {
        public int Id; // 节点ID

        // 包围盒
        public AABB box;

        // 关联的实体
        public Entity entity;

        // 碰撞层，用于过滤碰撞检测
        public CollisionLayer layerMask;

        public int parentIndex; // 父节点ID
        public int LeftChild;
        public int RightChild;
        public bool isLeaf;// => LeftChild == -1; // 是否为叶子节点
    }

    /// <summary>
    /// 动态AABB树，
    /// 使用二叉树
    /// 基于BVH
    /// </summary>
    public struct DynamicAABBTree
    {
        public NativeList<AABBTreeNode> nodes;
        public NativeList<int> leafIndices; // 仅存放叶子节点Entity的index
        private NativeList<int> freeNodes; // Recycle removed nodes

        // 并行哈希映射，Entity到节点索引的映射
        private NativeParallelHashMap<Entity, int> entityToNodeMap; // Maps entities to their corresponding node indices

        // 根节点索引，-1表示树为空
        public int rootIndex;

        /// <summary>
        /// 初始化，Allocator.Persistent持久化
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="allocator"></param>
        public DynamicAABBTree(int capacity)
        {
            nodes = new NativeList<AABBTreeNode>(capacity, Allocator.Persistent);
            leafIndices = new NativeList<int>(capacity, Allocator.Persistent);
            freeNodes = new NativeList<int>(Allocator.Persistent);
            entityToNodeMap = new NativeParallelHashMap<Entity, int>(capacity, Allocator.Persistent);
            rootIndex = -1;
        }

        /// <summary>
        /// 销毁释放
        /// </summary>
        public void DisposeAABBTree()
        {
            nodes.Dispose();
            leafIndices.Dispose();
            freeNodes.Dispose();
            entityToNodeMap.Dispose();
        }

        /// <summary>
        /// 插入Entity时，创建一个空叶子节点
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="bounds"></param>
        /// <param name="collisionLayer"></param>
        /// <returns></returns>
        public int AddEntity(Entity entity, in AABB bounds, CollisionLayer collisionLayer)
        {
            int NodeID;

            // 从freeNodes中获取一个可用节点，如果没有则扩展nodes数组
            if (freeNodes.Length > 0)
            {
                NodeID = freeNodes[freeNodes.Length - 1];
                freeNodes.RemoveAt(freeNodes.Length - 1);
            }
            else
            {
                // ID与索引匹配
                NodeID = nodes.Length;
                nodes.Add(new AABBTreeNode());
            }

            nodes[NodeID] = new AABBTreeNode
            {
                box = bounds,
                entity = entity,
                parentIndex = -1,
                LeftChild = -1,
                RightChild = -1,
                layerMask = collisionLayer,
                isLeaf = true
            };

            // 将实体与节点ID关联
            entityToNodeMap[entity] = NodeID;

            // 插入树中
            if (rootIndex == -1)
            {
                rootIndex = NodeID;
                leafIndices.Add(NodeID);
            }
            else
            {
                InsertLeaf(NodeID, rootIndex);
            }

            return NodeID;
        }


        /// <summary>
        /// 插入叶子
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="currentEvaluatedNode"></param>
        private void InsertLeaf(int nodeID, int currentEvaluatedNode)
        {
            // 当前包围盒节点为叶子节点时
            if (nodes[currentEvaluatedNode].isLeaf)
            {
                // 新评估对象
                AABBTreeNode newEvaluatedNode = nodes[currentEvaluatedNode];

                // 当前要插入节点
                AABBTreeNode newNode = nodes[nodeID];

                int newParent = nodes.Length;

                // 与别的Entity分到一个包围盒时，父包围盒为两者的最小全包围
                nodes.Add(new AABBTreeNode
                {
                    box = Union(newEvaluatedNode.box, newNode.box),
                    LeftChild = currentEvaluatedNode,
                    RightChild = nodeID,
                    parentIndex = newEvaluatedNode.parentIndex,
                    isLeaf = false
                });

                newEvaluatedNode.parentIndex = newParent;
                newNode.parentIndex = newParent;
                nodes[currentEvaluatedNode] = newEvaluatedNode;
                nodes[nodeID] = newNode;

                if (nodes[newParent].parentIndex != -1)
                {
                    //Debug.Log("test");
                    AABBTreeNode grandparent = nodes[nodes[newParent].parentIndex];
                    if (grandparent.LeftChild == currentEvaluatedNode) grandparent.LeftChild = newParent;
                    else grandparent.RightChild = newParent;
                    nodes[nodes[newParent].parentIndex] = grandparent; // Update the parent node
                    Refit(newParent);
                }
                else { rootIndex = newParent; }

                leafIndices.Add(nodeID);
            }
            else // 非叶子继续向下找
            {
                // 左右存在空位直接放
                if (nodes[currentEvaluatedNode].LeftChild == -1)
                {
                    //Debug.Log("HERE 0");
                    AABBTreeNode newEvaluatedNode = nodes[currentEvaluatedNode];
                    AABBTreeNode newNode = nodes[nodeID];
                    newEvaluatedNode.LeftChild = nodeID;
                    newNode.parentIndex = currentEvaluatedNode;
                    nodes[currentEvaluatedNode] = newEvaluatedNode;
                    nodes[nodeID] = newNode;
                    leafIndices.Add(nodeID);
                    return;
                }

                if (nodes[currentEvaluatedNode].RightChild == -1)
                {
                    //Debug.Log("HERE 1");
                    AABBTreeNode newEvaluatedNode = nodes[currentEvaluatedNode];
                    AABBTreeNode newNode = nodes[nodeID];
                    newEvaluatedNode.RightChild = nodeID;
                    newNode.parentIndex = currentEvaluatedNode;
                    nodes[currentEvaluatedNode] = newEvaluatedNode;
                    nodes[nodeID] = newNode;
                    leafIndices.Add(nodeID);
                    return;
                }

                /// 计算左右占据包围盒大小
                float costLeft = Area(Union(nodes[nodes[currentEvaluatedNode].LeftChild].box, nodes[nodeID].box));
                float costRight = Area(Union(nodes[nodes[currentEvaluatedNode].RightChild].box, nodes[nodeID].box));

                if (costLeft < costRight)
                    InsertLeaf(nodeID, nodes[currentEvaluatedNode].LeftChild);
                else
                    InsertLeaf(nodeID, nodes[currentEvaluatedNode].RightChild);

                // 插入到非叶子节点的空子节点位时，或与叶子节点组合成新的非叶节点时结束
                Refit(currentEvaluatedNode);
            }
            // nodes[nodeID].isLeaf = true;
        }


        public void DisableEntity(Entity entity)
        {
            if (!entityToNodeMap.TryGetValue(entity, out int nodeID))
            {
                Debug.LogError($"BVH树删除Entity不存在: {entity.Index}");
            }
            // 记录需要调整的最下层
            int parent = nodes[nodeID].parentIndex;
            int indexToRefit = parent;

            // 没有父节点？你把根节点删了？
            if (parent == -1)
            {
                Debug.Log("你把根节点删了？");
                rootIndex = -1;
            }
            else
            {
                // 需自下而上更新包围盒
                int newParentIdx = UpdateAncestor(parent, nodeID);

                indexToRefit = newParentIdx;
            }

            // 添加到自由节点
            freeNodes.Add(nodeID);

            // 移除现有节点map
            entityToNodeMap.Remove(entity);

            Refit(indexToRefit);

            // 与最后一个交换后移除最后一个，压榨性能
            int indexOfID = leafIndices.IndexOf(nodeID);

            int lastIndex = leafIndices.Length - 1;
            if (indexOfID != lastIndex)
            {
                leafIndices[indexOfID] = leafIndices[lastIndex];
            }

            leafIndices.RemoveAt(lastIndex);
        }

        /// <summary>
        /// 调整祖先节点
        /// 有待考究，为什么不直接把另一个挪到父节点
        /// </summary>
        /// <param name="currentParentIdx"></param>
        /// <param name="currentEvaluatedNode"></param>
        /// <returns></returns>
        private int UpdateAncestor(int currentParentIdx, int currentEvaluatedNode)
        {
            int otherChildIdx = -99;
            AABBTreeNode newParent = nodes[currentParentIdx];
            if (nodes[currentParentIdx].LeftChild == currentEvaluatedNode)
            {
                newParent.LeftChild = -1;  // mark as unused
                otherChildIdx = newParent.RightChild;
            }
            else
            {
                newParent.RightChild = -1;  // mark as unused
                otherChildIdx = newParent.LeftChild;
            }
            while (otherChildIdx == -1 && nodes[currentParentIdx].parentIndex != -1)
            {
                //Debug.LogWarning("pop parent");
                //newParent.nodeType = 0; //disable

                /// ?
                nodes[currentParentIdx] = newParent;

                currentEvaluatedNode = currentParentIdx;
                currentParentIdx = nodes[currentParentIdx].parentIndex;
                newParent = nodes[currentParentIdx];
                if (nodes[currentParentIdx].LeftChild == currentEvaluatedNode)
                {
                    freeNodes.Add(newParent.LeftChild);
                    newParent.LeftChild = -1;  // mark as unused
                    otherChildIdx = newParent.RightChild;
                }
                else
                {
                    freeNodes.Add(newParent.RightChild);
                    newParent.RightChild = -1;  // mark as unused
                    otherChildIdx = newParent.LeftChild;
                }
            }
            nodes[currentParentIdx] = newParent;

            ///// both childs are disabled. Disable the parent as well
            //if(otherChildState==-1)
            //{
            //    AABBTreeNode newGrandParent = nodes[nodes[parent].parentIndex];
            //    if (newGrandParent.LeftChild == parent) { newGrandParent.LeftChild = -1; }
            //    else { newGrandParent.RightChild = -1; }
            //    nodes[nodes[parent].parentIndex] = newGrandParent;

            //}
            //nodes[parent] = newParent;
            return currentParentIdx;
        }

        /// <summary>
        /// 更新树，自下而上更新父节点
        /// </summary>
        /// <param name="parentNodeID"></param>
        public void Refit(int parentNodeID)
        {
            while (parentNodeID != -1)
            {
                AABBTreeNode newNode = nodes[parentNodeID];
                if (newNode.LeftChild == -1)
                {
                    newNode.box = nodes[newNode.RightChild].box;
                }
                else if (newNode.RightChild == -1)
                {
                    newNode.box = nodes[newNode.LeftChild].box;
                }
                else
                {
                    newNode.box = Union(nodes[newNode.LeftChild].box, nodes[newNode.RightChild].box);
                }

                ///如果包围盒没有变化，自己以及上层都无需调整大小
                if ((newNode.box.LowerBound == nodes[parentNodeID].box.LowerBound)
                     && (newNode.box.UpperBound == nodes[parentNodeID].box.UpperBound))
                {
                    break;
                }
                else
                {
                    // 存在变化直接把新的赋值
                    nodes[parentNodeID] = newNode;
                    parentNodeID = nodes[parentNodeID].parentIndex;
                }
            }
        }


        /// <summary>
        /// 节点是否有交集
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private bool IsOverlapping(int nodeA, int nodeB)
        {
            return 0 < PhysicsUtilities.Proximity(nodes[nodeA].box, nodes[nodeB].box);
        }


        /// <summary>
        /// 节点是否有交集
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private bool IsOverlapping(AABBTreeNode nodeA, AABBTreeNode nodeB)
        {
            return 0 < PhysicsUtilities.Proximity(nodeA.box, nodeB.box);
        }


        /// <summary>
        /// 节点与AABB是否有交集
        /// </summary>
        private bool IsOverlapping(in AABB aabbA, in AABB aabbB)
        {
            return 0 < PhysicsUtilities.Proximity(aabbA, aabbB);
        }


        /// <summary>
        /// 查询临时碰撞盒与树中节点的碰撞
        /// 用于攻击效果、技能范围等只存在一帧的碰撞盒
        /// </summary>
        /// <param name="queryAABB">临时碰撞盒</param>
        /// <param name="queryLayer">查询碰撞层</param>
        /// <param name="ColPair">输出碰撞对列表</param>
        public void QueryAABBCollisions(in AABB queryAABB, CollisionLayer queryLayer, ref NativeList<Entity> Cols)
        {
            if (rootIndex == -1) return;
            QueryAABBNode(queryAABB, queryLayer, ref Cols, rootIndex);
        }

        /// <summary>
        /// 递归查询节点与临时AABB的碰撞
        /// </summary>
        private void QueryAABBNode(in AABB queryAABB, CollisionLayer queryLayer, ref NativeList<Entity> Cols, int nodeIndex)
        {
            if (nodeIndex == -1) return;

            AABBTreeNode node = nodes[nodeIndex];

            // 包围盒没有交集直接返回
            if (!IsOverlapping(queryAABB, node.box)) return;

            // 是叶子节点，检测碰撞层并记录
            if (node.isLeaf)
            {
                if (queryLayer == node.layerMask)
                {
                    // 返回节点
                    Cols.Add(node.entity);
                }
                return;
            }

            // 非叶子节点，继续递归
            QueryAABBNode(in queryAABB, queryLayer, ref Cols, node.LeftChild);
            QueryAABBNode(in queryAABB, queryLayer, ref Cols, node.RightChild);
        }



        /// <summary>
        /// 搜索可能碰撞的结果
        /// 先处理根节点下左与右的大集合之间的碰撞，在递归依次处理
        /// </summary>
        /// <param name="ColPair"></param>
        /// <param name="index"></param>
        public void GatherIntersectingNodes(ref NativeList<CollisionPair> ColPair, int index)
        {

            if (index == -1) return;
            /// Parent node
            if (nodes[index].isLeaf == false)
            {
                TryRegisterCollisionPair(ref ColPair, nodes[index].LeftChild, nodes[index].RightChild);
                GatherIntersectingNodes(ref ColPair, nodes[index].LeftChild);
                GatherIntersectingNodes(ref ColPair, nodes[index].RightChild);
            }
        }


        /// <summary>
        /// 检测两个碰撞盒，将可能发生碰撞的包围盒对注册到系统
        /// </summary>
        /// <param name="ColPair"></param>
        /// <param name="nodeAidx"></param>
        /// <param name="nodeBidx"></param>
        private void TryRegisterCollisionPair(ref NativeList<CollisionPair> ColPair, int nodeAidx, int nodeBidx)
        {
            if (nodeAidx == -1 || nodeBidx == -1) return;

            // 包围盒没有交集直接返回
            if (!IsOverlapping(nodeAidx, nodeBidx)) return;

            var nodeA = nodes[nodeAidx];
            var nodeB = nodes[nodeBidx];

            // bool shouldcol = PhysicsUtilities.ShouldCollide(nodeA.layerMask, nodeB.layerMask);
            // Debug.Log($"{nodeA.entity}与{nodeB.entity}接触，是否应该碰撞：{shouldcol}");
            // 两个都是叶子节点直接添加并返回
            if (nodeA.isLeaf == true && nodeB.isLeaf == true
                    && PhysicsUtilities.ShouldCollide(nodeA.layerMask, nodeB.layerMask))
            {
                ColPair.Add(new CollisionPair { EntityA = nodeA.entity, EntityB = nodeB.entity });
                return;
            }

            // A为叶子，递归B
            if (nodeA.isLeaf == true)
            {
                TryRegisterCollisionPair(ref ColPair, nodeAidx, nodeB.LeftChild);
                TryRegisterCollisionPair(ref ColPair, nodeAidx, nodeB.RightChild);
                return;
            }

            // B为叶子，递归A
            if (nodeB.isLeaf == true)
            {
                TryRegisterCollisionPair(ref ColPair, nodeBidx, nodeA.LeftChild);
                TryRegisterCollisionPair(ref ColPair, nodeBidx, nodeA.RightChild);
                return;
            }
            // Both are internal nodes, recurse into both children
            TryRegisterCollisionPair(ref ColPair, nodeA.LeftChild, nodeB.LeftChild);
            TryRegisterCollisionPair(ref ColPair, nodeA.LeftChild, nodeB.RightChild);
            TryRegisterCollisionPair(ref ColPair, nodeA.RightChild, nodeB.LeftChild);
            TryRegisterCollisionPair(ref ColPair, nodeA.RightChild, nodeB.RightChild);
        }

        /// <summary>
        /// 遍历动态树每个有效节点，收集动态树与静态BVH树的碰撞结果
        /// </summary>
        /// <param name="ColPair"></param>
        /// <param name="dynamibBodiesTree"></param>
        /// <param name="index"></param>
        public void GatherIntersectingStaticNodes(ref NativeList<CollisionPair> ColPair, ref DynamicAABBTree dynamibBodiesTree, int index)
        {
            if (index == -1) return;
            for (int i = 0; i < dynamibBodiesTree.leafIndices.Length; i++)
            {
                TryRegisterStaticCollisionPair(ref ColPair, index, dynamibBodiesTree.nodes[dynamibBodiesTree.leafIndices[i]]);
            }
        }

        /// <summary>
        /// 注册与静态BVH树的碰撞对
        /// </summary>
        /// <param name="ColPair"></param>
        /// <param name="staticNodeIdx"></param>
        /// <param name="dynamicNode"></param>
        private void TryRegisterStaticCollisionPair(ref NativeList<CollisionPair> ColPair, int staticNodeIdx, AABBTreeNode dynamicNode)
        {
            if (staticNodeIdx == -1) return;
            AABBTreeNode node = nodes[staticNodeIdx];
            if (!IsOverlapping(node, dynamicNode)) return;

            if (node.isLeaf == true)
            {
                if (PhysicsUtilities.ShouldCollide(node.layerMask, dynamicNode.layerMask))
                {
                    ColPair.Add(new CollisionPair { EntityA = node.entity, EntityB = dynamicNode.entity });
                }
            }
            else
            {
                TryRegisterStaticCollisionPair(ref ColPair, node.LeftChild, dynamicNode);
                TryRegisterStaticCollisionPair(ref ColPair, node.RightChild, dynamicNode);
            }
        }


        /// <summary>
        /// 创建两物体的交集围成的包围盒
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public AABB Union(AABB A, AABB B)
        {
            AABB C;
            C.LowerBound = new Vector2(Mathf.Min(A.LowerBound.x, B.LowerBound.x), Mathf.Min(A.LowerBound.y, B.LowerBound.y));
            C.UpperBound = new Vector2(Mathf.Max(A.UpperBound.x, B.UpperBound.x), Mathf.Max(A.UpperBound.y, B.UpperBound.y));
            return C;
        }

        /// <summary>
        /// 计算包围盒面积
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public float Area(AABB A)
        {
            Vector2 d = A.UpperBound - A.LowerBound;
            return d.x * d.y;
        }

    }

}