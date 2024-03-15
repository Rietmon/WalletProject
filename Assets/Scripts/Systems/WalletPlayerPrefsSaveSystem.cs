using Components;
using Cores;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    [DisableAutoCreation, UpdateBefore(typeof(WalletPostSaveSystem)), UpdateAfter(typeof(WalletSystem))]
    public unsafe partial class WalletPlayerPrefsSaveSystem : SystemBase
    {
        public static bool deserializeOnCreate;
        // Rietmon: Made it as field to avoid allocations
        private string[] keys;
        
        // Rietmon: We can move this code to separate method like Deserialize, but I will keep it here for simplicity
        // Also if we split logic we should have field for check if we already deserialized data
        protected override void OnCreate()
        {
            keys = new string[Currency.Count];
            for (var i = 0; i < Currency.Count; i++)
                keys[i] = $"wallet_currency_{i}";

            if (!deserializeOnCreate)
                return;
            
            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData, WalletModificationData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length is 0 or > 1)
                return;
            
            var entity = entities[0];
            
            var walletComponent = SystemAPI.GetComponentRW<WalletComponentData>(entity);
            for (var j = 0; j < Currency.Count; j++)
            {
#if ENABLE_PLAYER_PREF_UNSAFE_CONVERTING_VALUE_TO_BYTES
                var str = PlayerPrefs.GetString(keys[j]);
                if (string.IsNullOrEmpty(str))
                    continue;
                
                fixed (char* pBuffer = str)
                    walletComponent.ValueRW.wallet[j] = *(long*)pBuffer;
#else
                var value = PlayerPrefs.GetInt(keys[j]);
                walletComponent.ValueRW.wallet[j] = value;
#endif
            }

            Debug.Log($"[{nameof(WalletPlayerPrefsSaveSystem)}] ({nameof(OnCreate)}) " +
                      $"Deserialized from player prefs!");
        }

        protected override void OnUpdate()
        {
            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData, WalletModificationData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length is 0 or > 1)
                return;
            
            var entity = entities[0];
            var walletModificationBuffer = SystemAPI.GetBuffer<WalletModificationData>(entity);
            var bufferLength = walletModificationBuffer.Length;
            if (bufferLength == 0)
                return;
            
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
            
            Debug.Log($"[{nameof(WalletPlayerPrefsSaveSystem)}] ({nameof(OnUpdate)}) " +
                      $"Saved to player prefs!");
        }
    }
}