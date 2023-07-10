using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    //이동 목표 Scene 이름
    static string next_Scene;
    void Start()
    {
        //호출되는 즉시 LoadScene 코루틴 실행
        StartCoroutine(Load_Scene_Process()); 
    }

   public static void LoadScene(string scene_Name)
    {
        //정적 메서드로 LoadingScene 호출
        next_Scene = scene_Name;
        SceneManager.LoadScene("Loading");
    }
    IEnumerator Load_Scene_Process()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(next_Scene);
        op.allowSceneActivation = false;
        yield return new WaitForSeconds(1.5f);
        op.allowSceneActivation = true;
    }

}
