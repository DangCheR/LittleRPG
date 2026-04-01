using System;
using UnityEngine;
using QFramework;

namespace LittleRPG
{
    public interface ITweenUtility : IUtility
    {
        /// <summary>
        /// 初始化动画引擎 (设置全局容量、默认回收等)
        /// </summary>
        void Init();

        // ================= 全局控制 =================
        void PauseAll();
        void ResumeAll();
        void KillAll(bool complete = false);

        // ================= 实用功能 =================

        /// <summary>
        /// 极度好用的：无协程延迟调用！
        /// </summary>
        void DelayedCall(float delay, Action callback, bool ignoreTimeScale = false);

        // ================= 常用表现 (UI/特效) =================

        /// <summary>
        /// 标准 UI 弹出动画 (从小变大，带一点弹性回弹)
        /// </summary>
        void UIPopup(Transform target, float duration = 0.3f, Action onComplete = null);

        /// <summary>
        /// 标准 UI 关闭动画 (缩小消失)
        /// </summary>
        void UIClose(Transform target, float duration = 0.2f, Action onComplete = null);

        /// <summary>
        /// 屏幕/受击震动
        /// </summary>
        void Shake(Transform target, float duration = 0.5f, float strength = 1f);

        /// <summary>
        /// UI飞回来动画
        /// 用于物品从地上飞回背包格子，或者UI元素飞回原位等场景
        /// </summary>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        /// <param name="onComplete"></param>
        void UIFlyToLocalZero(Transform target, float duration = 0.2f, Action onComplete = null);


        public void UIFlyToTarget(
            RectTransform from,
            Vector3 to,
            float duration = 0.3f,
            Action onComplete = null
        );

        /// <summary>
        /// 伤害飘字动画 (向上飘并变淡)
        /// </summary>
        void FloatingText(CanvasGroup canvasGroup, Transform target, float distance = 50f, float duration = 1f, Action onComplete = null);
    }
}