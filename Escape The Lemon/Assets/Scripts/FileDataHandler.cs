using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";

    private bool useEncryption = false;
    private readonly string key = "useaproperkey";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption) {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                byte[] dataToLoad;
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    dataToLoad = new byte[stream.Length];

                    stream.Read(dataToLoad, 0, dataToLoad.Length);
                }
                if (useEncryption) EncryptDecrypt(ref dataToLoad);

                string data = Encoding.UTF8.GetString(dataToLoad);

                loadedData = JsonUtility.FromJson<GameData>(data);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            byte[] dataToStore = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));

            if (useEncryption) EncryptDecrypt(ref dataToStore);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                stream.Write(dataToStore, 0, dataToStore.Length);
            }
        } catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    private void EncryptDecrypt(ref byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(data[i] ^ key[i % key.Length]);
        }
    }
}
