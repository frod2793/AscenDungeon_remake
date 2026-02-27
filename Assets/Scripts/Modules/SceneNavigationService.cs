using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Modules
{
    public class SceneNavigationService : ISceneNavigationService
    {
        // Town 씬의 빌드 인덱스를 하드코딩으로 안전하게 유지합니다. (Build Settings에 따라 조정 가능)
        private const int TownSceneIndex = 2;
    public async UniTask LoadSceneAsync(int sceneIndex, bool useLoadingUI = true)
    {
        await LoadSceneInternal(sceneIndex, useLoadingUI);
    }

    public async UniTask LoadSceneAsync(SceneIndex sceneIndex, bool useLoadingUI = true)
    {
        await LoadSceneInternal((int)sceneIndex, useLoadingUI);
    }

    private async UniTask LoadSceneInternal(int sceneIndex, bool useLoadingUI)
    {
        GameObject loading = null;
        UnityEngine.Debug.Log($"[SceneNavigation] 씬 로드 시작 — 인덱스: {sceneIndex}");
        
        if (useLoadingUI)
        {
            loading = GameObject.FindWithTag("Loading");
            if (loading != null)
            {
                loading.SetActive(true);
                UnityEngine.Debug.Log("[SceneNavigation] 로딩 UI 활성화");
            }
        }

        try
        {
            // 씬 로딩 시작 전 1프레임 대기하여 이전 상태(데이터 로드 등)가 완전히 정리되도록 함
            await UniTask.NextFrame();

            var op = SceneManager.LoadSceneAsync(sceneIndex);
            if (op == null)
            {
                UnityEngine.Debug.LogError($"[SceneNavigation] LoadSceneAsync 실패: 인덱스 {sceneIndex} 가 Build Settings에 없을 수 있습니다.");
                return;
            }

            // 진행률 로그를 위한 핸들러
            var progress = new System.Progress<float>(p => 
            {
                UnityEngine.Debug.Log($"[SceneNavigation] 씬 로드 진행 중 — 인덱스: {sceneIndex}, Progress: {p * 100:F1}%");
            });

            await op.ToUniTask(progress);
            UnityEngine.Debug.Log($"[SceneNavigation] 씬 로드 완료 — 인덱스: {sceneIndex}");

            // 필요한 씬 초기화 예시: Town 씬 진입 시 인벤토리 클리어
            if (sceneIndex == TownSceneIndex)
            {
                if (Inventory.Instance != null)
                {
                    Inventory.Instance.Clear();
                    UnityEngine.Debug.Log("[SceneNavigation] Town 씬 진입 시 인벤토리 초기화 완료");
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[SceneNavigation] 씬 로드 중 예외 발생: {e.Message}");
        }
        finally
        {
            if (loading != null)
            {
                loading.SetActive(false);
                UnityEngine.Debug.Log("[SceneNavigation] 로딩 UI 비활성화");
            }
        }
    }
    }
}
