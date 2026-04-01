using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using QFramework;

namespace LittleRPG
{
    public interface IHotUpdateSystem : ISystem
    {
        // 开始检查更新流程 (传入一个你需要更新的标签，默认是 default)
        void CheckUpdateAndDownload(string label = "default");
    }

    public class HotUpdateSystem : AbstractSystem, IHotUpdateSystem
    {
        protected override void OnInit() { }

        public async void CheckUpdateAndDownload(string label = "default")
        {
            try
            {
                // 1. 初始化 AA 系统
                this.SendEvent(new HotUpdateProgressEvent { StatusText = "正在初始化资源系统...", Progress = 0f });
                await Addressables.InitializeAsync().Task;

                // 2. 检查 Catalog (目录) 是否有更新
                this.SendEvent(new HotUpdateProgressEvent { StatusText = "正在检查更新...", Progress = 0.1f });
                List<string> catalogsToUpdate = await Addressables.CheckForCatalogUpdates(false).Task;

                if (catalogsToUpdate.Count > 0)
                {
                    // 3. 有更新！下载最新的 Catalog 目录
                    this.SendEvent(new HotUpdateProgressEvent { StatusText = "正在拉取最新版本信息...", Progress = 0.2f });
                    await Addressables.UpdateCatalogs(catalogsToUpdate, false).Task;
                }

                // 4. 检查具体需要下载的资源包大小
                this.SendEvent(new HotUpdateProgressEvent { StatusText = "正在计算下载大小...", Progress = 0.3f });
                
                // 这里传入 label (比如你可以给所有远端资源打上 "Remote" 标签)
                long totalDownloadSize = await Addressables.GetDownloadSizeAsync(label).Task;

                if (totalDownloadSize > 0)
                {
                    // 发现新资源！【这里你可以发送事件给 UI，让玩家点确认。为了演示，我们直接开始下】
                    Debug.Log($"[HotUpdate] 发现新资源，大小: {totalDownloadSize / 1048576f:F2} MB");
                    // this.SendEvent(new HotUpdateFoundEvent { DownloadSizeByte = totalDownloadSize });

                    await DownloadAssetsAsync(label);
                }
                else
                {
                    Debug.Log("[HotUpdate] 已经是最新版本，无需下载！");
                    this.SendEvent(new HotUpdateCompleteEvent());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HotUpdate] 热更新过程发生致命错误: {e.Message}");
                this.SendEvent(new HotUpdateProgressEvent { StatusText = "网络错误，请重试！", Progress = 0f });
            }
        }

        // 核心下载逻辑
        private async Task DownloadAssetsAsync(string label)
        {
            this.SendEvent(new HotUpdateProgressEvent { StatusText = "正在下载资源...", Progress = 0.3f });

            // 发起下载请求
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(label, false);

            // 监听下载进度
            while (!downloadHandle.IsDone)
            {
                float percent = downloadHandle.PercentComplete;
                this.SendEvent(new HotUpdateProgressEvent 
                { 
                    StatusText = $"正在下载资源 ({(percent * 100):F1}%)...", 
                    Progress = 0.3f + (percent * 0.7f) // 映射到 30% ~ 100%
                });

                // 挂起一帧，防止死循环卡死主线程
                await Task.Yield(); 
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("[HotUpdate] 下载完成！");
                
                // 【重要】释放下载句柄
                Addressables.Release(downloadHandle);

                this.SendEvent(new HotUpdateProgressEvent { StatusText = "下载完成，准备进入游戏...", Progress = 1f });
                this.SendEvent(new HotUpdateCompleteEvent());
            }
            else
            {
                throw new Exception("下载过程失败！");
            }
        }
    }
}