using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player_Camera : MonoBehaviourPun
{
    [Header("Rotation_Dir")]
    public Shot_System weapon_DirY;
    public Transform player_DirY;
    public float eulerX;
    public float _eulerY;
    public float rotateSpeed;
    public float eulerY
    {
        get { return _eulerY; }
        set { _eulerY = value;
            _eulerY = Mathf.Clamp(_eulerY, -65f, 65f);
        }
    }

    [Header("Camera")]
    public Cinemachine.CinemachineVirtualCamera cam_Obj;
    public PlayerController _player_Con;

    private void Awake()
    {
        TryGetComponent(out _player_Con);
        SelectCamera();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!photonView.IsMine) return;
        if(!_player_Con.isStop)

        CameraRotation();
    }

    //Player Cam Rotation
    private void CameraRotation()
    {
        eulerX += Input.GetAxis("Mouse X") * Time.deltaTime * rotateSpeed;
        eulerY += -Input.GetAxis("Mouse Y") * Time.deltaTime * rotateSpeed;
        transform.localRotation = Quaternion.Euler(0, eulerX, 0);
        player_DirY.localRotation = Quaternion.Euler(eulerY,0 , 0);

        if(_player_Con._enemy == null)
        {
            if (weapon_DirY.weaponType == EWeapon.Bow && !_player_Con._isJump)
                weapon_DirY.transform.localRotation = Quaternion.Euler(eulerY, 0, -90f);
            else if (weapon_DirY.weaponType == EWeapon.Gun)
                weapon_DirY.transform.localRotation = Quaternion.Euler(eulerY, 0, 0);
        }
    }

    //Player Cam ����
    public void SelectCamera()
    {
        if (photonView.IsMine && _player_Con._enemy == null)
        {
            cam_Obj = GameObject.FindGameObjectWithTag("PlayerCam").GetComponent<Cinemachine.CinemachineVirtualCamera>();
            cam_Obj.Follow = player_DirY;
            cam_Obj.LookAt = player_DirY;
        }
    }
}
