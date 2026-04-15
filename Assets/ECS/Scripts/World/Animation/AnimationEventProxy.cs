using UnityEngine;
using Unity.Entities;

namespace LittleRPG.Combat
{
    public class AnimationEventProxy : MonoBehaviour
    {
        // 只需要记住自己是谁的皮囊就行了
        // 赋一个默认值 Entity.Null，防止未赋值时报错
        public Entity OwnerEntity = Entity.Null;

        public void OnAttackHit()
        {
            // 防御 1：实体还没被分配？滚回去
            if (OwnerEntity == Entity.Null) return;

            // 获取当前激活的 ECS 世界（这是最安全、最正宗的拿法）
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            // 实时获取 EntityManager
            var em = world.EntityManager;

            if (!em.Exists(OwnerEntity))
            {
                Debug.Log("Entity不存在");
                return;
            }

            if (!em.HasComponent<AttackSate>(OwnerEntity))
            {
                Debug.Log("AttackSate组件消失");
            }

            // 拿数据 -> 修改 -> 塞回去
            var state = em.GetComponentData<AttackSate>(OwnerEntity);
            state.TriggerAttackHit = true;
            em.SetComponentData(OwnerEntity, state);

        }

        /// <summary>
        /// 动画事件播放结束，通知ECS世界收尸 
        /// </summary>
        public void OnDeadAnimOver()
        {
            // 防御 1：实体还没被分配？滚回去
            if (OwnerEntity == Entity.Null) return;

            // 获取当前激活的 ECS 世界（这是最安全、最正宗的拿法）
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            // 实时获取 EntityManager
            var em = world.EntityManager;

            if (!em.Exists(OwnerEntity))
            {
                Debug.Log("Entity不存在");
                return;
            }

            if (!em.HasComponent<DeadTag>(OwnerEntity))
            {
                Debug.Log("AttackSate组件消失");
            }

            em.AddComponent<DeceasedTag>(OwnerEntity);
        }
    }
}