using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    //�̵� ��ǥ Scene �̸�
    static string next_Scene;
    void Start()
    {
        //ȣ��Ǵ� ��� LoadScene �ڷ�ƾ ����
        StartCoroutine(Load_Scene_Process()); 
    }

   public static void LoadScene(string scene_Name)
    {
        //���� �޼���� LoadingScene ȣ��
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
