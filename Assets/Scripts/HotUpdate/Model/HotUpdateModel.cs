using UnityEngine.ResourceManagement.AsyncOperations;
using QFramework;

namespace LittleRPG
{
    public interface IHotUpdateModel : IModel
    {
        public BindableProperty<string> tipText { get; }
        public BindableProperty<float> progress { get; }
        public BindableProperty<float> Max { get; }
        public BindableProperty<float> Curr { get; }
        // 这里你可以定义一些全局的热更新状态数据，比如当前版本号、下载进度等
    }

    public class HotUpdateModel : AbstractModel, IHotUpdateModel
    {
        public BindableProperty<string> tipText { get; }
        public BindableProperty<float> progress { get; }
        public BindableProperty<float> Max { get; }
        public BindableProperty<float> Curr { get; }
        public HotUpdateModel()
        {
            tipText = new BindableProperty<string>("正在检查更新...");
            progress = new BindableProperty<float>(0f);
            Max = new BindableProperty<float>(100f);
            Curr = new BindableProperty<float>(0f);
        }
        protected override void OnInit() { }
    }
}