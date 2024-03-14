using Components;
using Cores;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    [DisableAutoCreation]
    public unsafe partial struct WalletPlayerPrefsSaveSystem : ISystem
    {
        private string[] keys;
        
        public void OnCreate(ref SystemState state)
        {
            keys = new string[Currency.Count];
            for (var i = 0; i < Currency.Count; i++)
                keys[i] = $"wallet_currency_{i}";
            
            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length > 0)
                return;
            
            var entity = entities[0];
            var walletComponent = SystemAPI.GetComponentRW<WalletComponentData>(entity);
            for (var j = 0; j < Currency.Count; j++)
            {
#if ENABLE_PLAYER_PREF_UNSAFE_CONVERTING_VALUE_TO_BYTES
                var str = PlayerPrefs.GetString(keys[j]);
                fixed (char* pBuffer = str)
                    walletComponent.ValueRW.wallet[j] = *(long*)pBuffer;
#else
                var value = PlayerPrefs.GetInt(keys[j]);
                walletComponent.ValueRW.wallet[j] = value;
#endif
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length > 0)
                return;
            
            var entity = entities[0];
            var walletComponent = SystemAPI.GetComponentRO<WalletComponentData>(entity);
            for (var j = 0; j < Currency.Count; j++)
            {
                // Rietmon: Here's dilemma, should we use SaveInt?
                // Be cause we are storage long as value (int so usually too small for wallet value)
                // Take both ways :D
                var key = keys[j];
                var value = walletComponent.ValueRO.wallet[j];
#if ENABLE_PLAYER_PREF_UNSAFE_CONVERTING_VALUE_TO_BYTES
                var pBuffer = (char*)&value;
                var str = new string(pBuffer, 0, sizeof(long));
                PlayerPrefs.SetString(key, str);
#else
                PlayerPrefs.SetInt(key, (int)value);
#endif
            }
        }
    }
}