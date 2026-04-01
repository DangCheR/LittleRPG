using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

using Object = UnityEngine.Object;
using System.Threading.Tasks;

namespace LittleRPG
{
    public class ResLoader : ICanGetUtility
    {
        private IResUtility mResUtility;

        // 这个管家私人的“借书记录”
        private HashSet<string> mLoadedKeys = new HashSet<string>();

        public ResLoader()
        {
            mResUtility = this.GetUtility<IResUtility>();
        }

        // 返回 Task<T>，外部可以爽快地 await
        public async Task<T> LoadAssetAsync<T>(string key) where T : Object
        {
            T asset = await mResUtility.LoadAssetAsync<T>(key);
            
            // 只有成功加载的才需要被卸载
            if (asset != null && !mLoadedKeys.Contains(key))
            {
                mLoadedKeys.Add(key);
            }
            return asset;
        }

        /// <summary>
        /// 简单的回调写法，适合加载后直接使用的场景（比如UI），内部会自动记账
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public void LoadAssetAsyncWithCallback<T>(string key, Action<T> callback) where T : Object
        {
            // 【极其关键】：管家必须记账！
            if (!mLoadedKeys.Contains(key))
            {
                mLoadedKeys.Add(key);
            }

            // 呼叫底层去加载
            mResUtility.LoadAssetWithCallback<T>(key, callback);
        }

        // UI 销毁时，一键归还
        public void UnloadAll()
        {
            foreach (var key in mLoadedKeys)
            {
                mResUtility.ReleaseAsset(key);
            }
            mLoadedKeys.Clear();
            Debug.Log("[ResLoader] 管家下班，清理了所有自己借出的资源！");
        }

        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}