using Components;
using Cores;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    [BurstCompile]
    public unsafe partial struct WalletSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData, WalletModificationData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            // Rietmon: In task wasn't mentioned that we need to process multiple entities, so I will process only one
            if (length is 0 or > 1)
                return;
            
            var entity = entities[0];
            var walletModificationBuffer = SystemAPI.GetBuffer<WalletModificationData>(entity);
            var bufferLength = walletModificationBuffer.Length;
            if (bufferLength == 0)
                return;

            var walletComponent = SystemAPI.GetComponentRW<WalletComponentData>(entity);
            for (var j = 0; j < bufferLength; j++)
            {
                var walletModification = walletModificationBuffer[j];
                // Rietmon: We are using fixed array, so we need to check if currency is not out of range
                if (walletModification.currency >= Currency.Count)
                    continue;
                
                if (walletModification.amount != -1)
                    walletComponent.ValueRW.wallet[walletModification.currency] = walletModification.amount;
                else
                    walletComponent.ValueRW.wallet[walletModification.currency] += walletModification.delta;
            }
        }
    }
}