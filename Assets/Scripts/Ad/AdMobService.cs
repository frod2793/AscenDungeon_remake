using System;
using UnityEngine;
using GoogleMobileAds.Api;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Ad
{
    /// <summary>
    /// [설명]: Google Mobile Ads SDK를 사용하는 광고 서비스 구현체입니다.
    /// POCO 클래스로 작성되어 아키텍처 의존성을 최소화합니다.
    /// </summary>
    public class AdMobService : IAdService
    {
        #region 내부 변수
        private readonly AdMobConfig m_config;
        private InterstitialAd m_interstitialAd;
        private RewardedAd m_rewardedAd;

        private bool m_isInterstitialLoading = false;
        private bool m_isRewardedLoading = false;
        private bool m_isInitialized = false;
        #endregion

        #region 초기화
        public AdMobService(AdMobConfig config)
        {
            m_config = config;
        }

        public async UniTask InitializeAsync()
        {
            if (m_isInitialized) return;

            var utcs = new UniTaskCompletionSource<InitializationStatus>();
            MobileAds.Initialize(status => utcs.TrySetResult(status));
            
            await utcs.Task;
            m_isInitialized = true;

            // 초기 광고 로드
            LoadInterstitialAd();
            LoadRewardedAd();
        }
        #endregion

        #region 전면 광고 로직
        public void LoadInterstitialAd()
        {
            // [방어 코드]: 이미 로딩 중이면 중복 요청 방지
            if (m_isInterstitialLoading) return;

            if (m_interstitialAd != null)
            {
                m_interstitialAd.Destroy();
                m_interstitialAd = null;
            }

            Debug.Log("[AdMob] 전면 광고 로딩 시작...");
            m_isInterstitialLoading = true;

            // 광고 요청 생성 및 로드
            AdRequest request = new AdRequest();
            InterstitialAd.Load(m_config.InterstitialId, request, (ad, error) =>
            {
                m_isInterstitialLoading = false;

                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdMob] 전면 광고 로드 실패: {error}");
                    return;
                }

                Debug.Log("[AdMob] 전면 광고 로드 완료.");
                m_interstitialAd = ad;
            });
        }

        public void ShowInterstitialAd(Action onClosed = null)
        {
            if (IsInterstitialAdLoaded())
            {
                if (onClosed != null)
                {
                    m_interstitialAd.OnAdFullScreenContentClosed += () => 
                    {
                        onClosed.Invoke();
                        LoadInterstitialAd(); // 다음 광고 미리 로드
                    };
                }
                else
                {
                    m_interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitialAd();
                }

                m_interstitialAd.Show();
            }
            else
            {
                Debug.LogWarning("[AdMob] 전면 광고가 준비되지 않았습니다.");
                onClosed?.Invoke();
                LoadInterstitialAd();
            }
        }

        public bool IsInterstitialAdLoaded()
        {
            return m_interstitialAd != null && m_interstitialAd.CanShowAd();
        }
        #endregion

        #region 리워드 광고 로직
        public void LoadRewardedAd()
        {
            // [방어 코드]: 이미 로딩 중이면 중복 요청 방지
            if (m_isRewardedLoading) return;

            if (m_rewardedAd != null)
            {
                m_rewardedAd.Destroy();
                m_rewardedAd = null;
            }

            Debug.Log("[AdMob] 보상 광고 로딩 시작...");
            m_isRewardedLoading = true;

            AdRequest request = new AdRequest();
            RewardedAd.Load(m_config.RewardedId, request, (ad, error) =>
            {
                m_isRewardedLoading = false;

                if (error != null || ad == null)
                {
                    // [상태 분석]: 'Prefab Ad is Null'은 보통 에디터 환경의 테스트용 프리팹 누락 시 발생함
                    Debug.LogError($"[AdMob] 보상 광고 로드 실패: {error}\n(Tip: 에디터 환경이라면 GoogleMobileAds 설정을 확인하거나 실제 기기에서 테스트하세요)");
                    return;
                }

                Debug.Log("[AdMob] 보상 광고 로드 완료.");
                m_rewardedAd = ad;
            });
        }

        public void ShowRewardedAd(Action onRewarded, Action onClosed = null)
        {
            if (IsRewardedAdLoaded())
            {
                m_rewardedAd.OnAdFullScreenContentClosed += () => 
                {
                    onClosed?.Invoke();
                    LoadRewardedAd(); // 다음 광고 미리 로드
                };

                m_rewardedAd.Show((Reward reward) => 
                {
                    onRewarded?.Invoke();
                });
            }
            else
            {
                Debug.LogWarning("[AdMob] 보상 광고이 준비되지 않았습니다.");
                LoadRewardedAd();
            }
        }

        public bool IsRewardedAdLoaded()
        {
            return m_rewardedAd != null && m_rewardedAd.CanShowAd();
        }
        #endregion
    }
}
