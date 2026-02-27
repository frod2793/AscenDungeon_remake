---
name: verify-architecture-standards
description: 프로젝트 핵심 아키텍처 규칙(No Singleton, MVVM 준수, GPGS 업적 흐름) 검증.
---

# 프로젝트 아키텍처 표준 검증

## 목적

1. **싱글톤 지양 (Decoupling)** — 신규 클래스에서 `Singleton<T>` 상속 지양 및 의존성 주입(DI) 권장
2. **MVVM 패턴 준수** — UI(`View`)에서 비즈니스 로직 직접 수행 금지, `ViewModel` 위임 확인
3. **GPGS 업적 흐름 준수** — 로그인 완료 및 게임 진입 시점에 환영 업적이 명시적으로 해제되는지 검사
4. **Unity Safety** — `UnityEngine.Object` 타입에 대한 `?.` 연산자 사용 금지 (명시적 null 체크 강제)

## 실행 시점

- 새로운 시스템 아키텍처 도입 시
- 대규모 리팩토링 후 규칙 준수 여부 확인 시

## Related Files

| 파일 경로 | 용도 |
|----------|------|
| `Assets/GPGSIds.cs` | 업적 ID 상수 정의 (Global) |
| `Assets/Scripts/ViewModel/LoginViewModel.cs` | 닉네임 설정 및 게임 진입 로직 |
| `Assets/Scripts/Modules/SceneNavigationService.cs` | 씬 전환 서비스 |
| `Assets/Scripts/Login/LoginView.cs` | DI 주입 지점 |

## Workflow

### Step 1: GPGS 업적 해제 흐름 및 API 검증
- **흐름**: 로그인 완료(`ProceedToGame`) 및 닉네임 생성 성공 시 환영 업적(`GPGSIds.achievement____`)을 해제하는지 확인합니다.
- **API**: `Social.ReportProgress` 대신 `PlayGamesPlatform.Instance.UnlockAchievement`를 사용하는지 확인합니다.

```bash
# 1. GPGS v2 전용 UnlockAchievement 호출 확인
grep -nE "UnlockAchievement.*GPGSIds\.achievement____" Assets/Scripts/ViewModel/LoginViewModel.cs

# 2. 레거시 Social.ReportProgress 사용 여부 (금지)
grep -r "Social.ReportProgress" Assets/Scripts/BackEnd/
```

### Step 2: Unity Safety (Null Check) 검증
`MonoBehaviour`나 `GameObject` 등 유니티 객체에 `?.`를 사용하지 않는지 확인합니다.

```bash
# .cs 파일에서 ?. 사용처 탐색 (위험 패턴)
grep -rE "\w+\?\.(transform|gameObject|SetActive|GetComponent)" Assets/Scripts/
```

### Step 3: MVVM 규칙 검증
LoginView 같은 View 클래스에서 `BackEnd.Backend`에 직접 접근하는지 확인합니다. (ViewModel을 거쳐야 함)

```bash
grep -n "BackEnd.Backend" Assets/Scripts/Login/LoginView.cs
```

## Output Format

| 검사 항목 | 상태 | 상세 결과 |
|-----------|------|-----------|
| GPGS 업적 흐름 | PASS/FAIL | |
| 유니티 Null 가드 | PASS/FAIL | |
| MVVM 레이어 분리 | PASS/FAIL | |

## Exceptions

1. **레거시 코드**: 기존 `Singleton<T>`을 이미 광범위하게 사용하는 클래스는 예외로 두되, 신규 추가는 금지.
2. **에디터 도구**: `Assets/Scripts/Editor/` 내의 툴들은 유니티 객체 안전 규칙에서 제외 가능.
