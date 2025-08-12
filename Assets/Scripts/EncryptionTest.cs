using UnityEngine;
using System;
using System.Collections.Generic;
using DataSaveLoad_Encryption;

[Serializable]
public class GameData
{
    public string Name;
    public int Level;
    public List<Item> Inventory;
}

[Serializable]
public struct Item
{
    public string Name;
    public int amount;
}

public class EncryptionTest : MonoBehaviour
{
    public HybridEncryptionManager encryptionManager;
    public GameData gameData;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            string data =  JsonUtility.ToJson(gameData,true);
            encryptionManager.SaveData(data);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            string data = encryptionManager.LoadData();
            gameData = JsonUtility.FromJson<GameData>(data);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            gameData = new GameData();
        }
    }
}
