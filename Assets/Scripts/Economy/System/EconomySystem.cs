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

    public class EconomySystem : AbstractSystem, IEconomySystem
    {
        private IEconomyModel mModel;

        protected override void OnInit()
        {
            mModel = this.GetModel<IEconomyModel>();
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