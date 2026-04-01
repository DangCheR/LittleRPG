namespace LittleRPG
{
    // 热更状态提示事件 (用于在 UI 上显示 "正在检查更新...", "正在下载 45%...")
    public struct HotUpdateProgressEvent
    {
        public string StatusText; // 状态描述
        public float Progress;    // 进度 0~1
    }

    // 发现新版本事件 (弹出框：发现新版本 50MB，是否更新？)
    public struct HotUpdateFoundEvent
    {
        public long DownloadSizeByte; // 需要下载的字节数
    }

    // 热更彻底完成事件 (可以进入游戏了！)
    public struct HotUpdateCompleteEvent { }
}