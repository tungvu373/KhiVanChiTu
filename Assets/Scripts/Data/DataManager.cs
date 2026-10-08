using UnityEngine;
using System;

[Serializable]
public class GameSaveData 
{
    // Mảng chính chứa 3 ô Save
    public CharacterSaveData[] slots = new CharacterSaveData[3] { 
        new CharacterSaveData(), new CharacterSaveData(), new CharacterSaveData() 
    };
    public int lastPlayedSlotIndex = -1; 
}

[Serializable]
public class CharacterSaveData 
{
    public bool isSlotEmpty = true;
    public string characterName = "";
    public int gender = 0; // 0 = Nam, 1 = Nữ
    
    // Đồng bộ CultivationManager
    public int stageIndex = 0;
    public string stageName = "Luyện Khí Kỳ"; 
    public float currentTuVi = 0f;
    
    // Đồng bộ EconomyManager
    public float linhThach = 0f;
    public int coDuyen = 0;
    public int kiemY = 0;

    // Tiến trình
    public float playTime = 0f;
    public string lastPlayedDate = "";
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    public GameSaveData gameSaveData = new GameSaveData();
    private const string SAVE_KEY = "KhiVanChiTu_SaveData";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ DataManager qua các Scene
        LoadData();
    }

    public void LoadData()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json))
        {
            gameSaveData = new GameSaveData();
        }
        else
        {
            gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
        }
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(gameSaveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Game Saved Successfully!");
    }
}
