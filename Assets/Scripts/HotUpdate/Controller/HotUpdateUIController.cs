using UnityEngine;
using UnityEngine.UI;
using QFramework;
using TMPro;

namespace LittleRPG
{
    public class HotUpdateUIController : MonoBehaviour, IController
    {
        public RectTransform HotUpdatePanel; // 下载面板，包含进度条和状态文本
        public TextMeshProUGUI StatusText; // 用于显示 "正在检查更新...", "正在下载 45%..." 这种状态信息
        public Slider ProgressBar; // 进度条，显示下载进度

        public TextMeshProUGUI MaxText; // 这个是可选的，如果你想在 UI 上显示 "正在下载 45MB / 100MB" 这种信息的话，可以用上
        public TextMeshProUGUI CurrText; // 这个也是可选的，配合 MaxText 显示当前已经下载了多少

        public IHotUpdateModel mModel;
        private void Start()
        {
            this.SendCommand(new CheckUpdateCommand()); // 发送命令，开始热更新流程

            HotUpdatePanel = GameObject.Find("Canvas/HotUpdatePanel").GetComponent<RectTransform>();
            StatusText = HotUpdatePanel.Find("TipText").GetComponent<TextMeshProUGUI>();
            ProgressBar = HotUpdatePanel.Find("ProgressBar").GetComponent<Slider>();
            MaxText = HotUpdatePanel.Find("MaxText").GetComponent<TextMeshProUGUI>();
            CurrText = HotUpdatePanel.Find("CurrText").GetComponent<TextMeshProUGUI>();
            mModel = this.GetModel<IHotUpdateModel>();
            mModel.tipText.Register( (text) => StatusText.text = text).UnRegisterWhenGameObjectDestroyed(gameObject);
            mModel.progress.Register( (value) => ProgressBar.value = value).UnRegisterWhenGameObjectDestroyed(gameObject);
            mModel.Max.Register( (value) => MaxText.text = value.ToString()).UnRegisterWhenGameObjectDestroyed(gameObject);
            mModel.Curr.Register( (value) => CurrText.text = value.ToString()).UnRegisterWhenGameObjectDestroyed(gameObject);
            // 1. 监听进度更新
            this.RegisterEvent<HotUpdateProgressEvent>(e =>
            {
                StatusText.text = e.StatusText;
                ProgressBar.value = e.Progress;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 2. 监听完成事件
            this.RegisterEvent<HotUpdateCompleteEvent>(e =>
            {
                StatusText.text = "进入游戏！";
                HotUpdatePanel.gameObject.SetActive(false);
                // TODO: 在这里调用 SceneManager 加载主城场景，或者关掉 Loading 面板
                Debug.Log("热更结束，可以正式愉快地玩耍了！");
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}