using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    [SerializeField] 
    private int LoadSceneIndex;

    public void SceneLoad()
    {
        SceneManager.LoadScene(LoadSceneIndex);

        if (LoadSceneIndex == (int)SceneIndex.Town)
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.Clear();
            }
        }
    }

    public void SceneLoad(int index)
    {
        SceneManager.LoadScene(index);

        // Town 씬(3) 진입 시 인벤토리 초기화
        if (index == (int)SceneIndex.Town)
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.Clear();
            }
        }
    }

    public void SceneLoad(SceneIndex sceneIndex)
    {
        SceneLoad((int)sceneIndex);
    }
}
