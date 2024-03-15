using Components;
using Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class GameManager : MonoBehaviour
{
    [SerializeField] private bool usePlayerPrefsForLoad;
    [SerializeField] private bool useTextForLoad;
    
    private Entity walletEntity;
    
    private void Awake()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        
        walletEntity = world.EntityManager.CreateEntity();
        world.EntityManager.AddComponentData(walletEntity, new WalletComponentData());
        world.EntityManager.AddBuffer<WalletModificationData>(walletEntity);

        var playerPrefsSaveSystem = world.CreateSystemManaged<WalletPlayerPrefsSaveSystem>();
        playerPrefsSaveSystem.deserializeOnCreate = usePlayerPrefsForLoad;
        var textSaveSystem = world.CreateSystemManaged<WalletTextSaveSystem>();
        textSaveSystem.deserializeOnCreate = useTextForLoad;
        
        var simulationSystemGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        simulationSystemGroup.AddSystemToUpdateList(playerPrefsSaveSystem);
        simulationSystemGroup.AddSystemToUpdateList(textSaveSystem);
    }
    
    public void AddCurrency(byte currency, long delta)
    {
        var buffer = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<WalletModificationData>(walletEntity);
        buffer.Add(new WalletModificationData(currency, delta));
    }
    
    public void SetCurrency(byte currency, long value)
    {
        var buffer = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<WalletModificationData>(walletEntity);
        buffer.Add(new WalletModificationData(currency, 0, value));
    }
}