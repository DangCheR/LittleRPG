using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RiderIK : MonoBehaviour
{
    private Animator mAnimator;

    [Header("骑乘状态")]
    public bool IsRiding = false;

    [Header("小猪身上的锚点 (骑上猪时由代码赋值)")]
    public Transform MountSeatPoint; // 屁股坐哪
    public Transform LeftHandGrip;
    public Transform RightHandGrip;
    public Transform LeftFootStirrup;
    public Transform RightFootStirrup;[Header("IK 权重 (平滑过渡用)")]
    [Range(0, 1)] public float IKWeight = 0f; 

    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
    }

    // 骑上小猪的通用接口
    public void StartRiding(Transform seat, Transform lHand, Transform rHand, Transform lFoot, Transform rFoot)
    {
        MountSeatPoint = seat;
        LeftHandGrip = lHand;
        RightHandGrip = rHand;
        LeftFootStirrup = lFoot;
        RightFootStirrup = rFoot;

        IsRiding = true;

        // 1. 物理上认小猪当干爹 (让承太郎跟着猪走)
        transform.SetParent(MountSeatPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 2. 播放通用的骑乘 Idle 动画 (双腿分开的姿势)
        mAnimator.CrossFade("RideIdle", 0.2f);
        
        // 3. 用 DoTween 把 IKWeight 从 0 平滑变到 1 (可选，显得手脚吸附过去很自然)
        // this.GetUtility<ITweenUtility>().DoFloat(0, 1, 0.3f, w => IKWeight = w);
        IKWeight = 1f; // 简单粗暴直接设为 1
    }

    public void StopRiding()
    {
        IsRiding = false;
        IKWeight = 0f;
        transform.SetParent(null); // 脱离小猪
    }

    // ==========================================
    // 👑 核心魔法：Unity 原生 IK 回调函数
    // 只要 Animator 勾选了 IK Pass，每帧渲染前都会自动调这里！
    // ==========================================
    private void OnAnimatorIK(int layerIndex)
    {
        if (mAnimator == null) return;

        if (IsRiding)
        {
            // --- 1. 设置左手 IK ---
            if (LeftHandGrip != null)
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, IKWeight);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, IKWeight);
                mAnimator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandGrip.position);
                mAnimator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandGrip.rotation);
            }

            // --- 2. 设置右手 IK ---
            if (RightHandGrip != null)
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, IKWeight);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, IKWeight);
                mAnimator.SetIKPosition(AvatarIKGoal.RightHand, RightHandGrip.position);
                mAnimator.SetIKRotation(AvatarIKGoal.RightHand, RightHandGrip.rotation);
            }

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
            mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            mAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            // ... (右脚双手同理)
        }
    }
}