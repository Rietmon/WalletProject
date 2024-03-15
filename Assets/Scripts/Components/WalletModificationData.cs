using Unity.Entities;

namespace Components
{
    public readonly struct WalletModificationData : IBufferElementData
    {
        public readonly byte currency;
        public readonly long delta;
        public readonly long amount;

        public WalletModificationData(byte currency, long delta, long amount = -1)
        {
            this.currency = currency;
            this.delta = delta;
            this.amount = amount;
        }
    }
}