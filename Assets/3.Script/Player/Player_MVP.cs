using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MVP : MonoBehaviour
{
    public Material[] teamColor;
    public SkinnedMeshRenderer[] changeMesh;
    public MeshRenderer rend;
    public void TeamChange(ETeam team)
    {
        if (team == ETeam.Yellow)
        {
            foreach(SkinnedMeshRenderer rendMesh in changeMesh)
            {
                rendMesh.material = teamColor[0];
                rend.material = teamColor[0];
            }
        }
        else
        {
            foreach (SkinnedMeshRenderer rendMesh in changeMesh)
            {
                rendMesh.material = teamColor[1];
                rend.material = teamColor[1];
            }
        }
    }
}
