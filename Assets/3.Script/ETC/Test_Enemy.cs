using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Enemy : MonoBehaviour
{
    PlayerInput input;
    PlayerTeams team;
    PlayerController con;

    float time;
    private void Awake()
    {
        con = GetComponent<PlayerController>();
        input = GetComponent<PlayerInput>();
        team = GetComponent<PlayerTeams>();
    }

    private void Update()
    {
        input.move_Vec = Vector3.zero;
        

        time += Time.deltaTime;

        if(time >= 2)
        {
            StartCoroutine(Dummy_Attack());
            time = 0;
        }
    }

    IEnumerator Dummy_Attack()
    {
        input.fire = true;
        yield return new WaitForSeconds(1f);
        input.fire = false;
    }
}
