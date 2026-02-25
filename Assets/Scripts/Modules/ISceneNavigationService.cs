using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Modules
{
    public interface ISceneNavigationService
    {
        UniTask LoadSceneAsync(int sceneIndex, bool useLoadingUI = true);
        UniTask LoadSceneAsync(SceneIndex sceneIndex, bool useLoadingUI = true);
    }
}
