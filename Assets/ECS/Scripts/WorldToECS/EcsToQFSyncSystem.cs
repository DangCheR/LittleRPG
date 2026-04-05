// using Unity.Entities;
// using Unity.Burst;
// using QFramework;

// [UpdateInGroup(typeof(PresentationSystemGroup))] // 确保在渲染前同步数据
// public partial struct EcsToQFSyncSystem : ISystem, QFramework.ISystem // 让系统能访问 QF Model
// {
//     // 实现 ICanGetModel 接口需要的架构引用
//     public IArchitecture GetArchitecture() => CombatApp.Interface;

//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         // 1. 获取 QF Model (非 Burst 部分)
//         // 注意：由于访问了托管对象 PlayerModel，这一部分不能加 [BurstCompile]
//         // 或者我们可以先在外部获取 Model 引用
//         var playerModel = this.GetModel<IPlayerModel>();

//         // 2. 查询玩家实体数据（假设只有一个玩家）
//         foreach (var health in SystemAPI.Query<RefRO<HealthComponent>>().WithAll<PlayerTag>())
//         {
//             // 3. 将 ECS 数据同步到 QF Model
//             // 这里建议做一个简单的差值判断，避免每帧都触发 BindableProperty 的写操作
//             if (playerModel.CurHP.Value != health.ValueRO.Current)
//             {
//                 playerModel.CurHP.Value = health.ValueRO.Current;
//             }
//         }
//     }
// }