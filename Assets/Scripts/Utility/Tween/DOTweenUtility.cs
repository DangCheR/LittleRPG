using System;
using UnityEngine;
using DG.Tweening;
using QFramework;

namespace LittleRPG
{
    public class DOTweenUtility : ITweenUtility
    {
        public void Init()
        {
            // 初始化 DOTween，设置最大容量，提升性能避免运行时扩容
            DOTween.Init(recycleAllByDefault: true, useSafeMode: true, LogBehaviour.ErrorsOnly);
            DOTween.SetTweensCapacity(500, 50);
            Debug.Log("[DOTweenUtility] DOTween 已成功初始化！");
        }

        #region 全局控制
        public void PauseAll() => DOTween.PauseAll();
        public void ResumeAll() => DOTween.PlayAll();
        public void KillAll(bool complete = false) => DOTween.KillAll(complete);
        #endregion

        #region 延迟调用
        public void DelayedCall(float delay, Action callback, bool ignoreTimeScale = false)
        {
            DOVirtual.DelayedCall(delay, new TweenCallback(callback), ignoreTimeScale);
        }
        #endregion

        #region 常用表现
        public void UIPopup(Transform target, float duration = 0.3f, Action onComplete = null)
        {
            target.localScale = Vector3.zero;
            // OutBack 提供了一个非常好看的“弹簧”效果
            target.DOScale(Vector3.one, duration)
                  .SetEase(Ease.OutBack)
                  .SetUpdate(true) // UI 动画通常不受 Time.timeScale 影响
                  .OnComplete(() => onComplete?.Invoke());
        }
        /// <summary>
        /// UI强调效果，变大再变回正常大小
        /// 使用在金币消耗，装备选中等需要强调的场景
        /// </summary>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        /// <param name="onComplete"></param>
        public void UIStress(
            Transform target,
            float duration = 0.3f,
            float scaleMultiplier = 1.5f,
            Ease ease = Ease.OutBack,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
            {
                Debug.LogWarning("UIExpansion target is null");
                return;
            }

            // 记录原始缩放
            Vector3 originalScale = target.localScale;

            // 记录目标缩放
            Vector3 targetScale = originalScale * scaleMultiplier;

            //防止叠加动画
            target.DOKill();

            var sequence = DOTween.Sequence()
                .Append(target.DOScale(originalScale, duration).SetEase(ease).SetUpdate(ignoreTimeScale))
                .OnComplete(() => onComplete?.Invoke());

            Tweener BeLittleTween = target.DOScale(targetScale, duration * 0.4f)
                .SetEase(ease)
                .SetUpdate(ignoreTimeScale)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });

            // 变小动画
            Tweener BebigTween = target.DOScale(originalScale, duration * 0.6f)
                .SetEase(ease)
                .SetUpdate(ignoreTimeScale)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });

            sequence.Append(BeLittleTween).Append(BebigTween);
            sequence.Play();
        }

        /// <summary>
        /// UI飞回来动画
        /// 用于物品从地上飞回背包格子，或者UI元素
        /// </summary>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        /// <param name="onComplete"></param>
        public void UIFlyToLocalZero(Transform target, float duration = 0.2f, Action onComplete = null)
        {
            target.DOKill(); // 杀掉之前的动画防止冲突
                             // 使用 OutQuad 显得轻快一点
            target.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// UI飞到另一个UI元素位置的动画
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ease"></param>
        /// <param name="onComplete"></param>
        public void UIFlyToTarget(
            RectTransform from,
            Vector3 to,
            float duration = 0.3f,
            Action onComplete = null)
        {
            if (from == null || to == null)
            {
                Debug.LogWarning("UIFlyToTarget: target is null");
                return;
            }

            // 先杀掉旧动画
            from.DOKill();

            // 获取目标的世界坐标
            Vector3 targetWorldPos = to;

            // 直接飞到目标（世界坐标）
            from.DOMove(targetWorldPos, duration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        public void UIClose(Transform target, float duration = 0.2f, Action onComplete = null)
        {
            target.DOScale(Vector3.zero, duration)
                  .SetEase(Ease.InBack)
                  .SetUpdate(true)
                  .OnComplete(() => onComplete?.Invoke());
        }

        public void Shake(Transform target, float duration = 0.5f, float strength = 1f)
        {
            // 杀掉该物体身上旧的动画，防止震动叠加飞到九霄云外
            target.DOKill();
            target.DOShakePosition(duration, strength);
        }

        public void FloatingText(CanvasGroup canvasGroup, Transform target, float distance = 50f, float duration = 1f, Action onComplete = null)
        {
            canvasGroup.alpha = 1f;

            // 向上移动
            target.DOLocalMoveY(target.localPosition.y + distance, duration).SetEase(Ease.OutCubic);
            // 同时淡出
            canvasGroup.DOFade(0f, duration).SetEase(Ease.InExpo).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        #endregion
    }
}