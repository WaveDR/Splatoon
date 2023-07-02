using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[PunRPC]
public interface IPlayer
{
    public void Transform_Stat(int ammo, float speed, bool Squid, bool Human);
}
