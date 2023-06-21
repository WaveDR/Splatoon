using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Weapon { Brush, Gun, Bow }
public class PlayerShooter : MonoBehaviour
{
    [Header("Weapon")]
    
    public Weapon WeaponType;
    public GameObject[] weapon_Obj;
    public Shot_System weapon;
    public int weaponNum;            // 총 번호
    public GameObject ammo_Back;     // 총알 통 애니메이션용

    private bool _isFire;
    private float JumpRot;
    private float weapon_JumpRot
    {
        get { return JumpRot; }
        set { JumpRot = value;
              JumpRot = Mathf.Clamp(JumpRot, -90, 0);
            }
    }

    [Header("Attack Rate")]

    [SerializeField] private float fireMaxTime;
    [SerializeField] private float fireRateTime;
   // [SerializeField] private bool fireReady;

    [Header("IK Transform")]
    public Transform weapon_Pivot;
    public Transform[] left_HandMount;
    public Transform[] right_HandMount;


    [Header("Player Component")]
    private PlayerController _Player_Con;
    private PlayerInput _player_Input;
    private Animator _player_Anim;
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out _player_Input);
        TryGetComponent(out _Player_Con);
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

        for (int i = 0; i < weapon_Obj.Length; i++)
        {
            weapon_Obj[i].SetActive(true);
            if (weaponNum != i)
                weapon_Obj[i].SetActive(false);
        }
        _player_Anim.SetInteger("WeaponNum", weaponNum);

        weapon = GetComponentInChildren<Shot_System>();
        fireMaxTime = weapon.weapon_Stat.fire_Rate;
    }

    void Update()
    {
        //공격로직
        Fire_Paint();
    }

    private void OnDisable()
    {
        weapon_Obj[weaponNum].SetActive(false);
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
        

        if (WeaponType == Weapon.Bow)
        {
            if (_Player_Con._isJump && weapon_JumpRot < 0) // 점프 O
            {
                weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot += 540 * Time.deltaTime);
            }
            else if (!_Player_Con._isJump && weapon_JumpRot > -90 ) //점프 X
            {
                weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot -= 540 * Time.deltaTime);
            }
        }


        if (_player_Input.fire)
        {
            fireRateTime += Time.deltaTime;
            ammo_Back.transform.localScale = 
            new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
            _isFire = true;

            if(fireRateTime >= fireMaxTime && weapon.weapon_CurAmmo > 0)
            {
                _player_Anim.SetBool("isFire", true);
                weapon.Shot();
                fireRateTime = 0;
            }
            else
            {
                _player_Anim.SetBool("isFire", false);
            }
        }
        else
        {
            fireRateTime = 0;
            _player_Anim.SetBool("isFire", false);

            _isFire = false;
        }
    }

    public void Reload_Ammo()
    {
        if (weapon.weapon_CurAmmo <= weapon.weapon_MaxAmmo && !_isFire)
        {
            weapon.weapon_CurAmmo += Time.deltaTime * 20f;

            ammo_Back.transform.localScale =
           new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
        }

    }
}