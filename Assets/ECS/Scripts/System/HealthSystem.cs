using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace LittleRPG.Combat
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]

    public partial struct HealthSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HealthData>();
        }
        public void OnUpdata(ref SystemState state)
        {

        }
    }
}