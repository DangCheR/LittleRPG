using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace LittleRPG
{
    public class SaveSlotUIView : MonoBehaviour
    {
        [Header("UI 绑定")]
        public TextMeshProUGUI TitleText;      // 比如 "存档槽 1"
        public TextMeshProUGUI LevelText;      // 比如 "等级: 99"
        public TextMeshProUGUI LastTimeText;       // 比如 "2026-04-02 10:00"
        public TextMeshProUGUI HasTimeText;       // 已经游玩时间，比如 "已经游玩 20小时"

        [Header("状态切换节点")]
        public Transform DataGroup;  // 有档时显示的 UI 节点

        public Button ClickButton;    // 整个条目的点击按钮

        public Button DeleteButton;   // 删除存档的按钮
        // 内部状态
        public int SlotID { get; private set; }
        public Action<int> mOnClickCallback;
        public Action<int> mOnDeleteCallback;

        // 1. 大管家赐予身份
        public void Init(int slotID)
        {
            SlotID = slotID;
            DataGroup = transform.Find("DataGroup");
            TitleText = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
            LevelText = DataGroup.Find("LevelText").GetComponent<TextMeshProUGUI>();
            LastTimeText = DataGroup.Find("LastTimeText").GetComponent<TextMeshProUGUI>();
            HasTimeText = DataGroup.Find("HasTimeText").GetComponent<TextMeshProUGUI>();
            DeleteButton = DataGroup.Find("DeleteButton").GetComponent<Button>();

            DeleteButton.gameObject.SetActive(false); // 默认先隐藏删除按钮，后续可以根据需求显示它

            ClickButton = GetComponent<Button>();
            TitleText.text = $"存档 {SlotID}";

            // 绑定 C# 原生点击事件，向管家打报告
            ClickButton.onClick.AddListener(() =>
            {
                mOnClickCallback?.Invoke(SlotID);
            });

            DeleteButton.onClick.AddListener(() =>
            {
                mOnDeleteCallback?.Invoke(SlotID);
            });
        }

        // 刷新画面
        public void UpdateView(SaveSlotMeta meta)
        {
            if (meta.IsEmpty)
            {
                LevelText.text = "点击创建新存档";
                HasTimeText.text = "";
                LastTimeText.text = "";
                DeleteButton.gameObject.SetActive(false); // 默认先隐藏删除按钮，后续可以根据需求显示它
            }
            else
            {
                LastTimeText.text = $"上次存档时间: {meta.SaveTime}";
                LevelText.text = $"玩家等级: {meta.PlayerLevel}";
                HasTimeText.text = meta.HasTime;
                DeleteButton.gameObject.SetActive(true); // 显示删除按钮
            }
        }
    }
}