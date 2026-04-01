using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 用于拖拽
using System;
using QFramework;
using TMPro;

namespace LittleRPG
{
    /// <summary>
    /// 只是一个 UI层面View
    /// 只负责显示，不管数据
    /// </summary>
    public class SlotUIView : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        [Header("UI 绑定")]
        public Image IconImage;
        public TextMeshProUGUI CountText;
        private CanvasGroup draggingCanvasGroup; //控制子物体的射线
        public bool HasItem { get; set; } // 是否能拖动
        public bool IsLock { get; set; } // 是否锁定
        public int SlotIndex { get; set; } // 这个格子在背包中的index，由外部初始化时设置
        RectTransform rect;
        private RectTransform mDragLayer; // 拖拽时放在这里
        private RectTransform Dragging;// 拖拽时移动的一堆东西的集合

        //事件
        public event Action<int> OnSlotClickEvent; // 点击格子时发送事件，参数是格子index
        public event Action<int, int> OnSlotDroppedEvent; // 被放下
        public event Action<SlotUIView> OnDragFailedEvent; // 拖到无效区域
        public event Action<SlotUIView> OnSlotHoverEnterEvent; // 悬停进入
        public event Action<SlotUIView> OnSlotHoverExitEvent; // 悬停退出

        public void Init(int index)
        {
            SlotIndex = index;
        }

        /// <summary>
        /// 初始化组件加载
        /// </summary>
        private void InitComponent()
        {
            rect = GetComponent<RectTransform>();
            // Dragging = rect.Find("Dragging");
            Dragging = rect.Find("Dragging").GetComponent<RectTransform>();

            IconImage = Dragging.Find("ItemIcon").GetComponent<Image>();
            // 获取子物体显示数量的组件
            CountText = Dragging.Find("ItemCount").GetComponent<TextMeshProUGUI>();

            draggingCanvasGroup = Dragging.GetComponent<CanvasGroup>();
            draggingCanvasGroup.blocksRaycasts = true; // 默认不阻挡射线，只有在拖动时才阻挡
            mDragLayer = GameObject.Find("MyCanvas").GetComponent<RectTransform>();
        }

        #region 提供方法由总controller调用来刷新画面

        /// <summary>
        /// 修改图标
        /// </summary>
        /// <param name="icon"></param>
        public void UpdateIcon(Sprite icon = null)
        {
            if (IconImage == null) InitComponent();

            if (icon == null)
            {
                IconImage.enabled = false;
                IconImage.color = new Color(0, 0, 0, 0);
                CountText.text = "";
                HasItem = false; // 如果设置了空图标一定不能拖动
            }
            else
            {
                IconImage.enabled = true;
                IconImage.color = new Color(1, 1, 1, 1);
                IconImage.sprite = icon;
                HasItem = true; // 如果设置了图标一定能拖动
            }
        }

        /// <summary>
        /// 修改数量
        /// </summary>
        /// <param name="count"></param>
        public void UpdateCount(int count = 0)
        {
            if (CountText == null) InitComponent();

            if (count == 0)
            {
                CountText.text = "";
                HasItem = false; // 如果设置了空图标一定不能拖动
            }
            else if (count == 1)
            {
                CountText.text = "";
            }
            else
            {
                CountText.text = count.ToString();
            }
        }

        #endregion

        // --- 物理交互直接发送 Command
        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClickEvent?.Invoke(SlotIndex);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!HasItem)
            {
                Debug.Log("别拖了，没东西");
                return;
            }
            draggingCanvasGroup.blocksRaycasts = false;

            // 拖拽时设置父级为顶级
            Dragging.SetParent(mDragLayer, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!HasItem) return;
            Dragging.transform.position = eventData.position;
        }

        /// <summary>
        /// 拖拽结束时,发送事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            // 拖到UI外pointerEnter都是null，我操了
            SlotUIView enter = eventData.pointerEnter != null
                ? eventData.pointerEnter.GetComponent<SlotUIView>()
                : null;

            // 交给controller去处理飞回去的动画，或者丢地上的逻辑
            if (enter == null || enter == this)
            {
                Debug.Log("拖到了无效区域，物品归位");
                OnDragFailedEvent?.Invoke(this);
                return;
            }
        }

        /// <summary>
        /// 被拖动物体放到当前槽位时,发送事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData)
        {
            var sourceSlot = eventData.pointerDrag
                ?.GetComponentInParent<SlotUIView>();

            if (sourceSlot == null) return;
            if (sourceSlot == this) return;
            Debug.Log("被放下了sourceIndex: " + sourceSlot.SlotIndex + " targetIndex: " + SlotIndex);
            OnSlotDroppedEvent?.Invoke(sourceSlot.SlotIndex, SlotIndex);
            // this.SendCommand(new MoveItemCommand(sourceSlot.SlotIndex, SlotIndex));
        }

        /// <summary>
        /// 将拖拽的物品放到当前槽位
        /// </summary>
        public void DraggingToSlot()
        {
            Dragging.SetParent(transform);
            Dragging.localPosition = Vector2.zero;

            draggingCanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 播放飞回动画
        /// 指定一个起始坐标，例如从另一个格子飞过来，或者从地面飞过来
        /// </summary>
        /// <param name="startWorldPos"></param>
        /// <param name="tweenUtil"></param>
        public void PlayFlyBackAnimation(ITweenUtility tweenUtil, Vector3 startWorldPos = default(Vector3))
        {
            if (Dragging == null) return;

            // 为了让飞行的物体在最上层，短暂认一下干爹
            Dragging.SetParent(mDragLayer);
            Dragging.SetAsLastSibling();

            // 1. 强行把肉身移动到指定的起始世界坐标（比如别人的老家）
            if (startWorldPos != default(Vector3))
            {
                Dragging.position = startWorldPos;
            }

            // 3. 此时它的 LocalPosition 是非常大的偏移，让它自己飞回 0,0
            tweenUtil.UIFlyToTarget(Dragging, transform.position, 0.25f, () =>
            {
                if (Dragging == null)
                {
                    Debug.Log("飞回动画结束了，但拖拽物体已经没了，可能是被销毁了");
                    return;
                }
                // 比如重新开启射线阻挡，或者把父级改回格子
                Dragging.SetParent(transform as RectTransform);
                draggingCanvasGroup.blocksRaycasts = true;
            });
        }


        /// <summary>
        /// 挤到右上角
        /// </summary>
        public void PlayPushRightTop(ITweenUtility tweenUtil, float offset = 30f, float duration = 0.2f)
        {
            Vector3 target = Dragging.position + new Vector3(offset, offset);

            tweenUtil.UIFlyToTarget(Dragging, target, 0.2f);
        }

        /// <summary>
        /// 回到原位
        /// </summary>
        public void PlayRecover(ITweenUtility tweenUtil, float offset = 30f, float duration = 0.2f)
        {
            Vector3 target = transform.position;

            tweenUtil.UIFlyToTarget(Dragging, target, 0.2f);
        }

        /// <summary>
        /// 鼠标进入事件
        /// 用于处理被挤开动画
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!HasItem) return;
            // 1. 判断现在是不是有人正在拖拽东西？
            var draggingObj = eventData.pointerDrag?.GetComponent<SlotUIView>();

            // 2. 如果有人在拖东西，而且拖的不是我自己，且我这个格子不是空的
            if (draggingObj != null && draggingObj != this)
            {
                // 打小报告：老大！有人拿着东西悬停在我头上了！快给我播个被挤开的动画！
                // 把我自己的 Dragging (图标层) 传给老大去缩放
                OnSlotHoverEnterEvent?.Invoke(this);
            }
        }

        /// <summary>
        /// 鼠标从自己身上离开事件
        /// 用于处理被挤开后复位动画
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!HasItem) return;

            var draggingObj = eventData.pointerDrag?.GetComponent<SlotUIView>();

            if (draggingObj != null && draggingObj != this)
            {
                // 打小报告：老大！那个人走了！快把我恢复原状！
                OnSlotHoverExitEvent?.Invoke(this);
            }
        }

        public Vector3 GetDraggingWorldPosition()
        {
            return Dragging.position;
        }
    }
}