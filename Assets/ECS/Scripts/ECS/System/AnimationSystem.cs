using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Burst;
using TMPro;
using LittleRPG.Physics;

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

            int m_IsMovingHash = Animator.StringToHash("IsMoving");
            int m_IsAttackingHash = Animator.StringToHash("Attack");
            int m_IsChoppingHash = Animator.StringToHash("Chop");
            int m_IsRollingHash = Animator.StringToHash("IsRolling");
            int m_IsRidingHash = Animator.StringToHash("IsRiding");

            // 查找所有有 NeedsAnimationModel 标签，但还没有 PlayerAnimationGO 的实体
            foreach (var (transform, needAnim, entity) in SystemAPI.Query<RefRO<LocalTransform>, NeedsAnimationModel>()
                     .WithEntityAccess())
            {
                // 1. 实例化真正的 3D 模型
                GameObject modelGO = Object.Instantiate(needAnim.ModelWithAnimator);

                var proxy = modelGO.GetComponent<AnimationEventProxy>();

                if (proxy != null)
                {
                    proxy.OwnerEntity = entity; // 代理处理动画帧事件
                }

                var interact = modelGO.GetComponent<BaseInteractive>();

                if (interact != null)
                {
                    interact.OwnerEntity = entity; // 让交互组件也记住自己的 Entity
                }

                // 如果模型上有 CanTakeWeapon 组件
                // 说明它可以拿武器，位置同步系统需要知道武器挂在哪个骨骼上
                var canTakeWeapon = modelGO.GetComponent<CanTakeWeapon>();

                if (canTakeWeapon != null)
                {
                    ecb.AddComponent(entity, new WeaponBelongBone
                    {
                        WeaponHoldPoint = canTakeWeapon.WeaponHoldPoint,
                    });
                }

                // 2. 初始化位置 (强行贴合 ECS 实体的出生点)
                modelGO.transform.position = transform.ValueRO.Position;
                modelGO.transform.rotation = transform.ValueRO.Rotation;
                // Getcomponent<Animator>().SetBool(m_IsMovingHash, false); // 默认不动

                // 3. 把引用挂给实体，并撕掉“新兵标签”
                ecb.AddComponent(entity, new RunningAnimation
                {
                    RunningModel = modelGO,
                    animator = modelGO.GetComponent<Animator>()
                });

                ecb.RemoveComponent<NeedsAnimationModel>(entity);

                // 4. 【天帝级隐身术】：杀掉 ECS 原本的方块渲染组件！
                if (SystemAPI.HasComponent<MaterialMeshInfo>(entity))
                {
                    ecb.RemoveComponent<MaterialMeshInfo>(entity);
                }
            }

            /// <summary>
            /// 模型位置同步
            /// </summary>
            /// <param name="(transform"></param>
            /// <param name="SystemAPI.Query<RefRO<LocalTransform>"></param>
            /// <param name="RefRO<PhyBodyData>"></param>
            /// <param name="RunningAnimation>().WithEntityAccess()"></param>
            /// <returns></returns>
            foreach (var (transform, shape, animGO, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>,
                     RefRO<ShapeData>,
                     RunningAnimation>().WithEntityAccess())
            {
                if (SystemAPI.HasComponent<RiderTag>(entity))
                {
                    var riderTag = SystemAPI.GetComponent<RiderTag>(entity);

                    // 如果有坐骑，把模型交给主世界代理
                    if (riderTag.MountEntity != Entity.Null)
                    {
                        continue;
                    }
                }
                animGO.RunningModel.transform.position = transform.ValueRO.Position;
                animGO.RunningModel.transform.rotation = transform.ValueRO.Rotation;
            }


            /// <summary>
            /// 处理移动动画
            /// </summary>
            /// <param name="(transform"></param>
            /// <param name="SystemAPI.Query<RefRO<LocalTransform>"></param>
            /// <param name="RefRO<MoveComponent>"></param>
            /// <param name="RunningAnimation>().WithEntityAccess()"></param>
            /// <returns></returns>
            foreach (var (transform, moveComponent, animGO, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>,
                     RefRO<MoveComponent>,
                     RunningAnimation>().WithEntityAccess())
            {
                if (SystemAPI.HasComponent<RiderTag>(entity))
                {
                    var riderTag = SystemAPI.GetComponent<RiderTag>(entity);//.MountEntity = Entity.Null; // 默认没有骑乘坐骑
                    if (riderTag.MountEntity != Entity.Null)
                        continue; // 如果正在骑乘，移动动画由坐骑控制，玩家模型保持静止
                }

                // 物理位置同步 (ECS -> GameObject)
                var pos = transform.ValueRO.Position;

                // 模型的位置统一处理
                // pos.y = 0; // 官方为了防止乱飞锁了 Y 轴，看你的需求
                // animGO.RunningModel.transform.position = pos;

                // 旋转同步：与 ECS 实体同步（控制层在 ControllerSystem 中更新旋转）
                // animGO.RunningModel.transform.rotation = transform.ValueRO.Rotation;

                // 2. 动画状态机同步 (ECS Data -> Animator)
                // 判断是否在移动 (只要输入向量的长度大于一个很小的值，就算在移动)
                bool isMoving = math.lengthsq(moveComponent.ValueRO.MoveDirection) > 0.01f;

                animGO.animator.SetBool(m_IsMovingHash, isMoving);
            }

            /// <summary>
            /// 处理攻击动画
            /// </summary>
            /// <param name="(combatComponent"></param>
            /// <param name="SystemAPI.Query<RefRW<AttackSate>"></param>
            /// <param name="RunningAnimation>().WithEntityAccess()"></param>
            /// <returns></returns>
            foreach (var (combatComponent, animGO, entity) in
                     SystemAPI.Query<RefRW<AttackSate>,
                     RunningAnimation>()
                     .WithEntityAccess())
            {
                if (!combatComponent.ValueRO.StartAttack) continue;

                // 有武器时触发砍击动画
                if (SystemAPI.HasComponent<PlayerEquipData>(entity))
                {
                    var Weapon = SystemAPI.GetComponent<PlayerEquipData>(entity);
                    Debug.Log($"实体 {entity.Index}配备了武器，武器类型为 {Weapon.CurrentWeapon}！");

                    if (Weapon.CurrentWeapon == WeaponType.Sword)
                    {
                        animGO.animator.SetTrigger(m_IsChoppingHash);
                    }
                    else
                    {
                        animGO.animator.SetTrigger(m_IsAttackingHash);
                    }
                }
                else
                {
                    // 没有武器时触发默认攻击动画
                    animGO.animator.SetTrigger(m_IsAttackingHash);
                }

                Debug.Log($"实体 {entity.Index} 触发攻击动画！");
                combatComponent.ValueRW.StartAttack = false; // 重置攻击状态，避免重复触发
            }

            /// <summary>
            /// 处理骑乘动画
            /// </summary>
            /// <param name="(transform"></param>
            /// <param name="SystemAPI.Query<RefRO<LocalTransform>"></param>
            /// <param name="RefRO<RiderTag>"></param>
            /// <param name="RunningAnimation>().WithEntityAccess()"></param>
            /// <returns></returns>
            foreach (var (riderTag, animGO) in
                     SystemAPI.Query<RefRO<RiderTag>,
                     RunningAnimation>()
                     .WithChangeFilter<RiderTag>())
            {
                // 当走的时候上马导致玩家还在move动画
                // 如果正在骑乘，移动动画由坐骑控制，玩家模型保持Idle
                if (riderTag.ValueRO.MountEntity == Entity.Null) continue;
                animGO.animator.SetBool(m_IsMovingHash, false);
            }


            /// <summary>
            /// 处理死亡动画
            /// </summary>
            /// <param name="(tag"></param>
            /// <param name="SystemAPI.Query<DeadTag>().WithNone<DeceasedTag>().WithEntityAccess()"></param>
            /// <typeparam name="DeadTag"></typeparam>
            /// <returns></returns>
            foreach (var (animGO, daedTag, entity) in SystemAPI.Query<RunningAnimation, EnabledRefRW<DeadTag>>().WithNone<DeceasedTag>().WithEntityAccess())
            {
                // 仅仅打印一下
                UnityEngine.Debug.Log($"[ECS] 实体 {entity.Index} 触发死亡！正在准备播放动画...");

                int m_IsDeadHash = Animator.StringToHash("IsDead");

                // 触发死亡动画
                animGO.animator.SetTrigger(m_IsDeadHash);

                UnityEngine.Debug.Log($"设置完了，我没招了");

                //最后一个用DeadTag的负责关闭
                ecb.SetComponentEnabled<DeadTag>(entity, false);
                // 这里你可以在动画系统中根据 DeathTag 切换 Animator 状态
                // 此时不处理删除，逻辑跳过
            }

            /// <summary>
            /// 没人关系这个东西有没有死透
            /// 只有动画系统需要收尸
            /// </summary>
            /// <param name="(animGO"></param>
            /// <param name="SystemAPI.Query<RunningAnimation>().WithAll<DeceasedTag>().WithEntityAccess()"></param>
            /// <typeparam name="RunningAnimation"></typeparam>
            /// <returns></returns>
            foreach (var (animGO, entity) in SystemAPI.Query<RunningAnimation>().WithAll<DeceasedTag>().WithEntityAccess())
            {
                if (animGO.RunningModel != null)
                {
                    Object.Destroy(animGO.RunningModel);
                }

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

}