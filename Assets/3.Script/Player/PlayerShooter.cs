using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Weapon { Brush, Gun, Bow }
public class PlayerShooter : MonoBehaviour
{
    [Header("Weapon")]
    public Weapon WeaponType;
    public GameObject[] weapons;
    private int weaponNum;

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
        //���ÿ� ���� Ȱ��ȭ disable�� ������ ���

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
    }

    // Update is called once per frame
    void Update()
    {
        //���ݷ���
    }

    private void OnDisable()
    {
        weapons[weaponNum].SetActive(false);
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (WeaponType != Weapon.Brush)
        {
            // ���� �������� 3D �� ������ �Ȳ�ġ�� �̵�
            weapon_Pivot.position = _player_Anim.GetIKHintPosition(AvatarIKHint.RightElbow) + new Vector3(-0.05f, -0.1f, 0.07f);

            //������
            _player_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].rotation);

            //������

            _player_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.RightHand, right_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.RightHand, right_HandMount[weaponNum].rotation);
        }
    }
}
