using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Components;
using Cores;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public partial struct WalletTextSaveSystem : ISystem
    {
        private FileStream fileStream;
        private byte[] buffer;
        private bool isBusy;

        public void OnCreate(ref SystemState state)
        {
            fileStream = new FileStream($"{Application.persistentDataPath}wallet.txt", FileMode.OpenOrCreate);
            // Rietmon: 1024 bytes should be enough fot 2 currencies
            buffer = new byte[1024];
            
            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length > 0)
                return;
            
            var entity = entities[0];
            var walletComponent = SystemAPI.GetComponentRW<WalletComponentData>(entity);
            ReadFromFileAsync(walletComponent).ContinueWith((task) =>
            {
                if (!task.IsCompleted)
                    Debug.LogError($"Error");
            });
        }
        
        private async Task ReadFromFileAsync(RefRW<WalletComponentData> walletComponent)
        {
            static unsafe void Apply(RefRW<WalletComponentData> walletComponent, IReadOnlyList<string> values)
            {
                for (var j = 0; j < Currency.Count; j++)
                    walletComponent.ValueRW.wallet[j] = long.Parse(values[j]);
            }

            isBusy = true;
            var length = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            var str = Encoding.UTF8.GetString(buffer, 0, length);
            var values = str.Split('&');
            Apply(walletComponent, values);
            isBusy = false;
        }
        
        private async Task WriteFileAsync(RefRW<WalletComponentData> walletComponent)
        {
            static unsafe string Serialize(RefRW<WalletComponentData> walletComponent)
            {
                var stringBuilder = new StringBuilder();
                for (var j = 0; j < Currency.Count; j++)
                    stringBuilder.Append($"{walletComponent.ValueRW.wallet[j]}&");
                return stringBuilder.ToString();
            }

            isBusy = true;
            var str = Serialize(walletComponent);
            var length = Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
            await fileStream.WriteAsync(buffer, 0, length);
            isBusy = false;
        }
    }
}