using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LittleRPG.Physics
{

    /// <summary>
    /// 用于可碰撞的，添加物理控制
    /// </summary>
    public class PhyBodyAuthoring : MonoBehaviour
    {

        public ShapeType shapeType;

        [SerializeField, HideInInspector]
        public Vector2 dimensions;
        
        [SerializeField, HideInInspector]
        public float radius;

        public bool HasDynamics = true;
        public bool IsStaticBody = false;
        
        [SerializeField, HideInInspector]
        public float Mass = 1;          // 质量
        
        [Range(0.0f, 1f)]
        public float restitution = 0f;


        public CollisionLayer collisionLayer;  // 碰撞层


        /// <summary>
        /// 绘制碰撞框        /// </summary>
        private void OnDrawGizmos()
        {
            if (shapeType == ShapeType.Circle)
                Gizmos.DrawWireSphere(this.transform.position, radius);
            if (shapeType == ShapeType.Box)
            {
                Vector3 position = transform.position;
                Quaternion rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z); // Only Z rotation for 2D
                Vector3 scale = new Vector3(dimensions.x, dimensions.y, 1f);

                // Save the old matrix
                Matrix4x4 oldMatrix = Gizmos.matrix;

                // Apply the new matrix (position * rotation * scale)
                Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

                // Draw cube centered at origin, it will be transformed by the matrix
                Gizmos.DrawWireCube(Vector3.zero, scale);

                // Restore the old matrix
                Gizmos.matrix = oldMatrix;
            }
        }


        class PhyBodyBaker : Baker<PhyBodyAuthoring>
        {
            public override void Bake(PhyBodyAuthoring authoring)
            {

                float mass = authoring.IsStaticBody ? 0 : authoring.Mass;
                float inertia = 0;

                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                //Entity entityManualOverride = GetEntity(TransformUsageFlags.ManualOverride);
                AddComponent(entity, new LocalTransform
                {
                    Position = new float3(0, 0, 0),  // Set the initial position
                    Rotation = Quaternion.Euler(0, authoring.transform.eulerAngles.y, 0),  // Set the initial rotation
                    Scale = 1      // Set the initial scale
                });

                AddComponent(entity, new TreeInsersionData { IsStaticBody = authoring.IsStaticBody });

                // 只需要X Z两个轴
                AddComponent(entity, new ShapeData
                {
                    Position = new float2(authoring.transform.position.x, authoring.transform.position.z) ,
                    collisionLayer = authoring.collisionLayer,
                    shapeType = authoring.shapeType,
                    Rotation = authoring.transform.eulerAngles.y,
                    HasDynamics = authoring.HasDynamics,
                });

                switch (authoring.shapeType)
                {
                    case ShapeType.Circle:
                        AddComponent(entity, new CircleShapeData
                        {
                            radius = authoring.radius,
                        });
                        inertia = 0.5f * mass * authoring.radius * authoring.radius;
                        break;
                    case ShapeType.Box:
                        AddComponent(entity, new BoxShapeData
                        {
                            dimensions = authoring.dimensions,
                        });
                        inertia = (1f / 12f) * mass * (authoring.dimensions.x * authoring.dimensions.x + authoring.dimensions.y * authoring.dimensions.y);
                        break;

                }
                if (authoring.HasDynamics)
                {
                    //Debug.Log(inertia);
                    AddComponent(entity, new PhyBodyData
                    {
                        Mass = mass,
                        InvMass = mass > 0 ? 1 / mass : 0,
                        Inertia = inertia,
                        LinearDamp = 0.015f,
                        AngularDamp = 0.05f,
                        Restitution = authoring.restitution
                    });
                }

            }
        }

        #region EDITOR

#if UNITY_EDITOR

        [CustomEditor(typeof(PhysicsBodyAuthoring))]
        public class PhysicsBodyAuthoring : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

            }
        }
        //[CustomEditor(typeof(PhyBodyAuthoring)), CanEditMultipleObjects]
        [CustomEditor(typeof(PhyBodyAuthoring))]
        public class PhyBodyEditor : Editor
        {
            SerializedProperty radius;
            SerializedProperty dimensions;
            SerializedProperty triggerType;
            SerializedProperty mass;

            private void OnEnable()
            {
                radius = serializedObject.FindProperty("radius");
                dimensions = serializedObject.FindProperty("dimensions");
                triggerType = serializedObject.FindProperty("triggerType");
                mass = serializedObject.FindProperty("Mass");
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                serializedObject.Update();


                PhyBodyAuthoring phyBody = (PhyBodyAuthoring)target;

                if (phyBody.shapeType == ShapeType.Circle)
                {
                    EditorGUILayout.PropertyField(radius);
                }
                if (phyBody.shapeType == ShapeType.Box)
                {
                    EditorGUILayout.PropertyField(dimensions);
                }
                if (!phyBody.IsStaticBody)
                {
                    EditorGUILayout.PropertyField(mass);
                }


                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
        #endregion





    }
}