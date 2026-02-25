using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// [설명]: 빌드 시작 전 자동으로 버전을 갱신하는 에디터 스크립트입니다.
/// IPreprocessBuildWithReport 인터페이스를 사용하여 빌드 프로세스에 개입합니다.
/// </summary>
public class BuildVersionProcessor : IPreprocessBuildWithReport
{
    #region 프로퍼티
    /// <summary>
    /// [설명]: 빌드 사전 처리 순서를 지정합니다. 낮은 숫자일수록 먼저 실행됩니다.
    /// </summary>
    public int callbackOrder => 0;
    #endregion

    #region 빌드 이벤트 핸들러
    /// <summary>
    /// [설명]: 빌드가 시작될 때 호출되어 버전을 자동으로 갱신합니다.
    /// </summary>
    /// <param name="report">빌드 리포트 데이터</param>
    public void OnPreprocessBuild(BuildReport report)
    {
        UpdateBuildVersion();
    }
    #endregion

    #region 내부 로직
    /// <summary>
    /// [설명]: PlayerSettings의 버전 정보를 갱신합니다.
    /// Bundle Version과 Android Bundle Version Code를 업데이트합니다.
    /// </summary>
    private void UpdateBuildVersion()
    {
        // 1. Android Bundle Version Code 갱신
        int previousCode = PlayerSettings.Android.bundleVersionCode;
        PlayerSettings.Android.bundleVersionCode = previousCode + 1;

        // 2. Bundle Version (String) 갱신 (예: 0.1 -> 0.2)
        string previousVersion = PlayerSettings.bundleVersion;
        string newVersion = IncrementVersionString(previousVersion);
        PlayerSettings.bundleVersion = newVersion;

        Debug.Log($"[BuildProcessor] 빌드 버전 자동 갱신 완료: " +
                  $"Version {previousVersion} -> {newVersion}, " +
                  $"Version Code {previousCode} -> {PlayerSettings.Android.bundleVersionCode}");
    }

    /// <summary>
    /// [설명]: 버전 문자열의 마지막 숫자를 파싱하여 1 증가시킵니다.
    /// </summary>
    /// <param name="version">원본 버전 문자열 (예: 1.0.1)</param>
    /// <returns>갱신된 버전 문자열 (예: 1.0.2)</returns>
    private string IncrementVersionString(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.1";

        string[] parts = version.Split('.');
        if (parts.Length == 0) return "1";

        // 마지막 부분을 숫자로 변환 시도
        if (int.TryParse(parts[parts.Length - 1], out int lastNumber))
        {
            parts[parts.Length - 1] = (lastNumber + 1).ToString();
            return string.Join(".", parts);
        }

        // 숫자가 아닐 경우 뒤에 .1을 붙임
        return version + ".1";
    }
    #endregion
}
