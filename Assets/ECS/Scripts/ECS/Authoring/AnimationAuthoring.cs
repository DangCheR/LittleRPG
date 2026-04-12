using Unity.Entities;
using UnityEngine;

/// <summary>
/// 需要模型的自己挂
/// </summary>
namespace LittleRPG.Combat
{
    public class AnimationAuthoring : MonoBehaviour
    {
        public GameObject ModelWithAnimator;
        
        class Baker : Baker<AnimationAuthoring>
        {
            public override void Bake(AnimationAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponentObject(entity, new NeedsAnimationModel
                {
                    ModelWithAnimator = authoring.ModelWithAnimator,
                });
            }
        }
    }
}