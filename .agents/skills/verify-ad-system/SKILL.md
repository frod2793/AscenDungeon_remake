---
name: verify-ad-system
description: 신규 AdMob 광고 서비스 아키텍처 및 SDK v10.7+ API 통합 상태를 검증합니다.
---

# 구글 애드몹 광고 시스템 검증

## 목적

1. **아키텍처 규격 준수** — `IAdService`, `AdMobService`, `AdsManager` 간의 의존성 주입 및 분리 구조 검증
2. **최신 SDK API 연동** — `InterstitialAd.Load`, `RewardedAd.Load`, `CanShowAd` 등 v10.7+ 정적 API 사용 확인
3. **설정 무결성** — `AdMobConfig` (ScriptableObject)를 통한 ID 관리 및 플랫폼별 예외 처리 검증
4. **호환성 및 호출부** — 기존 호출부(`DungeonClearUI` 등)가 `AdsManager.Service`를 올바르게 사용하는지 확인

## 실행 시점

- 광고 시스템 리팩토링 후
- 구글 애드몹 SDK 버전을 업데이트한 후
- 새로운 광고 단위(Ad Unit)를 추가하거나 설정을 변경했을 때

## Related Files

| 파일 경로 | 용도 |
|----------|------|
| `Assets/Scripts/Ad/IAdService.cs` | 광고 서비스 인터페이스 |
| `Assets/Scripts/Ad/AdMobService.cs` | SDK 연동 실질 구현체 |
| `Assets/Scripts/Ad/AdsManager.cs` | 서비스 생명주기 관리 매니저 |
| `Assets/Scripts/Ad/AdMobConfig.cs` | 광고 ID 설정 데이터 (SO) |
| `Assets/Scripts/Dungeon/DungeonClearUI.cs` | 던전 성공 UI (광고 호출부) |
| `Assets/Scripts/Dungeon/DungeonFaildUI.cs` | 던전 실패 UI (광고 호출부) |
| `Assets/Scripts/Dungeon/DungeonClearUI.cs` | 광고 호출 예시 UI |

## Workflow

### Step 1: SDK API 버전 검증
최신 SDK는 정적 메서드를 통한 로드 방식을 사용해야 합니다.

**검사:** `new InterstitialAd()` 같은 생성자 호출이 없고 `InterstitialAd.Load()`를 사용하는지 확인.

```bash
grep -n "new InterstitialAd" Assets/Scripts/Ad/AdMobService.cs
grep -n "InterstitialAd.Load" Assets/Scripts/Ad/AdMobService.cs
```

**FAIL:** 구형 생성자 호출 발견 시 최신 API로 수정 필요.

### Step 2: 서비스 싱글톤 접근점 검증
`Ads.Instance` 대신 `AdsManager.Service`를 사용해야 합니다.

**검사:** 프로젝트 전체에서 삭제된 `Ads.Instance` 참조가 남아있는지 확인.

```bash
grep -r "Ads.Instance" Assets/Scripts/
```

**FAIL:** 검색 결과가 있다면 `AdsManager.Service`로 교체 필요.

### Step 3: 설정 자산 구조 검증
광고 ID는 코드에 하드코딩되지 않고 `AdMobConfig`를 통해 전달되어야 합니다.

**검사:** `AdMobService`가 `AdMobConfig`를 매개변수로 받는지 확인.

```bash
grep -n "m_config" Assets/Scripts/Ad/AdMobService.cs
```

### Step 4: 로딩 연출 태그 검증
광고 로딩 중 UI 블로킹을 위해 `Loading` 태크 오브젝트를 찾는지 확인.

```bash
grep -n "GameObject.FindGameObjectWithTag(\"Loading\")" Assets/Scripts/Ad/AdsManager.cs
```

### Step 5: 중복 로딩 가드 플래그 검증
중복 로딩으로 인한 크래시나 에러 (`Prefab Ad is Null` 등) 방지를 위해 상태 플래그 검사를 하는지 확인합니다.

```bash
grep -n "m_isRewardedLoading" Assets/Scripts/Ad/AdMobService.cs
grep -n "m_isInterstitialLoading" Assets/Scripts/Ad/AdMobService.cs
```

## Output Format

| 검사 항목 | 상태 | 상세 결과 |
|-----------|------|-----------|
| SDK API 버전 | PASS/FAIL | |
| 서비스 접근점 | PASS/FAIL | |
| 설정 자산 연동 | PASS/FAIL | |
| 로딩 태그 가드 | PASS/FAIL | |

## Exceptions

1. **테스트 광고 ID**: 구글 공식 테스트 ID(`ca-app-pub-3940256099942544/...`) 사용은 개발 중 허용됨.
2. **에디터 조건부 컴파일**: `#if UNITY_EDITOR` 블록 내의 더미 로직은 에러로 처리하지 않음.
