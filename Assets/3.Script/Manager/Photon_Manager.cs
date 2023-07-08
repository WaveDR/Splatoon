using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    public static Photon_Manager Instance = null;
    public Setting_Manager set_Manager;
    private readonly string game_Version = "1";

    public int max_Player;
    public int max_PlayerCount;
    public bool isCreateRoom;
    public ServerSettings setting = null;
    public GameObject matching_UI;

    //Player Prefabs

    public GameObject playerPrefabs;

    public Text stateUI;

    private bool isReady;
    public GameObject start_Btn;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        set_Manager.GetComponentInChildren<Setting_Manager>();
    }
    private void Start()
    {
        //Connect();
        PhotonNetwork.LogLevel = PunLogLevel.Informational;
    }

    private void OnApplicationQuit()
    {
        DisConnect();
    }
    #region ���� ���� CallBack Method
    //Connect Tto Master -

    public void SetRoom_MaxPlayer(int i)
    {
        max_Player = i;
        isCreateRoom = true;
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = max_Player });
        Debug.Log("Created Room");
        stateUI.text = "�� ����";
        matching_UI.SetActive(false);
    }

    public void Matching_Room()
    {
        Connect();
    }
    public void Connect()
    {
        PhotonNetwork.GameVersion = game_Version;

        //Master ���� ����

        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port,"");

        Debug.Log("Connect to Master Server...");
        stateUI.text = "������ �����մϴ�...";
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
        stateUI.text = "������ ����ƽ��ϴ�.";
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master Server");
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Join Lobby Zone");
        stateUI.text = "�κ� ����ƽ��ϴ�.";

        base.OnJoinedLobby();
        PhotonNetwork.JoinRandomRoom();

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Not Empty Room...");
        stateUI.text = "1 vs 1 ��ġ!";
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.LoadLevel("InGame");
        Debug.Log("Room Join Success");
        stateUI.text = "�濡 �����մϴ�.";
        start_Btn.SetActive(true);
        //���߿� ������ �÷��̾� ������ && �� ���̸� ���� ���� ���� �� �ֵ��� ��������
        StartCoroutine(Player_Spawn());
    }

    IEnumerator Player_Spawn()
    {
        yield return new WaitForSeconds(1f);
        GameObject player = PhotonNetwork.Instantiate(playerPrefabs.name, Vector3.zero, Quaternion.identity);
        PlayerController player_Con = player.GetComponent<PlayerController>();
       
        player_Con.photonView.RPC("Player_Set", RpcTarget.AllBuffered, 
            player_Con.player_Team.team,player_Con._player_shot.WeaponType,player_Con.player_Input.player_Name);
  
        GameManager.Instance.SetPlayerPos();
    }
    #endregion

    private void Update()
    {
       //if (PhotonNetwork.InRoom)
       //{
       //    if(isCreateRoom && PhotonNetwork.CurrentRoom.PlayerCount >= max_Player && GameManager.Instance.isLobby)
       //    {
       //        GameManager.Instance.photonView.RPC("isLobby_Server", RpcTarget.AllBuffered);
       //    }
       //}
    }
}
