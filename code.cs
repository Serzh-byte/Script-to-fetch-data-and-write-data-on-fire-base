using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    // Reference to Firebase database
    private DatabaseReference databaseReference;
    
    // Singleton pattern to ensure only one instance
    public static FirebaseManager Instance { get; private set; }

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize Firebase
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        // Check and fix Firebase dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    // Generic method to write data to Firebase
    public async Task WriteData(string path, object data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data);
            await databaseReference.Child(path).SetRawJsonValueAsync(jsonData);
            Debug.Log($"Data successfully written to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write data: {e.Message}");
        }
    }

    // Generic method to fetch data from Firebase
    public async Task<T> FetchData<T>(string path) where T : class
    {
        try
        {
            DataSnapshot snapshot = await databaseReference.Child(path).GetValueAsync();
            if (snapshot.Exists)
            {
                string jsonData = snapshot.GetRawJsonValue();
                T result = JsonUtility.FromJson<T>(jsonData);
                Debug.Log($"Data successfully fetched from {path}");
                return result;
            }
            else
            {
                Debug.LogWarning($"No data exists at {path}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch data: {e.Message}");
            return null;
        }
    }

    // Example usage
    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public int score;
        public float playTime;
    }

    // Example methods to test the functionality
    public async void TestWrite()
    {
        PlayerData player = new PlayerData
        {
            playerName = "TestPlayer",
            score = 100,
            playTime = 45.5f
        };
        
        await WriteData("players/testUser", player);
    }

    public async void TestFetch()
    {
        PlayerData fetchedData = await FetchData<PlayerData>("players/testUser");
        if (fetchedData != null)
        {
            Debug.Log($"Fetched: Name: {fetchedData.playerName}, Score: {fetchedData.score}, Time: {fetchedData.playTime}");
        }
    }
}
