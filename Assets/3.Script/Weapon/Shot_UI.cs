using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class Shot_UI : MonoBehaviourPun
{
    [Header("Shot_UI")]
    public GameObject[] weapon_Aim;

    public Image bowAim_UI;
    public Text killLog_UI;
    public GameObject killLog_Obj;
    public GameObject enemyData_Obj;
    public Text enemyName_UI;
    public Text enemyScore_UI;

    public Image ammoBack_UI;
    public Image ammoNot_UI;
    public Text name_UI;
    public Text score_UI;


}
