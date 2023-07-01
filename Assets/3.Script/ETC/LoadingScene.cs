using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    static string next_Scene;

    public static void LoadScene(string scene_Name)
    {
        next_Scene = scene_Name;
        SceneManager.LoadScene("Loading");
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Load_Scene_Process()); 
    }
    IEnumerator Load_Scene_Process()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(next_Scene);
        op.allowSceneActivation = false;
        yield return new WaitForSeconds(2f);

        op.allowSceneActivation = true;


    }

}
