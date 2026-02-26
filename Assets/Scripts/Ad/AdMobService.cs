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

        // [추가]: 콜백 중복 실행 방지를 위한 캐싱 변수
        private Action m_onInterstitialClosed;
        private Action m_onRewardedClosed;
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

            Debug.Log("[AdMob] 전면 광고 로딩 시작... (Loading Interstitial Ad)");
            m_isInterstitialLoading = true;

            // 광고 요청 생성 및 로드
            AdRequest request = new AdRequest();
            InterstitialAd.Load(m_config.InterstitialId, request, (ad, error) =>
            {
                m_isInterstitialLoading = false;

                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdMob] 전면 광고 로드 실패 (Interstitial Ad Load Failed): {error.GetMessage()} [Code: {error.GetCode()}]");
                    
                    // [해결]: 로드 실패 시 일정 시간 후 재시도
                    RetryLoadInterstitialAd().Forget();
                    return;
                }

                Debug.Log($"[AdMob] 전면 광고 로드 완료 (Interstitial Ad Loaded). Response: {ad.GetResponseInfo().GetResponseId()}");
                
                // [개선]: 이벤트는 로드 시점에 한 번만 등록하여 중복 구독 방지
                ad.OnAdFullScreenContentClosed += HandleInterstitialClosed;
                m_interstitialAd = ad;
            });
        }

        private void HandleInterstitialClosed()
        {
            UniTask.Post(() => 
            {
                var callback = m_onInterstitialClosed;
                m_onInterstitialClosed = null; // 실행 전 초기화하여 중복 실행 방지
                
                callback?.Invoke();
                LoadInterstitialAd(); // 다음 광고 미리 로드
            });
        }

        private async UniTaskVoid RetryLoadInterstitialAd()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(10));
            Debug.Log("[AdMob] 전면 광고 로드 재시도 중... (Retrying Interstitial Ad Load)");
            LoadInterstitialAd();
        }

        public void ShowInterstitialAd(Action onClosed = null)
        {
            if (IsInterstitialAdLoaded())
            {
                // [해결]: Time.timeScale이 0일 경우 AdMob UI(WebView)가 멈춰서 검은 화면만 뜨는 버그를 방지합니다.
                float previousTimeScale = Time.timeScale;
                if (previousTimeScale == 0f)
                {
                    Time.timeScale = 1f;
                }

                m_onInterstitialClosed = () => 
                {
                    // [변경]: 이전에는 강제로 0으로 되돌렸으나, ResurrectionWindow 등에서 
                    // 창을 닫으며 1로 이미 설정한 경우를 덮어씌워버리는(Freeze) 버그가 있어 제거합니다.
                    onClosed?.Invoke();
                };

                m_interstitialAd.Show();
            }
            else
            {
                Debug.LogWarning("[AdMob] 전면 광고가 준비되지 않았습니다. (Interstitial Ad Not Ready)");
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

            Debug.Log("[AdMob] 보상 광고 로딩 시작... (Loading Rewarded Ad)");
            m_isRewardedLoading = true;

            AdRequest request = new AdRequest();
            RewardedAd.Load(m_config.RewardedId, request, (ad, error) =>
            {
                m_isRewardedLoading = false;

                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdMob] 보상 광고 로드 실패 (Rewarded Ad Load Failed): {error.GetMessage()} [Code: {error.GetCode()}]");
                    
                    // [해결]: 로드 실패 시 일정 시간 후 재시도
                    RetryLoadRewardedAd().Forget();
                    return;
                }

                Debug.Log($"[AdMob] 보상 광고 로드 완료 (Rewarded Ad Loaded). Response: {ad.GetResponseInfo().GetResponseId()}");
                
                // [개선]: 이벤트는 로드 시점에 한 번만 등록
                ad.OnAdFullScreenContentClosed += HandleRewardedClosed;
                m_rewardedAd = ad;
            });
        }

        private void HandleRewardedClosed()
        {
            UniTask.Post(() => 
            {
                var callback = m_onRewardedClosed;
                m_onRewardedClosed = null;
                
                callback?.Invoke();
                LoadRewardedAd(); 
            });
        }

        private async UniTaskVoid RetryLoadRewardedAd()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(10));
            Debug.Log("[AdMob] 보상 광고 로드 재시도 중... (Retrying Rewarded Ad Load)");
            LoadRewardedAd();
        }

        public void ShowRewardedAd(Action onRewarded, Action onClosed = null)
        {
            if (IsRewardedAdLoaded())
            {
                // [해결]: Time.timeScale이 0일 경우 AdMob UI(WebView)가 멈춰서 검은 화면만 뜨는 버그를 방지합니다.
                float previousTimeScale = Time.timeScale;
                if (previousTimeScale == 0f)
                {
                    Time.timeScale = 1f;
                }

                m_onRewardedClosed = () => 
                {
                    // [변경]: 이전에는 강제로 0으로 되돌렸으나, ResurrectionWindow 등에서 
                    // 창을 닫으며 1로 이미 설정한 경우를 덮어씌워버리는(Freeze) 버그가 있어 제거합니다.
                    onClosed?.Invoke();
                };

                m_rewardedAd.Show((Reward reward) => 
                {
                    // [해결]: 보상 획득 콜백은 메인 스레드에서 실행되도록 보장합니다.
                    UniTask.Post(() => 
                    {
                        onRewarded?.Invoke();
                    });
                });
            }
            else
            {
                Debug.LogWarning("[AdMob] 보상 광고가 준비되지 않았습니다. (Rewarded Ad Not Ready)");
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
