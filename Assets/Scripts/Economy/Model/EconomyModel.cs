using System.Collections.Generic;
using QFramework;

namespace LittleRPG
{
    public enum IEconomyType
    {
        Gold = 1,
        Gem = 2,
        Crystal = 3 // 预留的水晶
    }

    public interface IEconomyModel : IModel
    {
        // 核心：用字典存储所有的货币！Key=货币ID, Value=响应式数值
        Dictionary<IEconomyType, BindableProperty<long>> Currencies { get; }

        // 快捷获取某种货币的响应式属性（如果没注册过，自动初始化为 0）
        BindableProperty<long> GetCurrency(IEconomyType currencyType);
    }

    public class EconomyModel : AbstractModel, IEconomyModel
    {
        public Dictionary<IEconomyType, BindableProperty<long>> Currencies { get; private set; }

        protected override void OnInit()
        {
            Currencies = new Dictionary<IEconomyType, BindableProperty<long>>();

            // 初始化开局默认资产 (比如给 1000 金币，0 宝石)
            GetCurrency(IEconomyType.Gold).Value = 1000;
            GetCurrency(IEconomyType.Gem).Value = 99;
        }

        public BindableProperty<long> GetCurrency(IEconomyType currencyType)
        {
            // 如果字典里没有这种货币，动态帮它建一个初始值为 0 的属性！
            // 这就是极其强大的拓展性：以后加“水晶”，不需要改 Model！
            if (!Currencies.ContainsKey(currencyType))
            {
                Currencies.Add(currencyType, new BindableProperty<long>(0));
            }
            return Currencies[currencyType];
        }
    }
}