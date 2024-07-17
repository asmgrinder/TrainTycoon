using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int TicketCost = 10;
    public int LuggageCost = 15;
    public int BlanketCost = 50;

    [SerializeField] TMP_Text CoinsText;

    [SerializeField] NewZoneController saveToilet;
    [SerializeField] NewZoneController[] saveSeatZones;
    [SerializeField] NewZoneController[] saveHelperZones;

    [SerializeField] GameObject menu;
    [SerializeField] GameObject gameUI;

    static bool showMenuAtStart = true;

    public enum TrainState { Station, Moving, Count };
    public TrainState State => state;

    public float[] StatesTimes = new float[2] { 10, 30 };

    public bool CanRespawnPassengers => (TrainState.Station == state && stateTimer > StatesTimes[(int)State] - 1);

    public static GameManager Instance => instance;

    static GameManager instance;

    int coins = 25;

    float stateTimer = 0;
    TrainState state = TrainState.Station;

    [Serializable]
    public class SaveInfo
    {
        public int Coins;
        public bool ToiletZoneOn;
        public bool[] SeatsZonesOn;
        public bool[] HelperZonesOn;
    }

    SaveInfo saveInfo;

    private static string dataFilePath;

    //string json = JsonUtility.ToJson(saveInfo);
    //saveInfo = JsonUtility.FromJson<SaveInfo>(json);
    // JsonUtility.FromJsonOverwrite(json, saveInfo);

    void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Application.targetFrameRate = 60;
    }


    void Awake()
    {
        SceneManager.sceneLoaded += onSceneLoaded;

        int level = SceneManager.GetActiveScene().buildIndex;
        dataFilePath = Path.Combine(Application.persistentDataPath, "GameData" + level.ToString() + ".json");
        Debug.Log("Save file path: " + dataFilePath);
        //File.Delete(dataFilePath);
        newSave();
        if (null == instance)
        {
            instance = this;
        }

        if (showMenuAtStart)
        {
            showMenuAtStart = false;
            menu.SetActive(true);
            gameUI.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        load();
        CoinsText.text = coins.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // process train states
        if (stateTimer > 0)
        {
            stateTimer -= Mathf.Min(stateTimer, Time.deltaTime);
            if (0 == stateTimer)
            {
                state = (TrainState)(((int)State + 1) % (int)TrainState.Count);
                stateTimer = StatesTimes[(int)State];
            }
        }
    }

    public void StartStates()
    {
        stateTimer = StatesTimes[0];
    }

    public void MarkZoneOpen(NewZoneController Source)
    {
        int index = -1;
        if (Source == saveToilet)
        {
            saveInfo.ToiletZoneOn = true;
            save();
        }
        else if (0 <= (index = Array.FindIndex(saveSeatZones, ssz => ssz == Source)))
        {
            saveInfo.SeatsZonesOn[index] = true;
            save();
        }
        else if (0 <= (index = Array.FindIndex(saveHelperZones, shz => shz == Source)))
        {
            saveInfo.HelperZonesOn[index] = true;
            save();
        }
    }

    void newSave()
    {
        saveInfo = new();
        saveInfo.Coins = coins;
        saveInfo.SeatsZonesOn = new bool[saveSeatZones.Length];
        saveInfo.HelperZonesOn = new bool[saveHelperZones.Length];
    }

    void load()
    {
        try
        {
            using (StreamReader reader = new(dataFilePath))
            {
                string dataToLoad = reader.ReadToEnd();

                Debug.Log("load: " + dataToLoad);

                //JsonUtility.FromJsonOverwrite(dataToLoad, saveInfo);
                SaveInfo si = JsonUtility.FromJson<SaveInfo>(dataToLoad);
                if (si.SeatsZonesOn.Length == saveSeatZones.Length
                    && si.HelperZonesOn.Length == saveHelperZones.Length)
                {
                    saveInfo = si;
                    coins = saveInfo.Coins;
                    CoinsText.text = coins.ToString();
                    for (int i = 0; i < saveInfo.SeatsZonesOn.Length; i++)
                    {
                        if (saveInfo.SeatsZonesOn[i])
                        {
                            saveSeatZones[i].OpenZone();
                        }
                    }
                    for (int i = 0; i < saveInfo.HelperZonesOn.Length; i++)
                    {
                        if (saveInfo.HelperZonesOn[i])
                        {
                            saveHelperZones[i].OpenZone();
                        }
                    }
                    if (saveInfo.ToiletZoneOn)
                    {
                        saveToilet.OpenZone();
                    }
                }
            }
        }
        catch (FileNotFoundException e)
        {
            Debug.Log("Load: " + e.Message);
            newSave();
        }
    }

    void save()
    {
        try
        {
            using (StreamWriter writer = new(dataFilePath))
            {
                saveInfo.Coins = coins;
                string dataToWrite = JsonUtility.ToJson(saveInfo);
                writer.Write(dataToWrite);
                Debug.Log("save: " + dataToWrite);
            }
        }
        catch (Exception)
        {
            Debug.Log("Save failed");
        }
    }

    public void TicketSold()
    {
        coins += TicketCost;
        CoinsText.text = coins.ToString();
        save();
    }

    public void LuggageTaken()
    {
        coins += LuggageCost;
        CoinsText.text = coins.ToString();
        save();
    }

    public void BlanketTaken()
    {
        coins += BlanketCost;
        CoinsText.text = coins.ToString();
        save();
    }

    public bool SpendCoins(int Amount)
    {
        if (coins >= Amount)
        {
            coins -= Amount;
            CoinsText.text = coins.ToString();
            save();
            return true;
        }
        return false;
    }
}
