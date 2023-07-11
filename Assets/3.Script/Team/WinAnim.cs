using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinAnim : MonoBehaviour
{
    public Animator judge_Anim;
    private void Awake()
    {
        TryGetComponent(out judge_Anim);
    }
    public void Win()
    {
        judge_Anim.SetBool("isWin", true);
    }
    public void Lose()
    {
        judge_Anim.SetBool("isLose", true);
    }
}
