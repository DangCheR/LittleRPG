
using QFramework;

namespace LittleRPG
{
    class SpendEconomy : AbstractCommand
    {
        public IEconomyType currencyType;
        public int amount;
        public SpendEconomy(IEconomyType currencyType, int amount)
        {
            this.currencyType = currencyType;
            this.amount = amount;
        }

        protected override void OnExecute()
        {
            // 把脏活累活交给 System 这个大管家
            this.GetSystem<IEconomySystem>().Spend((int)currencyType, amount);
        }
    }

}