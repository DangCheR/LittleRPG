using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.Example
{

    //使用依赖倒置
    interface IMyCountModel : IModel
    {
        BindableProperty<int> Count
        {
            get;
        }
    }
    //定义一个model
    class MyCountModel : AbstractModel, IMyCountModel
    {
        public BindableProperty<int> Count { get; } = new();

        protected override void OnInit()
        {
            var storage = this.GetUtility<MyPlayerPrefsStorage>();

            //初始化无需事件
            Count.SetValueWithoutEvent(storage.LoadInt(nameof(Count)));

            //直接注册修改时事件
            Count.Register(newCount =>
            {
                storage.SaveInt(nameof(Count), newCount);
            });
        }
    }

    //定义存储INT接口，依赖倒置
    interface IStorageInt : IUtility
    {
        void SaveInt(string key, int newValue);
        int LoadInt(string key, int defaultValue = 0);
    }

    //定义一个存储类
    class MyPlayerPrefsStorage : IStorageInt
    {
        public void SaveInt(string key, int newValue)
        {
            PlayerPrefs.SetInt(key, newValue);
        }

        public int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
    }

    class MyEasySaveStorage : IStorageInt
    {
        public string save_path = "SaveFile.es3";

        public void SaveInt(string key, int newValue)
        {
            ES3.Save(key, newValue);
        }

        public int LoadInt(string key, int defaultValue = 0)
        {
            return ES3.Load(key, save_path, defaultValue);
        }
    }
    //定义一个架构
    //注册model
    class MyCountArchitecture : Architecture<MyCountArchitecture>
    {
        protected override void Init()
        {
            this.RegisterSystem<MyAchievementSystem>(new());
            this.RegisterModel<MyCountModel>(new());
            this.RegisterUtility<MyPlayerPrefsStorage>(new());
        }

        protected override void ExecuteCommand(ICommand command)
        {
            LogKit.I("命令被拦截");
            base.ExecuteCommand(command);
            LogKit.I("命令执行结束");
        }

    }

    //使用命令模式
    //AbstractCommand引用了IBelongToArchitecture，可以获取架构，所以可以获取GetModel
    class AddCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<MyCountModel>().Count.Value++;
            this.SendEvent<MyChangeModelEvent>();
        }
    }

    class SubCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<MyCountModel>().Count.Value--;
            this.SendEvent<MyChangeModelEvent>();
        }
    }

    // 成就系统，依旧监听model Count修改的事件
    class MyAchievementSystem : AbstractSystem
    {
        protected override void OnInit()
        {
            var model = this.GetModel<MyCountModel>();

            model.Count.Register(newCount =>
            {
                if (newCount == 10)
                {
                    LogKit.I("10");
                }
                else if (newCount == 20)
                {
                    LogKit.I("20");
                }
                else if (newCount == -10)
                {
                    LogKit.I("-10");
                }
            });
        }
    }
    //使用事件来触发view的改变
    struct MyChangeModelEvent
    {

    }
    //定义一个管理
    class MyCounterAppController : MonoBehaviour, IController
    {
        private Button mBtnAdd;
        private Button mBtnSub;
        private Text mCountText;

        MyCountModel model;

        void Start()
        {
            // 实际上是依赖GetArchitecture的GetModel
            model = this.GetModel<MyCountModel>();
            mBtnAdd = transform.Find("BtnAdd").GetComponent<Button>();
            mBtnSub = transform.Find("BtnSub").GetComponent<Button>();
            mCountText = transform.Find("CountText").GetComponent<Text>();

            mBtnAdd.onClick.AddListener(() =>
            {
                this.SendCommand<AddCommand>();
            });

            mBtnSub.onClick.AddListener(() =>
            {
                this.SendCommand<SubCommand>();
            });

            //注册事件
            IUnRegister ev = this.RegisterEvent<MyChangeModelEvent>(e =>
            {
                UpdateView();
            });

            ev.UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        public IArchitecture GetArchitecture()
        {
            return MyCountArchitecture.Interface;
        }

        void UpdateView()
        {
            mCountText.text = model.Count.ToString();
        }
        void OnDestroy()
        {
            model = null;
        }
    }
}
