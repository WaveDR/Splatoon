using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer
{

    public void Transform_Stat(int ammo, float speed, bool Squid, bool Human);

    public void Transform_Mesh(bool squid, bool human);
}
