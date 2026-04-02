namespace LittleRPG
{
    /// <summary>
    /// 存档协议：任何需要存档的模块，都要实现这个接口
    /// </summary>
    public interface ISaveHandler
    {
        public string SaveFileName { get; } // 存档文件夹的名字（比如“Slot1”），存档系统会自动在 Application.persistentDataPath 下创建这个文件夹来存储这个模块的存档数据
        // 告诉存档系统，保存的时候执行什么？
        void OnSave(ISaveUtility saveUtil, string folderPath);
        
        // 告诉存档系统，读取的时候执行什么？
        void OnLoad(ISaveUtility saveUtil, string folderPath);

        // 告诉存档系统，初始化时或存档丢失
        void NewSave(ISaveUtility saveUtil, string folderPath);

        void OnDelete(ISaveUtility saveUtil, string folderPath); // 删除存档时需要执行的操作
    }
}