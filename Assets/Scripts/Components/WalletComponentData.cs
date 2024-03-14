using Cores;
using Unity.Burst;
using Unity.Entities;

namespace Components
{
    [BurstCompile]
    public unsafe struct WalletComponentData : IComponentData
    {
        public fixed long wallet[Currency.Count];
    }
}