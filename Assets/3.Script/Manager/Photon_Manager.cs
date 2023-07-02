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
    public bool isCreateRoom;
    public ServerSettings setting = null;
    public GameObject matching_UI;

    //Player Prefabs

    public GameObject playerPrefabs;

    public PlayerController player_Con;
    public PlayerInput player_Input;
    public PlayerTeams player_team;
    public PlayerShooter player_shot;

    public Text stateUI;

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
        PhotonNetwork.LoadLevel("InGame");

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
        stateUI.text = "2�� ��";
    }



    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Room Join Success");
        stateUI.text = "�濡 �����մϴ�.";

        photonView.RPC("Set_PlayerInfo", RpcTarget.All);
        //���߿� ������ �÷��̾� ������ && �� ���̸� ���� ���� ���� �� �ֵ��� ��������
        StartCoroutine(Player_Spawn());
    }
 
    IEnumerator Player_Spawn()
    {
        yield return new WaitForSeconds(1f);

        PhotonNetwork.Instantiate(playerPrefabs.name, Vector3.zero, Quaternion.identity);

        GameManager.Instance.FindPlayer();
        GameManager.Instance.SetPlayerPos();
        photonView.RPC("Player_Spawn", RpcTarget.Others);
    }

    private void Update()
    {
        if (isCreateRoom && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= max_Player)
            {
                GameManager.Instance.isLobby = false;
            }
        }
    }
    #endregion

    [PunRPC]
    public void Set_PlayerInfo()
    {
        player_shot = playerPrefabs.GetComponent<PlayerShooter>();
        player_team = playerPrefabs.GetComponent<PlayerTeams>();
        player_Input = playerPrefabs.GetComponent<PlayerInput>();

        player_team.team = set_Manager.team;
        player_shot.WeaponType = set_Manager.weapon;
        player_Input.player_Name = set_Manager.player_Name.text;
    }

}
