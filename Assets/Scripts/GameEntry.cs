using Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public unsafe class GameEntry : MonoBehaviour
{
    private void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        var player = entityManager.CreateEntity();
        entityManager.AddComponentData(player, new WalletComponentData());
        
        var buffer = entityManager.AddBuffer<WalletModificationData>(player);
        buffer.Add(new WalletModificationData(0, 100));
        buffer.Add(new WalletModificationData(1, 100));
        buffer.Add(new WalletModificationData(1, 100));
        
        var other = entityManager.CreateEntity();
        entityManager.AddComponentData(other, new WalletComponentData());
    }
    
    private void Update()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(WalletComponentData));
        var entities = query.ToEntityArray(Allocator.Temp);
        var length = entities.Length;
        for (var i = 0; i < length; i++)
        {
            var entity = entities[i];
            var walletComponent = entityManager.GetComponentData<WalletComponentData>(entity);
            Debug.Log($"{walletComponent.wallet[0]} : {walletComponent.wallet[1]}");
        }
        entities.Dispose();
    }
}