using UnityEngine;
using QFramework;
using System.Collections.Generic;

namespace LittleRPG
{
    public class SaveController : MonoBehaviour, IController
    {
        [Header("UI 配置")]
        public GameObject SaveSlotPrefab;
        public Transform ContentParent;// 存档槽位的父物体

        private ISaveModel mSaveModel;

        private ResLoader mLoader; // 资源管家，负责加载存档槽位预制件等资源

        private Dictionary<int, SaveSlotUIView> SlotViews = new Dictionary<int, SaveSlotUIView>(); // 存档槽位的视图列表，key 是 SlotID

        private void Start()
        {
            mSaveModel = this.GetModel<ISaveModel>();

            mLoader = new ResLoader();

            this.RegisterEvent<SaveSlotDeletedEvent>(UpdateSlotUI).UnRegisterWhenGameObjectDestroyed(gameObject);
            mLoader.LoadAssetAsyncWithCallback<GameObject>("Assets/Resources_moved/Prefabs/SaveSlot.prefab", (slotP) =>
            {
                SaveSlotPrefab = slotP;

                // 刷新 UI
                GenerateSlotUIs();
            });
        }

        // 刷新UI

        private void GenerateSlotUIs()
        {
            // 假设我们的游戏固定提供 3 个槽位（0, 1, 2）
            // 即使字典里有些是空的，我们在 LoadAllSlotMetas 里也应该给它塞一个 IsEmpty=true 的 Meta
            foreach (var kvp in mSaveModel.SlotMetas)
            {
                int slotID = kvp.Key;
                SaveSlotMeta meta = kvp.Value;

                // 实例化傀儡
                var go = Instantiate(SaveSlotPrefab, ContentParent);
                var view = go.GetComponent<SaveSlotUIView>();

                SlotViews[slotID] = view;

                // 赋予身份并安插窃听器（点击回调）
                view.Init(slotID);
                view.mOnClickCallback += HandleSlotClicked;
                view.mOnDeleteCallback += HandleSlotDeleted;
                // 根据数据绘制皮囊
                view.UpdateView(meta);
            }
        }

        private void UpdateSlotUI(SaveSlotDeletedEvent evt)
        {
            SlotViews[evt.SlotID].UpdateView(mSaveModel.SlotMetas[evt.SlotID]);
        }

        // ==========================================
        // 3. 响应用户的物理交互
        // ==========================================
        private void HandleSlotClicked(int clickedSlotID)
        {
            // 去 Model 里查一下这个档到底是不是空的
            var meta = mSaveModel.SlotMetas[clickedSlotID];

            // 2. 进入游戏场景
            // startGameEnterBtn.interactable = false;
            //如果数据库中存储的名字的话
            //先进场景再说
            if (PlayDataManager.instance.LoadData(FieldManager.Playername) != null)
            {
                this.SendCommand<LoadSceneCommandMain>();
                AudioManager.instance.SetAudioToGameBgm();
                // return;
            }

            if (meta.IsEmpty)
            {
                // 空的？直接发 Command 建新号！
                this.SendCommand(new StartNewGameCommand(clickedSlotID));
            }
            else
            {
                // 有号？发 Command 读进度！
                // 进阶：这里可以弹一个二级确认框 “确认要加载存档 1 吗？” -> “确定”再发 Command
                this.SendCommand(new LoadGameCommand(clickedSlotID));
            }
        }

        private void HandleSlotDeleted(int deletedSlotID)
        {
            // 处理存档删除逻辑
            Debug.Log($"[SaveController] 请求删除存档 {deletedSlotID}...");
            this.SendCommand(new DeleteGameCommand(deletedSlotID));
        }

        public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
    }
}