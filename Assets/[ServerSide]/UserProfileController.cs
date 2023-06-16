using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public sealed class UserProfileController : MonoBehaviour
{
    public static UserProfileController Instance;

    public ClientWebSocket clientSocket;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UIController.Instance.ShowLoading(); 
        InitiateUserCheck();
    }

    private async void InitiateUserCheck()
    {
        var url = "ws://localhost:5143/ws";
        var uri = new Uri(@"ws://localhost:5143/ws");
        var cancellationToken = new CancellationTokenSource();
        cancellationToken.CancelAfter(5000);
        
        Debug.Log("Connecting....");

        while (!cancellationToken.IsCancellationRequested)
        {
            using (clientSocket = new ClientWebSocket())
            {
                try
                {
                    Debug.Log("<color=cyan>WebSocket connecting.</color>");
                    await clientSocket.ConnectAsync(new Uri(url), cancellationToken.Token);

                    Debug.Log("<color=cyan>WebSocket receiving.</color>");
                    await Receive();

                    Debug.Log("<color=cyan>WebSocket closed.</color>");
                    
                    await clientSocket.ConnectAsync(uri, cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("<color=cyan>WebSocket shutting down.</color>");
                }
            }
        }


        if(clientSocket.State == WebSocketState.Open) Debug.Log("Connected!");
        else
        {
            Debug.Log("Something went wrong :(");
        }
    }
    
    private async Task Receive()
    {
        var arraySegment = new ArraySegment<byte>(new byte[8192]);
        while (clientSocket.State == WebSocketState.Open)
        {
            var result = await clientSocket.ReceiveAsync(arraySegment, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(arraySegment.Array, 0, result.Count);
                // if (OnReceived != null) OnReceived(message);
            }
        }
    }

    public void NewLogin(string loginName)
    {
        StartCoroutine(PostDataCoroutine(loginName));
        // if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        // {
        //     PlayFabSettings.staticSettings.TitleId = "BE28C";
        // }
        //
        // PlayerPrefs.SetString("localLogin", Guid.NewGuid().ToString());
        // PlayerPrefs.SetString("localUsername", loginName);
        //
        // Debug.Log($"Registration { PlayerPrefs.GetString("localLogin")}");
        //
        //
        // var request = new LoginWithCustomIDRequest 
        //     { 
        //         CustomId = PlayerPrefs.GetString("localLogin"),
        //         CreateAccount = true
        //     };
        //
        // PlayFabClientAPI.LoginWithCustomID(request, OnRegisterSuccess, OnLoginFailure);
    }

    public void ChangeName(string newName)
    {
        StartCoroutine(PostDataCoroutine(newName));

        // var requestNameChange = new UpdateUserTitleDisplayNameRequest
        // {
        //     DisplayName = newName
        // };
        //
        // PlayerPrefs.SetString("localUsername", newName);
        //
        // PlayFabClientAPI.UpdateUserTitleDisplayName(requestNameChange, OnNameChangeSuccess, OnError);
        //
        // UIController.Instance.ShowSuccessPanel();
    }

    // private void OnError(PlayFabError error)
    // {
    //     Debug.LogWarning("Something went wrong with your first API call.  :(");
    //     Debug.Log("Here's some debug information:");
    //     Debug.Log(error.GenerateErrorReport());
    // }

    #region ReactionsToRequests

    // private void OnNameChangeSuccess(UpdateUserTitleDisplayNameResult obj)
    // {
    //     Debug.Log($"Name updated successfully to {obj.DisplayName}");
    // }

    // private void OnRegisterSuccess(LoginResult obj)
    // {
    //     Debug.Log("Congratulations, you are now registered!");
    //     ChangeName(PlayerPrefs.GetString("localUsername"));
    //     UIController.Instance.ShowSuccessPanel();
    // }
    //
    // private void OnLoginSuccess(LoginResult result)
    // {
    //     Debug.Log("Congratulations, on login!");
    //     UIController.Instance.ShowSuccessPanel();
    // }
    //
    private void OnLoginFailure()
    {
        Debug.LogWarning("No login data detected, new player");

        UIController.Instance.ShowLoginPanel();
    }

    private void OnError(UnityWebRequest request)
    {
        Debug.LogWarning("Error occured");
        Debug.Log($"{request.error}");
    }

    #endregion

    //GET
    IEnumerator GetDataCoroutine()
    {
        string url = $"ws://localhost:5143";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error occured");
                Debug.Log($"{request.error}");
                OnLoginFailure();
            }
            else
            {
                Debug.Log("Loaded successfully");
                UIController.Instance.ShowSuccessPanel();
                // outputArea.text = request.downloadHandler.text;
            }
        }
    }

    //POST
    IEnumerator PostDataCoroutine(string name)
    {
        name = "101";
        string url = "https://localhost:7232/player";
        // // WWWForm form = new WWWForm();
        // var plyr = new Player();
        int Id = 101;
        var data = JsonConvert.SerializeObject(Id);
        // form.AddField("Id", 101);

        List<IMultipartFormSection> wwwForm = new List<IMultipartFormSection>();
        wwwForm.Add(new MultipartFormDataSection("id", name));

        var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.uploadHandler.contentType = "application/json";
        
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            OnError(request);
        else
            Debug.Log("Data has been sent");
    }
}