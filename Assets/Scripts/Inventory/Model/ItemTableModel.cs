using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 该类是一个全局的装备表，包含装备基本显示信息
/// </summary>
namespace LittleRPG
{
    interface IItemModel : IModel
    {

    }

    public class ItemInfo
    {
        public int ItemID; // 物品ID
        public string ItemName; //物品名称
        public string SpriteIconPath;// 物品图标路径

        public Sprite SpriteIcon;
        public string Describe;
        public int maxStack;
        public int MaxStack
        {
            get => maxStack;
            set
            {
                maxStack = value < 1 || value > 999 ? 1 : value;
            }
        }

        public ItemInfo(int id,
            string Name,
            string describe,
            Sprite sprite,
            int Max)
        {
            ItemID = id;
            ItemName = Name;
            Describe = describe;
            SpriteIcon = sprite;
            maxStack = Max;
        }
    }

    //所有装备的基础内容
    public class ItemTableModel : AbstractModel, IItemModel
    {
        public Dictionary<int, ItemInfo> ItemDic = new();

        protected override void OnInit()
        {
            //假装我们有一个读取JOSN或Excel的操作
            //直接存储配置表
            string AxeSpritePath = "Sprites/Items/axe";
            string BootsSpritePath = "Sprites/Items/boots";
            string BowSpritePath = "Sprites/Items/bow";
            Sprite AxeSprite = Resources.Load<Sprite>(AxeSpritePath);
            Sprite BootsSprite = Resources.Load<Sprite>(BootsSpritePath);
            Sprite BowSprite = Resources.Load<Sprite>(BowSpritePath);
            ItemDic[0] = new ItemInfo(0,"大斧头", "帅的一皮", AxeSprite, 3);
            ItemDic[1] = new ItemInfo(1,"大鞋子", "快的一皮", BootsSprite, 3);
            ItemDic[2] = new ItemInfo(2,"大弓箭", "猛的一皮", BowSprite, 3);

        }
    }
}
