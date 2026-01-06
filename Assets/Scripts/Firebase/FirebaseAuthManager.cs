using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using System;

public class FirebaseAuthManager 
{
    private static FirebaseAuthManager instance=null;

    public static FirebaseAuthManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new FirebaseAuthManager();
            }
            return instance;
        }
    }

    private FirebaseAuth auth;//로그인 / 회원가입 등에 사용
    private FirebaseUser user;// 인증이 완료된 유저 정보

    public string UserId=>user?.UserId;
    public Action<bool>LoginState;
    public bool IsLoggedIn => user != null;
    public void Init()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            Debug.Log($"자동 로그인 성공: {user.UserId}");
            LoginState?.Invoke(true);
        }
        auth.StateChanged += OnChanged;
    }

    private void OnChanged(object sendedr, EventArgs e)
    {
        if(auth.CurrentUser!=user)
        {
            bool signed = (auth.CurrentUser != user && auth.CurrentUser != null);
            if (!signed && user != null)
            {
                Debug.Log("로그아웃");
                GameDataManager.Instance.ClearUserData();
                LoginState?.Invoke(false);
            }
            user = auth.CurrentUser;
            if (signed)
            {
                Debug.Log("로그인");
                GameDataManager.Instance.OnUserLoggedIn(user.UserId);
                LoginState?.Invoke(true);
            }
        }
    }
    

    public void Create(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if(task.IsCanceled)
            {
                Debug.LogError("회원가입 취소");
                return;
            }
            if(task.IsFaulted)
            {
                Debug.LogError("회원가입 실패");
                return;
            }
            FirebaseUser newUser = task.Result.User;
            Debug.LogError("회원가입 완료");
        });
    }
    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                // 로그인 정보 로컬에 저장
                PlayerPrefs.SetString("UserEmail", email);
                PlayerPrefs.SetString("UserPassword", password);
                PlayerPrefs.Save();
                Debug.Log("로그인 정보 저장 완료");
            }
        });
    }
    public void LogOut()
    {
        auth.SignOut();
        Debug.Log("로그아웃");
    }
}
