using Firebase.Database;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseDatabaseManager
{
    private static FirebaseDatabaseManager instance;
    public static FirebaseDatabaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FirebaseDatabaseManager();
                instance.Init(); // 인스턴스 생성 시 자동으로 Init 호출
            }
            return instance;
        }
    }

    private DatabaseReference databaseReference;

    public void Init()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    private bool CheckDatabaseReference()
    {
        if (databaseReference == null)
        {
            Debug.LogError("Database reference is null. Trying to reinitialize...");
            Init();
            return false;
        }
        return true;
    }
    public void SavePlayerData(string userId, GameDataManager.GameData gameData)
    {
        string json = JsonUtility.ToJson(gameData);
        databaseReference.Child("users").Child(userId).Child("playerData").SetRawJsonValueAsync(json);
    }
    public void LoadPlayerData(string userId, Action<GameDataManager.GameData> callback)
    {
        databaseReference.Child("users").Child(userId).Child("playerData")
            .GetValueAsync().ContinueWith(task => {
                if (task.IsCompleted && task.Result.Exists)
                {
                    GameDataManager.GameData playerData =
                        JsonUtility.FromJson<GameDataManager.GameData>(task.Result.GetRawJsonValue());
                    callback(playerData);
                }
            });
    }
    // 건물 데이터 저장
    public void SaveBuildingData(string userId, Vector3Int position, ObjectData buildingData, DateTime completionTime)
    {
        Debug.Log("저장");
        if (!CheckDatabaseReference()) return;
        BuildingSaveData saveData = new BuildingSaveData
        {
            Name = buildingData.Name,
            ID = buildingData.ID,
            position = new Vector3Int(position.x,position.y,position.z),
            saveConstructTime = DateTime.Now.ToString(),
            completionTime = completionTime.ToString("O"),
            isComplete = false
        };

        string json = JsonUtility.ToJson(saveData);
        string key = $"{position.x}_{position.y}";
        databaseReference.Child("users").Child(userId).Child("buildings").Child(key).SetRawJsonValueAsync(json);
    }

    // 건물 상태 업데이트
    public void UpdateBuildingState(string userId, Vector3Int position, bool isComplete)
    {
        if (!CheckDatabaseReference()) return;
        string key = $"{position.x}_{position.y}";
        databaseReference.Child("users").Child(userId).Child("buildings").Child(key).Child("isComplete").SetValueAsync(isComplete);
    }

    // 건물 삭제
    public void RemoveBuilding(string userId, Vector3Int position)
    {
        string key = $"{position.x}_{position.y}";
        databaseReference.Child("users").Child(userId).Child("buildings").Child(key).RemoveValueAsync();
    }

    // 유저의 모든 건물 데이터 가져오기
    public void LoadUserBuildings(string userId, Action<Dictionary<string, BuildingSaveData>> callback)
    {
        Debug.Log($"LoadUserBuildings 시작: userId = {userId}");
        if (databaseReference == null)
        {
            Debug.LogError("databaseReference is null");
            return;
        }

        DatabaseReference buildingsRef = databaseReference.Child("users").Child(userId).Child("buildings");

        // 실시간 동기화 설정
        buildingsRef.KeepSynced(true);

        //GetValueAsync는 1회성 데이터 로드
        //ValueChanged는 데이터 변경될 때마다 자동으로 호출되어 실시간 동기화 가능
        buildingsRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"Database error: {args.DatabaseError.Message}");
                return;
            }

            Dictionary<string, BuildingSaveData> buildings = new Dictionary<string, BuildingSaveData>();
            DataSnapshot snapshot = args.Snapshot;

            if (!snapshot.Exists)
            {
                callback(buildings);
                return;
            }

            // 파이어베이스는 기본적으로 비동기
            foreach (DataSnapshot buildingSnapshot in snapshot.Children)
            {
                BuildingSaveData buildingData = JsonUtility.FromJson<BuildingSaveData>(buildingSnapshot.GetRawJsonValue());
                Debug.Log(buildingData.Name);
                buildings.Add(buildingSnapshot.Key, buildingData);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    BuildSystemManager.Instance.BuildDataLoad(buildingData);
                });
            }
            //모든 건물 처리가 끝난 후 콜백 실행으로 처리 완료 알림
            callback(buildings);
        };
    }


}

[System.Serializable]
public class BuildingSaveData: ObjectData
{
    public Vector3Int position;
    public string saveConstructTime;
    public string completionTime;
    public bool isComplete;
}

[System.Serializable]
public class SerializableVector3Int
{
    public int x;
    public int y;
    public int z;

    public SerializableVector3Int(Vector3Int vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }

    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }
}