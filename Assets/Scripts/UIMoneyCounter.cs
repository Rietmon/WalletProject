using System;
using TMPro;
using UnityEngine;

public class UIMoneyCounter : MonoBehaviour
{
    public Action<byte, long> AddMoney { get; set; }
    public Action<byte, long> SetMoney { get; set; }
    
    [field: SerializeField] public byte Currency { get; private set; }
    [SerializeField] private TextMeshProUGUI moneyText;

    private long value;
    
    public void InitializeCounter(long value)
    {
        this.value = value;
        UpdateCounter();
    }
    
    public void UpdateCounter() => moneyText.text = value.ToString();

    public void AddMoneyButton(int delta)
    {
        value += delta;
        AddMoney(Currency, delta);
        UpdateCounter();
    }

    public void ResetMoneyButton()
    {
        value = 0;
        SetMoney(Currency, 0);
        UpdateCounter();
    }
}