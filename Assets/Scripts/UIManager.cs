using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Action<byte, long> AddMoney { get; set; }
    public Action<byte, long> SetMoney { get; set; }
    
    [SerializeField] private UIMoneyCounter[] moneyCounters;
    
    private void Start()
    {
        foreach (var moneyCounter in moneyCounters)
        {
            moneyCounter.AddMoney = AddMoney;
            moneyCounter.SetMoney = SetMoney;
        }
    }
    
    public void InitializeCounters(byte currency, long value)
    {
        foreach (var moneyCounter in moneyCounters)
        {
            if (moneyCounter.Currency != currency)
                continue;
            
            moneyCounter.InitializeCounter(value);
        }
    }
}