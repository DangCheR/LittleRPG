using UnityEngine;
using UnityEngine.UI;
using QFramework;
using TMPro;
using System.Collections.Generic;

namespace LittleRPG
{

    public class EconomyUIController : MonoBehaviour, IController
    {
        [Header("UI 文本组件")]
        public Image GoldIcon;
        public Image GemIcon;
        public TextMeshProUGUI GoldText;
        public TextMeshProUGUI GemText;

        private void Awake()
        {
            var EconomyUI = GameObject.Find("MyCanvas").transform.Find("EconomyPanel");
            var gold = EconomyUI.Find("GoldBG");
            var diamond = EconomyUI.Find("GemBG");
            GoldIcon = gold.Find("CoinImage").GetComponent<Image>();
            GoldText = gold.Find("CoinNum").GetComponent<TextMeshProUGUI>();
            GemIcon = diamond.Find("CoinImage").GetComponent<Image>();
            GemText = diamond.Find("CoinNum").GetComponent<TextMeshProUGUI>();

            // 假装我们有读取配置的能力，先手动绑定一下
            GoldIcon.sprite = Resources.Load<Sprite>("Sprites/Economy/Gold");
            GemIcon.sprite = Resources.Load<Sprite>("Sprites/Economy/Gem");

        }
        private void Start()
        {
            var ecoModel = this.GetModel<IEconomyModel>();

            // 【极致优雅的单向数据绑定】
            // RegisterWithInitValue 会立刻执行一次，并且以后每次数值变化都会自动执行！

            // 绑定金币
            ecoModel.GetCurrency(IEconomyType.Gold).RegisterWithInitValue(gold =>
            {
                GoldText.text = gold.ToString();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 绑定钻石
            ecoModel.GetCurrency(IEconomyType.Gem).RegisterWithInitValue(diamond =>
            {
                GemText.text = diamond.ToString();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}