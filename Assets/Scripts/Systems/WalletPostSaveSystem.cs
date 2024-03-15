using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    // Rietmon: No need to save each frame. We can detect changes with WalletModificationData and clean it after saving
    [BurstCompile]
    public partial struct WalletPostSaveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData, WalletModificationData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length is 0 or > 1)
                return;
            
            var entity = entities[0];
            var walletModificationBuffer = SystemAPI.GetBuffer<WalletModificationData>(entity);
            walletModificationBuffer.Clear();
        }
    }
}