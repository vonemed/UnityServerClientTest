using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedLibrary.Packets;
using SharedLibrary.Packets.ClientToServer;
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
        // var url = "localhost:5143";
        // var uri = new Uri(@"ws://193.124.129.94:5143");
        var uri = new Uri(@"ws://localhost:5143/ws");
        var cancellationToken = new CancellationTokenSource();
        // cancellationToken.CancelAfter(5000);
        
        Debug.Log("Connecting....");

        while (!cancellationToken.IsCancellationRequested)
        {
            using (clientSocket = new ClientWebSocket())
            {
                try
                {
                    Debug.Log("<color=cyan>WebSocket connecting.</color>");
                    clientSocket.Options.AddSubProtocol("Tls");
                    await clientSocket.ConnectAsync(uri, cancellationToken.Token);
                    
                    UIController.Instance.ShowLoginPanel();
                    
                    Debug.Log("<color=cyan>WebSocket receiving.</color>");
                    // await Send();
                    await Receive();

                    // Debug.Log("<color=cyan>WebSocket closed.</color>");
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

    private async Task Send<T>(T packet)
    {
        Console.WriteLine("Sending packet....");
        
        var packagePacket = JsonConvert.SerializeObject(packet);
        
        var package = new PacketContainer
        {
            Key = packet.GetType().FullName,
            Data = packagePacket
        };

        var packedPackage = JsonConvert.SerializeObject(package);
        
        Console.WriteLine($"Container items -> {package}");
        
        ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(packedPackage));
        
        if (clientSocket.State == WebSocketState.Open)
        {
            await clientSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task NewLogin(string loginName)
    {
        var packet = new LoginRequestPacket
        {
            Username = loginName
        };
        
        await Send(packet);
    }

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
    
}