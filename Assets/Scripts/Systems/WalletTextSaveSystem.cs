#if ENABLE_SYSTEM_BASED_SAVE_SYSTEM
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
    [DisableAutoCreation]
    public partial class WalletTextSaveSystem : SystemBase
    {
        private static readonly string pathToFile = $"{Application.persistentDataPath}/wallet.txt";
        private FileStream fileStream;
        private byte[] buffer;
        private bool isBusy;

        protected override void OnCreate()
        {
            fileStream = new FileStream(pathToFile, FileMode.OpenOrCreate,
                FileAccess.ReadWrite);
            // Rietmon: 1024 bytes should be enough fot 2 currencies
            buffer = new byte[1024];

            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length is 0 or > 1)
                return;

            var entity = entities[0];
            var walletComponent = SystemAPI.GetComponentRW<WalletComponentData>(entity);
            ReadFromFileAsync(walletComponent).ContinueWith((task) =>
            {
                if (!task.IsFaulted)
                    return;

                Debug.LogError($"[{nameof(WalletTextSaveSystem)}] ({nameof(OnCreate)}) " +
                               $"Catch critical error on reading from file! Deleting file and creating new one...\n" +
                               $"Error: {task.Exception}");
                fileStream.Dispose();
                File.Delete(pathToFile);
                fileStream = new FileStream(pathToFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            });
        }

        protected override void OnUpdate()
        {
            if (isBusy)
                return;

            var query = SystemAPI.QueryBuilder().WithAll<WalletComponentData>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var length = entities.Length;
            if (length is 0 or > 1)
                return;

            var entity = entities[0];
            var walletComponent = SystemAPI.GetComponentRW<WalletComponentData>(entity);
            WriteFileAsync(walletComponent).ContinueWith((task) =>
            {
                if (!task.IsFaulted)
                    return;
                
                Debug.LogError($"[{nameof(WalletTextSaveSystem)}] ({nameof(OnUpdate)}) " +
                               $"Catch critical error at writing to file! Probably it was deleted, creating a new one...\n" +
                               $"Error: {task.Exception}");
                fileStream.Dispose();
                fileStream = new FileStream(pathToFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            });
        }

        private async Task ReadFromFileAsync(RefRW<WalletComponentData> walletComponent)
        {
            static unsafe void Apply(RefRW<WalletComponentData> walletComponent, IReadOnlyList<string> values)
            {
                for (var j = 0; j < Currency.Count; j++)
                {
                    var value = long.TryParse(values[j], out var result) ? result : 0;
                    walletComponent.ValueRW.wallet[j] = value;
                }
            }

            isBusy = true;
            var length = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            if (length > 0)
            {
                var str = Encoding.UTF8.GetString(buffer, 0, length);
                var values = str.Split('&');
                Apply(walletComponent, values);
            }
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
            fileStream.SetLength(length);
            var task = fileStream.WriteAsync(buffer, 0, length);
            await task;
            if (task.IsFaulted)
            {
                Debug.LogError($"[{nameof(WalletTextSaveSystem)}] ({nameof(WriteFileAsync)}) " +
                               $"Catch critical error at writing to file! I don't know what goes wrong...\n" +
                               $"Let's recreate file, might it will help :D\n" +
                               $"Error: {task.Exception}");
                await fileStream.DisposeAsync();
                fileStream = new FileStream(pathToFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            isBusy = false;
        }
    }
}
#endif