using QFramework;
using UnityEngine;

namespace LittleRPG
{


    public interface IEconomySystem : ISystem
    {
        bool HasEnough(int currencyID, long amount);
        void Earn(int currencyID, long amount);
        bool Spend(int currencyID, long amount);
    }

    public class EconomySystem : AbstractSystem, IEconomySystem, ISaveHandler
    {
        /// <summary>
        /// 用于存档路径
        /// </summary>
        /// <value></value>
        public string SaveFileName { get; } = "economy.es3";

        private IEconomyModel mModel;

        protected override void OnInit()
        {
            this.GetSystem<ISaveSystem>().RegisterSaveHandler(this);
            mModel = this.GetModel<IEconomyModel>();
        }

        public void OnSave(ISaveUtility saveUtil, string folderPath)
        {
            string filePath = folderPath + SaveFileName;

            foreach (var kvp in mModel.Currencies)
            {
                // 存储每种货币的数量，key 就是货币 ID，value 就是数量
                saveUtil.Save($"Currency_{(int)kvp.Key}", kvp.Value.Value, filePath);
            }
        }

        public void OnLoad(ISaveUtility saveUtil, string folderPath)
        {
            string filePath = folderPath + SaveFileName;
            if (!saveUtil.HasFile(filePath))
            {
                this.NewSave(saveUtil, folderPath); // 没有存档，创建一个新的存档
                return;
            }

            // 直接遍历枚举里定义的所有货币类型，逐个加载它们的数量！这样以后加新货币，不需要改这里了！
            foreach (var currencyID in System.Enum.GetValues(typeof(IEconomyType)))
            {
                long savedAmount = saveUtil.Load($"Currency_{(int)currencyID}", 0L, filePath);
                mModel.GetCurrency((IEconomyType)currencyID).Value = savedAmount; // 确保每种货币的属性都被初始化了
            }
        }

        public void NewSave(ISaveUtility saveUtil, string folderPath)
        {
            mModel.GetCurrency(IEconomyType.Gold).Value = 1000;
            mModel.GetCurrency(IEconomyType.Gem).Value = 99;

            string filePath = folderPath + SaveFileName;

            if (saveUtil.HasFile(filePath))
            {
                saveUtil.DeleteFile(filePath);
            }

            // 3. 执行一次强行存档，把新创建的文件夹盖下去
            this.OnSave(saveUtil, folderPath);
        }

        public void OnDelete(ISaveUtility saveUtil, string folderPath)
        {
            string filePath = folderPath + SaveFileName;
            if (saveUtil.HasFile(filePath))
            {
                saveUtil.DeleteFile(filePath);
            }
        }

        // 查账
        public bool HasEnough(int currencyID, long amount)
        {
            return mModel.GetCurrency((IEconomyType)currencyID).Value >= amount;
        }

        // 赚钱
        public void Earn(int currencyID, long amount)
        {
            if (amount <= 0) return;
            mModel.GetCurrency((IEconomyType)currencyID).Value += amount;
        }

        // 消费 (扣钱)
        public bool Spend(int currencyID, long amount)
        {
            if (amount <= 0) return true;

            var currencyProp = mModel.GetCurrency((IEconomyType)currencyID);
            if (currencyProp.Value >= amount)
            {
                currencyProp.Value -= amount; // 扣除！
                Debug.Log($"消耗了 {amount} 的货币[{currencyID}]，当前余额: {currencyProp.Value}");
                return true;
            }

            Debug.LogWarning($"货币[{currencyID}]不足！需要 {amount}，只有 {currencyProp.Value}");
            return false;
        }
    }
}