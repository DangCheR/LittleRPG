using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using QFramework;

using Object = UnityEngine.Object;
using System.Threading.Tasks;

namespace LittleRPG
{
    public interface IResUtility : IUtility
    {
        void ReleaseAsset(string key);
        // 注意：回调直接返回 T，不暴露底层的 Handle，让上层更舒服
        void LoadAssetWithCallback<T>(string key, Action<T> callback) where T : Object;
        Task<T> LoadAssetAsync<T>(string key) where T : Object;
    }

    public class AddressablesUtility : IResUtility
    {
private class AssetState
        {
            public AsyncOperationHandle Handle;
            public int RefCount;
        }

        private Dictionary<string, AssetState> mAssetCache = new Dictionary<string, AssetState>();

        public async Task<T> LoadAssetAsync<T>(string key) where T : Object
        {
            // 1. 缓存秒返（最爽的地方）
            if (mAssetCache.TryGetValue(key, out var state))
            {
                state.RefCount++;
                if (!state.Handle.IsDone) await state.Handle.Task;
                return state.Handle.Result as T;
            }

            // 2. 真实加载（穿上防弹衣）
            AsyncOperationHandle<T> handle = default;
            try
            {
                handle = Addressables.LoadAssetAsync<T>(key);
                // 立刻记账，防止同一帧并发请求
                mAssetCache[key] = new AssetState { Handle = handle, RefCount = 1 };

                await handle.Task; // 等待加载完成

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }
                else
                {
                    Debug.LogWarning($"[ResUtility] 资源加载失败 (可能不存在): {key}");
                }
            }
            catch (Exception e)
            {
                // 【核心防御】：任何异常（包括 InvalidKeyException）都在这里被拦截！
                Debug.LogError($"[ResUtility] 致命异常被拦截，Key: {key}\n错误: {e.Message}");
            }

            // 3. 走到这里说明加载失败/异常了，清理垃圾账本并返回 null
            if (mAssetCache.ContainsKey(key))
            {
                mAssetCache.Remove(key);
            }
            return null;
        }

        /// <summary>
        /// 安全的回调写法，用于加载后简单处理
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public void LoadAssetWithCallback<T>(string key, Action<T> callback) where T : Object
        {
            // 1. 如果缓存里有，直接复用！
            if (mAssetCache.TryGetValue(key, out var state))
            {
                state.RefCount++;
                
                // Addressables 的 Completed 事件有个很好的特性：
                // 如果它已经加载完了，你绑定事件的瞬间，它就会立刻执行回调！
                state.Handle.Completed += (AsyncOperationHandle handle) => 
                {
                    callback?.Invoke(handle.Result as T);
                };
                return;
            }

            // 2. 如果缓存里没有，发起真实的加载
            AsyncOperationHandle<T> newHandle = Addressables.LoadAssetAsync<T>(key);
            
            // 【极其关键】：一定要记在总账本上！
            mAssetCache[key] = new AssetState { Handle = newHandle, RefCount = 1 };

            newHandle.Completed += (handle) => 
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    callback?.Invoke(handle.Result);
                }
                else
                {
                    Debug.LogError($"[ResUtility] 加载失败，Key: {key}");
                    mAssetCache.Remove(key); // 失败了要清理垃圾账
                    callback?.Invoke(null);  // 即使失败，也要告诉调用者，别让他死等
                }
            };
        }

        public void ReleaseAsset(string key)
        {
            if (mAssetCache.TryGetValue(key, out var state))
            {
                state.RefCount--;
                // 只有借阅人数归 0，才真正卸载
                if (state.RefCount <= 0)
                {
                    Addressables.Release(state.Handle);
                    mAssetCache.Remove(key);
                    Debug.Log($"[ResUtility] 资源 {key} 彻底卸载！");
                }
            }
        }
    }
}