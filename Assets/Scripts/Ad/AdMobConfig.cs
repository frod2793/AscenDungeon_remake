using UnityEngine;

namespace Assets.Scripts.Ad
{
    /// <summary>
    /// [설명]: AdMob 광고 ID 설정을 관리하는 데이터 클래스입니다.
    /// ScriptableObject를 사용하여 에디터에서 설정 가능하도록 합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "AdMobConfig", menuName = "Config/AdMobConfig")]
    public class AdMobConfig : ScriptableObject
    {
        #region 에디터 설정
        [Header("모드 설정")]
        [SerializeField, Tooltip("테스트 모드 사용 여부")]
        private bool m_useTestMode = true;

        [Header("Android 광고 ID")]
        [SerializeField, Tooltip("안드로이드 전면 광고 ID (Live)")]
        private string m_androidInterstitialId = "ca-app-pub-5708876822263347/3093570612";
        [SerializeField, Tooltip("안드로이드 리워드 광고 ID (Live)")]
        private string m_androidRewardedId = "ca-app-pub-5708876822263347/6491913665";

        [Header("Test 광고 ID (Google Default)")]
        [SerializeField] private string m_testInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string m_testRewardedId = "ca-app-pub-3940256099942544/5224354917";
        #endregion

        #region 프로퍼티
        /// <summary>
        /// [설명]: 현재 설정에 따른 전면 광고 ID를 반환합니다.
        /// </summary>
        public string InterstitialId => m_useTestMode ? m_testInterstitialId : m_androidInterstitialId;

        /// <summary>
        /// [설명]: 현재 설정에 따른 리워드 광고 ID를 반환합니다.
        /// </summary>
        public string RewardedId => m_useTestMode ? m_testRewardedId : m_androidRewardedId;

        /// <summary>
        /// [설명]: 테스트 모드 여부를 반환합니다.
        /// </summary>
        public bool UseTestMode => m_useTestMode;
        #endregion
    }
}
