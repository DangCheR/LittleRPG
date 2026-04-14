using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LittleRPG.Physics
{
    // An authoring component is just a normal MonoBehavior that has a Baker<T> class.
    public class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public Transform point;
        public float radius;
        public int SpawnCount;       // 这一波生成的数量
        public uint Seed;            // 随机种子
        // In baking, this Baker will run once for every SpawnerAuthoring instance in a subscene.
        // (Note that nesting an authoring component's Baker class inside the authoring MonoBehaviour class
        // is simply an optional matter of style.)
        class Baker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new RandomSpawner
                {
                    Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    point = new float2(authoring.point.position.x, authoring.point.position.z),
                    radius = authoring.radius,
                    SpawnCount = authoring.SpawnCount,
                    Seed = authoring.Seed
                });
            }
        }

        private void OnDrawGizmos()
        {
            // 设置颜色（半透明绿）
            Gizmos.color = new Color(0, 1, 0, 0.3f);

            Vector3 center = point != null ? point.position : transform.position;

            // 画一个填充圆（如果是3D场景，通常用平铺在地面上的圆）
            // 或者是画一个线框圆
            Gizmos.DrawWireSphere(center, radius);

            // 如果想画更明显的平面圆
            Gizmos.color = new Color(0, 1, 0, 0.1f);
            Gizmos.DrawSphere(center, radius);

            // 辅助线
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + Vector3.forward * radius);
        }
    }
}
