using System;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Ad
{
    /// <summary>
    /// [설명]: 광고 서비스의 인터페이스입니다.
    /// 전면 광고 및 리워드 광고의 생명주기를 관리합니다.
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// [설명]: 광고 SDK 및 초기 광구들을 초기화합니다.
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// [설명]: 전면 광고를 로드합니다.
        /// </summary>
        void LoadInterstitialAd();

        /// <summary>
        /// [설명]: 전면 광고를 표시합니다.
        /// </summary>
        /// <param name="onClosed">광고가 닫혔을 때 호출될 콜백</param>
        void ShowInterstitialAd(Action onClosed = null);

        /// <summary>
        /// [설명]: 리워드 광고를 로드합니다.
        /// </summary>
        void LoadRewardedAd();

        /// <summary>
        /// [설명]: 리워드 광고를 표시합니다.
        /// </summary>
        /// <param name="onRewarded">보상을 획득했을 때 호출될 콜백</param>
        /// <param name="onClosed">광고가 닫혔을 때 호출될 콜백</param>
        void ShowRewardedAd(Action onRewarded, Action onClosed = null);

        /// <summary>
        /// [설명]: 전면 광고가 로드되었는지 확인합니다.
        /// </summary>
        bool IsInterstitialAdLoaded();

        /// <summary>
        /// [설명]: 리워드 광고가 로드되었는지 확인합니다.
        /// </summary>
        bool IsRewardedAdLoaded();
    }
}
