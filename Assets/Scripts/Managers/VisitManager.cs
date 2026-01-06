using UnityEngine;
using System.Collections.Generic;
using Firebase.Database;
using System;
using PimDeWitte.UnityMainThreadDispatcher;

public class VisitManager : Singleton<VisitManager>
{
    private DatabaseReference databaseReference;
    public string currentVisitingUserId { get; private set; } // 현재 방문 중인 유저 ID
    public bool isVisiting => !string.IsNullOrEmpty(currentVisitingUserId); // 방문 중인지 여부

    protected override void Awake()
    {
        base.Awake();
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // 방문 가능한 유저 목록 로드
    public void LoadVisitableUsers(Action<List<UserProfile>> callback)
    {
        DatabaseReference usersRef = databaseReference.Child("users");
        usersRef.GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("방문 가능한 유저 목록 로드 실패");
                return;
            }

            List<UserProfile> users = new List<UserProfile>();
            DataSnapshot snapshot = task.Result;

            foreach (DataSnapshot userSnapshot in snapshot.Children)
            {
                // 현재 유저는 제외
                if (userSnapshot.Key == FirebaseAuthManager.Instance.UserId)
                    continue;

                UserProfile profile = new UserProfile
                {
                    UserId = userSnapshot.Key,
                    // 필요한 경우 추가 프로필 정보 로드
                };
                users.Add(profile);
            }

            // 메인 스레드에서 콜백 실행
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                callback?.Invoke(users);
            });
        });
    }

    // 다른 유저의 땅 방문 시작
    public void StartVisit(string userId)
    {
        if (userId == FirebaseAuthManager.Instance.UserId)
        {
            Debug.LogWarning("자신의 땅은 방문할 수 없습니다.");
            return;
        }

        // 로딩 매니저를 통해 방문 시작
        LoadingSceneManager.Instance.StartVisitLoading(userId);
    }
    // 방문 종료
    public void EndVisit()
    {
        if (!isVisiting) return;

        // 방문 중인 건물들 제거
        ClearCurrentBuildings();

        // 원래 유저의 데이터 로드
        string originalUserId = FirebaseAuthManager.Instance.UserId;
        GameDataManager.Instance.BuildDataLoad();

        currentVisitingUserId = null;
    }

    // 현재 표시된 건물들 제거
    private void ClearCurrentBuildings()
    {
        var objectPlacer = BuildSystemManager.Instance.objectPlacer;
        if (objectPlacer != null)
        {
            objectPlacer.ClearAllBuildings();
        }
    }
    public void SetVisitingStatus(string userId)
    {
        currentVisitingUserId = userId;
        // 방문 UI 업데이트 등 필요한 처리
    }

    // 방문하는 유저의 건물 데이터 로드
    private void LoadVisitingUserBuildings(string userId)
    {
        FirebaseDatabaseManager.Instance.LoadUserBuildings(userId, (buildings) => {
            Debug.Log($"방문한 유저의 건물 {buildings.Count}개 로드됨");
        });
    }
}

// 유저 프로필 데이터 구조체
[System.Serializable]
public struct UserProfile
{
    public string UserId;
    // 필요한 경우 닉네임, 레벨 등 추가 가능
}