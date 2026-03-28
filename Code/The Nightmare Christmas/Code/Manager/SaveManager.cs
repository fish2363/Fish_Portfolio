using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public bool isAlreadyStart;
    public bool isFirstSpawn;
    public bool isSecondSpawn;
    public bool isThirdSpawn;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public SaveData saveData = new SaveData();

    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "GameSaveData.json");

            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerSpawnPoint()
    {
        if (saveData.isSecondSpawn)
            saveData.isThirdSpawn = true;
        else if (saveData.isFirstSpawn)
            saveData.isSecondSpawn = true;
        else if (saveData.isAlreadyStart)
            saveData.isFirstSpawn = true;
        else
            saveData.isAlreadyStart = true;

        SaveGame();
    }

    public void SaveGame()
    {
        string jsonText = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(saveFilePath, jsonText);

        Debug.Log($"[SaveManager] 게임 저장 완료 경로: {saveFilePath}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string jsonText = File.ReadAllText(saveFilePath);

            saveData = JsonUtility.FromJson<SaveData>(jsonText);

            Debug.Log("게임 불러오기 성공");
        }
        else
        {
            saveData = new SaveData();
            Debug.Log("저장된 파일이 없어 새 데이터를 생성합니다");
        }
    }
}