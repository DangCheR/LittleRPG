using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Burst;

namespace LittleRPG.Combat
{
    // 注意：不能加 [BurstCompile]，因为操作了 GameObject 和 Animator
    [UpdateInGroup(typeof(PresentationSystemGroup))] // 放在表现层渲染前执行，保证视觉不延迟
    public partial struct PlayerAnimationSyncSystem : ISystem
    {
        // [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // state.RequireForUpdate<NeedsAnimationModel>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // 查找所有有 NeedsAnimationModel 标签，但还没有 PlayerAnimationGO 的实体
            foreach (var (transform, needAnim, entity) in SystemAPI.Query<RefRO<LocalTransform>, NeedsAnimationModel>()
                     .WithEntityAccess())
            {

                // 1. 实例化真正的 3D 模型
                GameObject modelGO = Object.Instantiate(needAnim.ModelWithAnimator);

                var proxy = modelGO.AddComponent<AnimationEventProxy>();

                proxy.OwnerEntity = entity; // 代理处理动画帧事件

                // 2. 初始化位置 (强行贴合 ECS 实体的出生点)
                modelGO.transform.position = transform.ValueRO.Position;
                modelGO.transform.rotation = transform.ValueRO.Rotation;

                // 3. 把引用挂给实体，并撕掉“新兵标签”
                ecb.AddComponent(entity, new RunningAnimation
                {
                    RunningAnimator = modelGO,
                    animator = modelGO.GetComponent<Animator>()
                });

                ecb.RemoveComponent<NeedsAnimationModel>(entity);

                // 4. 【天帝级隐身术】：杀掉 ECS 原本的方块渲染组件！
                if (SystemAPI.HasComponent<MaterialMeshInfo>(entity))
                {
                    ecb.RemoveComponent<MaterialMeshInfo>(entity);
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            // 遍历所有已经挂载了 3D 皮囊，且拥有输入数据 (用来判断动画) 的实体
            foreach (var (transform, inputData, animGO) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerInputData>, RunningAnimation>())
            {

                // 1. 物理位置同步 (ECS -> GameObject)
                // (注意：如果是严格的物理游戏，你可能还需要平滑插值，这里直接覆盖)
                var pos = transform.ValueRO.Position;
                // pos.y = 0; // 官方为了防止乱飞锁了 Y 轴，看你的需求
                animGO.RunningAnimator.transform.position = pos;

                // 旋转同步：与 ECS 实体同步（控制层在 ControllerSystem 中更新旋转）
                animGO.RunningAnimator.transform.rotation = transform.ValueRO.Rotation;

                // 2. 动画状态机同步 (ECS Data -> Animator)
                // 判断是否在移动 (只要输入向量的长度大于一个很小的值，就算在移动)
                bool isMoving = math.lengthsq(inputData.ValueRO.Move) > 0.01f;
                int m_IsMovingHash = Animator.StringToHash("IsMoving");
                int m_IsAttackingHash = Animator.StringToHash("Attack");
                int m_IsRollingHash = Animator.StringToHash("IsRolling");

                animGO.animator.SetBool(m_IsMovingHash, isMoving);

                // 同步攻击和翻滚 (这种瞬发状态，用 Trigger 或 Bool 都行，取决于你的状态机怎么连的)
                if (inputData.ValueRO.IsAttacking)
                {
                    animGO.animator.SetTrigger(m_IsAttackingHash);
                }

                // if (inputData.ValueRO.IsRolling)
                // {
                //     animGO.Animator.SetTrigger(m_IsRollingHash);
                // }
            }
        }
    }
}