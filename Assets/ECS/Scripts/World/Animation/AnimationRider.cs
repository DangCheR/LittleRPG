using UnityEngine;
using QFramework;
using Unity.Entities;
using Unity.Transforms;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 骑乘动画控制器
    /// 挂在玩家模型上，负责根据骑乘状态切换动画和设置 IK
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationRider : MonoBehaviour
    {
        private Animator mAnimator;

        public Entity playerEntity;

        public EntityManager entityManager;

        [Header("骑乘状态")]
        public bool IsRiding = false;

        [Header("坐骑锚点")]
        public Transform MountSeatPoint;

        public Transform LeftFootStirrup;

        public Transform RightFootStirrup;

        // IK 权重 (平滑过渡用)
        [Range(0, 1)] public float IKWeight = 0f;

        // public GameObject horse;

        private void Awake()
        {
            mAnimator = GetComponent<Animator>();
        }

        // 骑上小猪的通用接口
        public void StartRiding(Transform seat, Transform lFoot, Transform rFoot)
        {
            MountSeatPoint = seat;
            LeftFootStirrup = lFoot;
            RightFootStirrup = rFoot;

            IsRiding = true;



            // 获取位置
            // if (playerEntity == Entity.Null) GetPlayerEntity();

            // var localTrans = entityManager.GetComponentData<LocalTransform>(playerEntity);

            // 拿数据 -> 修改 -> 塞回去

            // 1. 物理上认小猪当干爹 (让承太郎跟着猪走)
            transform.SetParent(MountSeatPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            // localTrans.Position.y = transform.position.y;

            // entityManager.SetComponentData(playerEntity, localTrans);

            // 2. 播放通用的骑乘 Idle 动画 (双腿分开的姿势)
            // mAnimator.CrossFade("Ride", 0.2f);

            // 3. 用 DoTween 把 IKWeight 从 0 平滑变到 1 (可选，显得手脚吸附过去很自然)
            // this.GetUtility<ITweenUtility>().DoFloat(0, 1, 0.3f, w => IKWeight = w);
            IKWeight = 1f; // 简单粗暴直接设为 1
        }

        public void StopRiding()
        {
            IsRiding = false;
            IKWeight = 0f;
            transform.SetParent(null); // 脱离小猪
            // var localTrans = entityManager.GetComponentData<LocalTransform>(playerEntity);
            
            // localTrans.Position.y = 0;

            // entityManager.SetComponentData(playerEntity, localTrans);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (mAnimator == null) return;

            if (IsRiding)
            {

                // --- 3. 设置左脚 IK ---
                if (LeftFootStirrup != null)
                {
                    mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, IKWeight);
                    mAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, IKWeight);
                    mAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFootStirrup.position);
                    mAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, LeftFootStirrup.rotation);
                }

                // --- 4. 设置右脚 IK ---
                if (RightFootStirrup != null)
                {
                    mAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, IKWeight);
                    mAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, IKWeight);
                    mAnimator.SetIKPosition(AvatarIKGoal.RightFoot, RightFootStirrup.position);
                    mAnimator.SetIKRotation(AvatarIKGoal.RightFoot, RightFootStirrup.rotation);
                }
            }
            else
            {
                // 如果不骑了，把权重全归零，交回给普通动画接管
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            }
        }

        public void GetPlayerEntity()
        {
            // // 获取当前激活的 ECS 世界（这是最安全、最正宗的拿法）
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            this.entityManager = world.EntityManager;

            // 创建查询并查找 Singleton
            var query = entityManager.CreateEntityQuery(typeof(PlayerInputData));

            // 确保查询到玩家实体
            if (!query.TryGetSingletonEntity<PlayerInputData>(out var playerEntity))
            {
                Debug.LogError("没有找到玩家实体！");
                return;
            }

            this.playerEntity = playerEntity;
        }
    }
}