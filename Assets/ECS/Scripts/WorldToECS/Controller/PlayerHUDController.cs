using UnityEngine;
using UnityEngine.UI;
using QFramework;
using TMPro;

namespace LittleRPG.Combat
{
    public class PlayerHUDController : MonoBehaviour, IController
    {
        public Slider HPSlider; // 血条 UI
        public TextMeshProUGUI text;
        private void Start()
        {
            // 监听底层发来的血量变化事件
            this.RegisterEvent<PlayerHealthChangedEvent>(e => 
            {
                // 丝滑更新 UI
                HPSlider.value = e.CurrentHP / e.MaxHP;
                text.text = $"{e.CurrentHP }/{ e.MaxHP}";
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}