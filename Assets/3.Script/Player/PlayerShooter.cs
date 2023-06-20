using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Weapon { Brush, Gun, Bow }
public class PlayerShooter : MonoBehaviour
{
    [Header("Weapon")]
    
    public Weapon WeaponType;
    public GameObject[] weapons;
    public GameObject ammo_Back;
    public Shot_System weapon_Shot;
    public int weaponNum;

    private int reloadAmmo = 1;

    [Header("Attack Rate")]

    [SerializeField] private float fireMaxTime;
    [SerializeField] private float fireRateTime;
   // [SerializeField] private bool fireReady;

    [Header("IK Transform")]
    public Transform weapon_Pivot;
    public Transform[] left_HandMount;
    public Transform[] right_HandMount;

    private PlayerInput _player_Input;
    private Animator _player_Anim;
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out _player_Input);
        TryGetComponent(out _player_Anim);
    }

    private void OnEnable()
    {
        //선택에 따라 활성화 disable도 동일한 방식

        switch (WeaponType)
        {
            case Weapon.Brush:
                weaponNum = 0;
                break;

            case Weapon.Gun:
                weaponNum = 1;
                break;

            case Weapon.Bow:
                weaponNum = 2;
                break;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(true);
            if (weaponNum != i)
                weapons[i].SetActive(false);
        }
        _player_Anim.SetInteger("WeaponNum", weaponNum);

        weapon_Shot = GetComponentInChildren<Shot_System>();
        fireMaxTime = weapon_Shot.weapon_Stat.fire_Rate;
    }

    void Update()
    {
        //공격로직
        Fire_Paint();
    }



    private void OnDisable()
    {
        weapons[weaponNum].SetActive(false);
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (WeaponType != Weapon.Brush)
        {
            // 총의 기준점을 3D 모델 오른쪽 팔꿈치로 이동
            weapon_Pivot.position = _player_Anim.GetIKHintPosition(AvatarIKHint.RightElbow) + new Vector3(-0.05f, -0.1f, 0.07f);

            //왼쪽팔
            _player_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].rotation);

            //오른팔

            _player_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.RightHand, right_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.RightHand, right_HandMount[weaponNum].rotation);
        }
    }
    private void Fire_Paint()
    {
        if (_player_Input.fire)
        {
            fireRateTime += Time.deltaTime;
            ammo_Back.transform.localScale = 
            new Vector3(ammo_Back.transform.localScale.x, weapon_Shot.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);

            if(fireRateTime >= fireMaxTime)
            {
                weapon_Shot.Shot();
                fireRateTime = 0;
            }
        }
        else
        {
            fireRateTime = 0;
        }

    }

    public void Reload_Ammo()
    {
        if(weapon_Shot.weapon_CurAmmo <= weapon_Shot.weapon_MaxAmmo)
        {
            weapon_Shot.weapon_CurAmmo += Time.deltaTime;

            ammo_Back.transform.localScale =
           new Vector3(ammo_Back.transform.localScale.x, weapon_Shot.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
        }

    }
}
