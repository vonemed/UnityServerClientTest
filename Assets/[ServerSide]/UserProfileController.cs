using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UserProfileController : MonoBehaviour
{
    public static UserProfileController Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UIController.Instance.ShowLoading();
        InitiateUserCheck();
    }

    private void InitiateUserCheck()
    {
        var localLoginData = PlayerPrefs.GetString("localLogin");
        // var userId = string.IsNullOrEmpty(localLoginData) ? Guid.NewGuid() : new Guid(localLoginData);
        
        // Debug.Log($"Login { Guid.NewGuid().ToString()}");
        var request = new LoginWithCustomIDRequest
        {
            CustomId = localLoginData
        };
            
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }
    
    public void NewLogin(string loginName)
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = "BE28C";
        }
        
        PlayerPrefs.SetString("localLogin", Guid.NewGuid().ToString());
        PlayerPrefs.SetString("localUsername", loginName);

        Debug.Log($"Registration { PlayerPrefs.GetString("localLogin")}");

        
        var request = new LoginWithCustomIDRequest 
            { 
                CustomId = PlayerPrefs.GetString("localLogin"),
                CreateAccount = true
            };
        
        PlayFabClientAPI.LoginWithCustomID(request, OnRegisterSuccess, OnLoginFailure);

        
        // var requestNameChange = new UpdateUserTitleDisplayNameRequest
        // {
        //     DisplayName = loginName
        // };
        //
        // PlayFabClientAPI.UpdateUserTitleDisplayName(requestNameChange, OnNameChangeSuccess, OnNameChangeFailure);
    }

    public void ChangeName(string newName)
    {
        var requestNameChange = new UpdateUserTitleDisplayNameRequest
        {
            
            DisplayName = newName
        };
        
        PlayerPrefs.SetString("localUsername", newName);
        
        PlayFabClientAPI.UpdateUserTitleDisplayName(requestNameChange, OnNameChangeSuccess, OnNameChangeFailure);
        
        UIController.Instance.ShowSuccessPanel();
    }

    public void GetName()
    {
        var request = new GetAccountInfoRequest();

        PlayFabClientAPI.GetAccountInfo(request, OnAccountSuccess, OnAccountFailure);
    }

    private void OnAccountFailure(PlayFabError obj)
    {
        Debug.Log("Data is not retrieved");
    }

    private void OnAccountSuccess(GetAccountInfoResult obj)
    {
        Debug.Log("Data retrieved");
        // obj.AccountInfo.TitleInfo.DisplayName
    }

    #region ReactionsToRequests
    private void OnNameChangeFailure(PlayFabError obj)
    {
        Debug.LogWarning("Something went wrong with name updating.  :(");
    }

    private void OnNameChangeSuccess(UpdateUserTitleDisplayNameResult obj)
    {
        Debug.Log($"Name updated successfully to {obj.DisplayName}");
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with registration.  :(");
        Debug.Log("Here's some debug information:");
        Debug.Log(error.GenerateErrorReport());
    }

    private void OnRegisterSuccess(LoginResult obj)
    {
        Debug.Log("Congratulations, you are now registered!");
        ChangeName(PlayerPrefs.GetString("localUsername"));
        UIController.Instance.ShowSuccessPanel();
    }
    
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, on login!");
        UIController.Instance.ShowSuccessPanel();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.Log("Here's some debug information:");
        Debug.Log(error.GenerateErrorReport());

        UIController.Instance.ShowLoginPanel();
    }
    
    #endregion
}
