using Components;
using Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public unsafe class GameManager : MonoBehaviour
{
    private void Awake()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        var player = entityManager.CreateEntity();
        entityManager.AddComponentData(player, new WalletComponentData());

        var system = world.CreateSystemManaged<WalletPlayerPrefsSaveSystem>();
        world.GetExistingSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(system);

        // var buffer = entityManager.AddBuffer<WalletModificationData>(player);
        // buffer.Add(new WalletModificationData(0, long.MaxValue));
        // buffer.Add(new WalletModificationData(1, 100));
        // buffer.Add(new WalletModificationData(1, 100));
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