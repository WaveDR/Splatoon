using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    private readonly string game_Version = "1";

    public int max_Player;
    public ServerSettings setting = null;

    //Player Prefabs

    public GameObject playerPrefabs;


    private void Awake()
    {
    }
    private void Start()
    {
        //Connect();
    }
    private void OnApplicationQuit()
    {
        DisConnect();
    }
    #region 서버 관련 CallBack Method
    //Connect Tto Master -

    public void Connect()
    {
        PhotonNetwork.GameVersion = game_Version;

        //Master 서버 연결

        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port,"");

        Debug.Log("Connect to Master Server...");
    }

    public void DisConnect()
    {
        PhotonNetwork.Disconnect(); //Photon Server DisConnect
    }

    //CallBack Methord

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connect To Master Server Join.");

        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Join Lobby Zone");
        base.OnJoinedLobby();
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Not Empty Room...");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = max_Player });
        Debug.Log("Created Room");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Room Join Success");
        PhotonNetwork.Instantiate(playerPrefabs.name, Vector3.zero, Quaternion.identity);
        
    }

    #endregion
}
