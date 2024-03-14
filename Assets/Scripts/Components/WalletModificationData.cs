using Unity.Entities;

namespace Components
{
    public readonly struct WalletModificationData : IBufferElementData
    {
        public readonly byte currency;
        public readonly long amount;

        public WalletModificationData(byte currency, long amount)
        {
            this.currency = currency;
            this.amount = amount;
        }
    }
}