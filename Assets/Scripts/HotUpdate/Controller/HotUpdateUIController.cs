using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace LittleRPG
{
    public class HotUpdateUIController : MonoBehaviour, IController
    {
        public Text StatusText;
        public Slider ProgressBar;

        private void Start()
        {
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
                // TODO: 在这里调用 SceneManager 加载主城场景，或者关掉 Loading 面板
                Debug.Log("热更结束，可以正式愉快地玩耍了！");
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 3. 大幕拉开，命令系统开始检查更新！
            // 假设你在 Addressables 里给需要热更的资源打了一个叫 "Remote" 的 Label
            this.GetSystem<IHotUpdateSystem>().CheckUpdateAndDownload("Remote");
        }

        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}