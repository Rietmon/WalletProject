using System;
using System.Threading.Tasks;
using Components;
using Cores;
using Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public unsafe class GameManager : MonoBehaviour
{
    [SerializeField] private bool usePlayerPrefsForLoad;
    [SerializeField] private bool useTextForLoad;
    [SerializeField] private UIManager uiManager;
    
    private Entity walletEntity;
    private RefRW<WalletComponentData> walletComponent;
    
    private void Awake()
    {
        uiManager.AddMoney = AddCurrency;
        uiManager.SetMoney = SetCurrency;
        
        var world = World.DefaultGameObjectInjectionWorld;
        
        walletEntity = world.EntityManager.CreateEntity();
        world.EntityManager.AddComponentData(walletEntity, new WalletComponentData());
        world.EntityManager.AddBuffer<WalletModificationData>(walletEntity);

        WalletPlayerPrefsSaveSystem.deserializeOnCreate = usePlayerPrefsForLoad;
        WalletTextSaveSystem.deserializeOnCreate = useTextForLoad;
        
        var playerPrefsSaveSystem = world.CreateSystemManaged<WalletPlayerPrefsSaveSystem>();
        var textSaveSystem = world.CreateSystemManaged<WalletTextSaveSystem>();
        
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

    private void Update()
    {
        var wallet = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<WalletComponentData>(walletEntity);
        for (var i = 0; i < Currency.Count; i++)
            uiManager.InitializeCounters((byte)i, wallet.wallet[i]);
    }
}