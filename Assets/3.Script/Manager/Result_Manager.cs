using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Result_Manager : MonoBehaviour
{
    [Header("PlayerData")]
    [Header("=========================================")]
    public GameObject playerPrefab;
    public Transform spawnPos;

    [Header("Data UI")]
    [Header("=========================================")]
    public Text player_Name;
    public Text player_Score;
    public Text player_Weapon;
    public Text player_Team;

    public Photon_Manager photonManager;
    public Animator anim;

    private void Awake()
    {
        TryGetComponent(out anim);
        photonManager = FindObjectOfType<Photon_Manager>();
    }
    void Start()
    {
        anim.SetBool("FadeOut", true);
        player_Score.text = PlayerPrefs.GetString("Score");
        player_Name.text =PlayerPrefs.GetString("Name");
        player_Weapon.text =PlayerPrefs.GetString("Weapon");
        player_Team.text = PlayerPrefs.GetString("Team");
        Instance_Model();
    }

    void Instance_Model()
    {
        GameObject player = Instantiate(playerPrefab, spawnPos.position, Quaternion.Euler(0,-135f, 0));
        player.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        player.AddComponent<DragAndRotate>();
        PlayerController player_Con = player.GetComponent<PlayerController>();
        switch (PlayerPrefs.GetString("Weapon"))
        {
            case "Brush":
                player_Con._player_shot.WeaponType = EWeapon.Brush;
                break;
            case "Gun":
                player_Con._player_shot.WeaponType = EWeapon.Gun;
                break;
            case "Bow":
                player_Con._player_shot.WeaponType = EWeapon.Bow;
                break;
        }

        switch (PlayerPrefs.GetString("Team"))
        {
            case "Yellow":
                player_Con.player_Team.team = ETeam.Yellow;
                break;
            case "Blue":
                player_Con.player_Team.team = ETeam.Blue;
                break;
        }

        player_Con.player_Input.enabled = false;
        player_Con.player_rigid.useGravity = false;
        player_Con.UI_On_Off(false);
        player.SetActive(false);
        player.SetActive(true);
        player_Con.enabled = false;
    }

    public void Goto_Lobby()
    {
        Destroy(photonManager.gameObject);
        LoadingScene.LoadScene("Lobby");
    }
}
