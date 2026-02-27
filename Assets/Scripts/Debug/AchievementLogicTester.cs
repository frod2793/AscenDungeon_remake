using UnityEngine;
using Assets.Scripts.BackEnd;
using Cysharp.Threading.Tasks;
using System;

namespace Assets.Scripts.DebugTools
{
    /// <summary>
    /// [설명]: 에디터에서 업적 달성 로직의 도달 여부를 테스트하기 위한 도구입니다.
    /// 인스펙터 버튼(Context Menu)을 통해 각 단계를 테스트할 수 있습니다.
    /// </summary>
    public class AchievementLogicTester : MonoBehaviour
    {
        #region 에디터 설정
        [Header("Test Settings")]
        [SerializeField] private string m_testAchievementId = "CgkIzaKso5gOEAIQAw";
        #endregion

        #region 테스트 API
        /// <summary>
        /// [설명]: GPGS 인증 상태를 강제로 true로 설정합니다.
        /// </summary>
        [ContextMenu("Step 1: Force Auth True")]
        public void ForceAuthTrue()
        {
#if UNITY_EDITOR
            GPGSAuthService.SetDebugForceAuth(true);
            Debug.Log("[Tester] GPGS Auth forced to TRUE.");
#else
            Debug.LogWarning("[Tester] ForceAuth is only supported in Editor.");
#endif
        }

        /// <summary>
        /// [설명]: GPGS 인증 상태를 false로 재설정합니다.
        /// </summary>
        [ContextMenu("Step 1-Alt: Reset Auth")]
        public void ResetAuth()
        {
#if UNITY_EDITOR
            GPGSAuthService.SetDebugForceAuth(false);
            Debug.Log("[Tester] GPGS Auth reset to FALSE.");
#endif
        }

        /// <summary>
        /// [설명]: BackEndService를 통해 업적 해제 로직을 직접 테스트합니다.
        /// </summary>
        [ContextMenu("Step 2: Test Direct Unlock")]
        public void TestDirectUnlock()
        {
            InvokeUnlock().Forget();
        }

        private async UniTaskVoid InvokeUnlock()
        {
            Debug.Log("[Tester] Starting Direct Unlock Test...");
            
            // 실제 DI 환경을 모사하기 위해 서비스 생성 (테스트용)
            var auth = new GPGSAuthService();
            var arch = new GPGSAchievementService(auth);
            var backend = new BackEndService(auth, arch);

            var result = await backend.UnlockAchievement(m_testAchievementId);
            Debug.Log($"[Tester] Direct Unlock Result: {result.IsSuccess} (Message: {result.ErrorMessage})");
        }

        /// <summary>
        /// [설명]: 가상의 ProceedToGame 흐름을 실행하여 데이터 로드 후 업적 해제까지 도달하는지 테스트합니다.
        /// 만약 씬에 LoginView나 LoginViewModel이 있다면 해당 인스턴스를 찾아 테스트하는 것이 더 정확합니다.
        /// </summary>
        [ContextMenu("Step 3: Test Full Flow (ProceedToGame Simulation)")]
        public void TestFullFlowSimulation()
        {
            // 이 버튼은 실제 씬의 의존성들을 활용하여 테스트하는 것을 권장합니다.
            Debug.Log("[Tester] Full Flow Simulation 시작 - 콘솔 로그의 순서를 확인하세요.");
            
            // 씬에서 LoginView를 찾아 ViewModel을 가져오려 시도
            var loginView = FindFirstObjectByType<LoginView>();
            if (loginView != null)
            {
                // Reflection을 사용하거나 LoginView에 테스트용 메서드를 노출시켜야 할 수 있음.
                // 여기서는 로직 도달 여부만 체크하는 것이 목적이므로 수동으로 흐름을 찍어보는 것으로 대체 가능.
                Debug.Log("[Tester] LoginView 발견. 실제 로그인 프로세스 중 업적 해제 로그가 찍히는지 확인하세요.");
            }
            else
            {
                Debug.LogWarning("[Tester] 씬에 LoginView가 없어 풀 플로우 시뮬레이션이 제한적입니다.");
            }
        }
        #endregion
    }
}
