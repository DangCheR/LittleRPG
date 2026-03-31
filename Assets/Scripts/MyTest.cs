using UnityEngine;
using QFramework;

namespace LittleRPG
{
    class MyTest : MonoBehaviour,
        ICanSendEvent,
        ICanSendCommand
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("按下了A键");
                this.SendEvent<InventoryItemChangedEvent>(new InventoryItemChangedEvent
                {
                    ItemID = 1,
                    ItemCount = 20
                });
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("按下了B键");
                this.SendCommand<SpendEconomy>(new SpendEconomy(IEconomyType.Gold, 100));
            }
        }
        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
        // 这个类用来测试一些东西，最后会被删掉
    }
}