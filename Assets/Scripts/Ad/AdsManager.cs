using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Assets.Scripts.Ad
{
    /// <summary>
    /// [설명]: 광고 시스템을 관리하는 매니저 클래스입니다.
    /// MonoBehaviour 기반으로 초기화를 수행하며, 서비스 인스턴스를 관리합니다.
    /// </summary>
    public class AdsManager : MonoBehaviour
    {
        #region 에디터 설정
        [SerializeField, Tooltip("애드몹 설정 자산")]
        private AdMobConfig m_config;
        #endregion

        #region 내부 변수
        private static AdsManager s_instance;
        private IAdService m_adService;
        #endregion

        #region 프로퍼티
        /// <summary>
        /// [설명]: 광고 서비스 인스턴스를 반환합니다.
        /// </summary>
        public static IAdService Service => s_instance?.m_adService;

        /// <summary>
        /// [설명]: 기존 Ads.Instance와의 호환성을 위한 정적 인스턴스입니다.
        /// </summary>
        public static AdsManager Instance => s_instance;
        #endregion

        #region 유니티 생명주기
        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeService().Forget();
        }
        #endregion

        #region 초기화 로직
        /// <summary>
        /// [설명]: 광고 서비스를 초기화합니다. 외부(로그인 단계)에서 명시적으로 호출할 수 있습니다.
        /// </summary>
        public async UniTask InitializeService()
        {
            if (m_adService != null) return; // 이미 초기화됨

            if (m_config == null)
            {
                Debug.LogError("[AdsManager] AdMobConfig is not assigned!");
                return;
            }

            m_adService = new AdMobService(m_config);
            await m_adService.InitializeAsync();
            
            Debug.Log("[AdsManager] Ad Service Initialized Successfully.");
        }
        #endregion

        #region 공개 API (기존 Ads 호환용)
        /// <summary>
        /// [설명]: 전면 광고를 표시합니다.
        /// </summary>
        public void ShowFrontAd()
        {
            m_adService?.ShowInterstitialAd();
        }

        /// <summary>
        /// [설명]: 리워드 광고를 표시합니다.
        /// </summary>
        public void ShowRewardAd()
        {
            m_adService?.ShowRewardedAd(null);
        }

        /// <summary>
        /// [설명]: 전면 광고 종료 이벤트를 등록합니다.
        /// </summary>
        public void ClosedADEvent(Action action)
        {
            // Note: 현재 인터페이스는 Show 호출 시 콜백을 받으므로, 
            // 호출부에서 직접 콜백을 넘기는 방식으로 리팩토링하는 것이 좋습니다.
            // 호환성을 위해 ShowInterstitialAd 내에서 처리되도록 서비스가 설계되었습니다.
        }
        #endregion
    }
}
