using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CoinSystem : MonoBehaviour, IDataPersistance
{
    public static CoinSystem Instance { get; private set; }
    public int Coins { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Update()
    {
        if (Menu.Instance != null)
        {
            Menu.Instance.coinText.text = Coins.ToString() + " COIN";
        }
     }

    public void LoadData(GameData data)
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Coins = data.coin;
    }

    public void SaveData(GameData data)
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        data.coin = Coins;
    }

    public void IncreaseCoin(int coin)
    {
        Coins += coin;
    }

    public void DecreaseCoin(int coin)
    {
        Coins -= coin;
    }
}
