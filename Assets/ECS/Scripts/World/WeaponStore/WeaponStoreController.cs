using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace LittleRPG.Combat
{
    public class WeaponStoreController : MonoBehaviour, IController
    {
        public Button boxBtn;
        public Button swordBtn;

        private void Start()
        {
            boxBtn.onClick.AddListener(() =>
            {
                // UI 只管发枚举 ID！
                this.SendCommand(new SelectWeaponCommand(WeaponType.Box));
            });

            swordBtn.onClick.AddListener(() =>
            {
                this.SendCommand(new SelectWeaponCommand(WeaponType.Sword));
            });
        }

        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}