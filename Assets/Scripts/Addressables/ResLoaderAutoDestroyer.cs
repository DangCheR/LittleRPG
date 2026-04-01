using UnityEngine;

namespace LittleRPG
{
    // 这是一个挂在 GameObject 上的隐藏脚本，用来在物体销毁时触发回收
    public class ResLoaderAutoDestroyer : MonoBehaviour
    {
        public ResLoader Loader;

        private void OnDestroy()
        {
            if (Loader != null)
            {
                Loader.UnloadAll(); // 物体死，资源跟着卸载！
            }
        }
    }

    public static class ResLoaderExtension
    {
        // 拓展方法：给任意 GameObject 分配一个自动回收的管家
        public static ResLoader AutoReleaseTo(this ResLoader loader, GameObject gameObject)
        {
            var destroyer = gameObject.AddComponent<ResLoaderAutoDestroyer>();
            destroyer.Loader = loader;
            return loader;
        }
    }
}