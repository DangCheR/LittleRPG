using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using LittleRPG;
public class LittleRPGArchitecture : Architecture<LittleRPGArchitecture>
{
    protected override void Init()
    {
        GameObject dontDestory = GameObject.Find("DontDestory");
        #region Model
        //加载模块
        this.RegisterModel(new LoadSceneModel());
        this.RegisterModel(new PlayerDataModel());
        //移速模块
        this.RegisterModel(new MoveSpeedModel());
        //物Item模块
        this.RegisterModel(new IItmeModel());
        //等级奖励模块
        this.RegisterModel(new RewardManagerModel());
        //购买消耗品模块
        this.RegisterModel(new StoreItemModel());
        //任务模块
        this.RegisterModel(new TaskManagerModel());
        //已接任务模块
        this.RegisterModel(new TaskDetailsModel());
        #region 子
        this.RegisterModel(new HuoLiMoModel());
        this.RegisterModel(new MoLiMoModel());
        this.RegisterModel(new XiangXunCaoModel());
        this.RegisterModel(new HpModel());
        this.RegisterModel(new MpModel());
        this.RegisterModel(new MeatModel());
        #endregion
        //背包模块
        this.RegisterModel(new BackGroundManagerModel());
        #endregion

        #region System
        this.RegisterSystem(new HuoLiMoSystem());
        this.RegisterSystem(new MoLiMoSystem());
        this.RegisterSystem(new XiangXunCaoSystem());
        this.RegisterSystem(new HpSystem());
        this.RegisterSystem(new MpSystem());
        this.RegisterSystem(new MeatSystem());
        #endregion

        #region Utility
        //协程工具
        CoroutineUtility coroutineUtility = dontDestory.AddComponent<CoroutineUtility>();
        this.RegisterUtility(coroutineUtility);

        SaveDataUtility saveDataUtility = dontDestory.AddComponent<SaveDataUtility>();
        this.RegisterUtility(saveDataUtility);

        #endregion
        /// <summary>
        /// 音频
        /// </summary>
        /// <param name="AudioModel()"></param>
        /// <typeparam name="IAudioModel"></typeparam>
        this.RegisterModel<IAudioModel>(new AudioModel());
        this.RegisterSystem<IAudioSystem>(new AudioSystem());

        /// <summary>
        /// 背包相关
        /// </summary>
        /// <param name="InventoryModel()"></param>
        /// <typeparam name="IInventoryModel"></typeparam>
        this.RegisterModel<IInventoryModel>(new InventoryModel()); // 注册玩家背包
        this.RegisterModel<ItemTableModel>(new ItemTableModel()); // 注册对照表
        this.RegisterSystem<IInventorySystem>(new InventorySystem()); //注册system
    }
}
