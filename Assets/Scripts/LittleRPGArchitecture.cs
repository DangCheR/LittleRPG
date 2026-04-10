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
        // #region Model
        //加载模块
        // this.RegisterModel(new LoadSceneModel());
        // this.RegisterModel(new PlayerDataModel());
        // //移速模块
        // this.RegisterModel(new MoveSpeedModel());
        // //物Item模块
        // this.RegisterModel(new IItmeModel());
        // //等级奖励模块
        // this.RegisterModel(new RewardManagerModel());
        // //购买消耗品模块
        // this.RegisterModel(new StoreItemModel());
        // //任务模块
        // this.RegisterModel(new TaskManagerModel());
        // //已接任务模块
        // this.RegisterModel(new TaskDetailsModel());
        // #region 子
        // this.RegisterModel(new HuoLiMoModel());
        // this.RegisterModel(new MoLiMoModel());
        // this.RegisterModel(new XiangXunCaoModel());
        // this.RegisterModel(new HpModel());
        // this.RegisterModel(new MpModel());
        // this.RegisterModel(new MeatModel());
        // #endregion
        //背包模块
        // this.RegisterModel(new BackGroundManagerModel());
        // #endregion

        // #region System
        // this.RegisterSystem(new HuoLiMoSystem());
        // this.RegisterSystem(new MoLiMoSystem());
        // this.RegisterSystem(new XiangXunCaoSystem());
        // this.RegisterSystem(new HpSystem());
        // this.RegisterSystem(new MpSystem());
        // this.RegisterSystem(new MeatSystem());
        // #endregion

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
        this.RegisterModel<IItemTableModel>(new ItemTableModel()); // 注册对照表
        this.RegisterSystem<IInventorySystem>(new InventorySystem()); //注册system

        /// <summary>
        /// 注册经济系统
        /// </summary>
        /// <param name="EconomyModel()"></param>
        /// <typeparam name="IEconomyModel"></typeparam>
        this.RegisterModel<IEconomyModel>(new EconomyModel()); // 注册经济系统Model
        this.RegisterSystem<IEconomySystem>(new EconomySystem()); // 注册经济系统System


        /// <summary>
        /// 热更新
        /// </summary>
        /// <param name="HotUpdateModel()"></param>
        /// <typeparam name="IHotUpdateModel"></typeparam>
        this.RegisterModel<IHotUpdateModel>(new HotUpdateModel()); // 注册热更新系统Model
        this.RegisterSystem<IHotUpdateSystem>(new HotUpdateSystem()); // 注册热更新系统System


        /// <summary>
        /// 注册tween工具
        /// </summary>
        /// <returns></returns>
        RegisterUtility<ITweenUtility>(new DOTweenUtility());

        /// <summary>
        /// 注册资源加载工具
        /// </summary>
        /// <param name="AddressablesUtility()"></param>
        /// <typeparam name="IResUtility"></typeparam>
        RegisterUtility<IResUtility>(new AddressablesUtility());


        /// <summary>
        /// 注册存档工具
        /// </summary>
        /// <param name="EasySaveUtility()"></param>
        /// <typeparam name="ISaveUtility"></typeparam>
        this.RegisterUtility<ISaveUtility>(new EasySaveUtility());
        this.RegisterSystem<ISaveSystem>(new SaveSystem());
        this.RegisterModel<ISaveModel>(new SaveModel());
    }
}