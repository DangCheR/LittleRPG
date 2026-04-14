using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace LittleRPG.Physics
{

    /// <summary>
    /// 碰撞类型
    /// </summary>
    public enum ShapeType // *
    {
        Circle,
        Box,
    }

    /// <summary>
    /// Box碰撞组件
    /// </summary>
    public struct BoxShapeData : IComponentData
    {
        public Vector2 dimensions;
    }

    /// <summary>
    /// 圆形
    /// </summary>
    public struct CircleShapeData : IComponentData
    {
        public float radius;
    }

    /// <summary>
    /// 包围盒，需要参与碰撞检测的都需要添加
    /// IsStaticBody：是否为静态包围盒数据
    /// </summary>
    public struct TreeInsersionData : IComponentData
    {
        public bool IsStaticBody;
    }

    /// <summary>
    /// 运行时物理数据
    /// </summary>
    public struct PhyBodyData : IComponentData
    {
        // 线速度
        public Vector2 Velocity;

        // 受力
        public Vector2 Force;

        // 旋转角速度
        public float AngularVelocity;

        // 扭矩
        public float AngularForce;

        // 阻力
        public float LinearDamp;

        // 旋转阻力
        public float AngularDamp;

        // 质量
        public float Mass;

        // 质量倒数
        public float InvMass;

        // 我说白了，我都用不上
        public float Restitution;
        public float Inertia;

        //?

        //ADD STATIC BOOl FOR WALLS
    }

    /// <summary>
    /// 碰撞组件
    /// </summary>
    public struct ShapeData : IComponentData
    {
        public Vector2 Position;
        public float Rotation; /// stored in degree for now
        public Vector2 PreviousPosition;
        public float PreviousRotation;
        public CollisionLayer collisionLayer;
        public ShapeType shapeType;
        public bool HasDynamics;
        public bool IsTrigger;

    }
}
