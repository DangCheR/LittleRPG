using Unity.Entities;
using UnityEngine;

namespace LittleRPG.Combat
{
    /// <summary>
    /// 需要动画的挂载
    /// </summary>
    public class NeedsAnimationModel : IComponentData
    {
        public GameObject ModelWithAnimator; // 模型资源
    }

    public class RunningAnimation : IComponentData
    {
        public GameObject RunningAnimator; // 运行时资源
        public Animator animator;
    }
}