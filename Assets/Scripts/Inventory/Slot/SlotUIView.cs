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
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        ICanSendEvent
    {
        [Header("UI 绑定")]
        public Image IconImage;
        public TextMeshProUGUI CountText;
        // public GameObject IconRoot; // 拖拽的本体

        public int SlotIndex { get; set; }

        public void Init(int index)
        {
            SlotIndex = index;
        }

        /// <summary>
        /// 初始化组件加载
        /// </summary>
        private void InitComponent()
        {
            IconImage = transform.Find("ItemIcon").GetComponent<Image>();
            if (IconImage == null)
            {
                Debug.Log("槽位的IconImage不存在");
            }
            // 获取子物体显示数量的组件
            CountText = transform.Find("ItemCount").GetComponent<TextMeshProUGUI>();
            if (CountText == null)
            {
                Debug.Log("槽位的IconImage不存在");
            }
        }

        #region 提供方法由总controller调用来刷新画面

        /// <summary>
        /// 修改图标
        /// </summary>
        /// <param name="icon"></param>
        public void UpdateIcon(Sprite icon = null)
        {
            if (IconImage == null)
            {
                InitComponent();
            }
            Debug.Log("被操作了");
            if (icon == null)
            {
                IconImage.enabled = false;
                IconImage.color = new Color(0, 0, 0, 0);
                CountText.text = "";
            }
            else
            {
                IconImage.enabled = true;
                IconImage.color = new Color(255, 255, 255, 1);
                IconImage.sprite = icon;
            }
        }

        /// <summary>
        /// 修改数量
        /// </summary>
        /// <param name="count"></param>
        public void UpdateCount(int count = 0)
        {
            if (CountText == null)
            {
                InitComponent();
            }

            if (count == 0)
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
            // 比如：发送使用物品的指令
            // this.SendCommand(new UseItemCommand(SlotIndex));
            Debug.Log($"点击了格子：{SlotIndex}");
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("开始拖拽");
            /* 视觉效果：图标放大或变半透明 */
        }
        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("拖拽中");
            /* 视觉效果：图标跟随鼠标 */
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("拖拽放下");
            /* 视觉效果：图标归位 */
        }

        public void OnDrop(PointerEventData eventData)
        {
            var sourceSlot = eventData.pointerDrag
                ?.GetComponentInParent<SlotUIView>();

            if (sourceSlot == null) return;
            if (sourceSlot == this) return;

            this.SendEvent(new SlotChangeEvent(
                // sourceSlot.BelongInventory,
                sourceSlot.SlotIndex,
                // this.BelongInventory,
                this.SlotIndex
            ));
        }

        // 必须实现此接口，证明自己是 LittleRPG 架构体系内的人
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}