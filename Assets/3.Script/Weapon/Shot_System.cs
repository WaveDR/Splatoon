using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot_System : MonoBehaviour
{
    public Weapon weaponType;
    [SerializeField] private Transform firePoint_File;
    [SerializeField] private Transform[] firePoint;

  // Start is called before the first frame update
    void Awake()
    {
        firePoint_File = transform.GetChild(0);
        firePoint = new Transform[firePoint_File.childCount];
        for (int i = 0; i < firePoint.Length; i++)
        {
            firePoint[i] = firePoint_File.GetChild(i).transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
