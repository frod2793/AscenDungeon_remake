---
name: verify-login-flow
description: 타이틀 씬의 로그인 흐름(MVVM), 어드레서블 로딩 및 리모트 동기화 검증
---

# 로그인 흐름 및 MVVM 무결성 검증

## 목적

1. **MVVM 패턴 준수** — `LoginView`와 `LoginViewModel` 간의 데이터 바인딩 및 명령 전달 구조 검증
2. **UniTask 비동기 안전성** — `Forget()`, `Forget()` 호출 및 예외 처리 구문(`try-catch`) 확인
3. **토큰 및 페더레이션** — 뒤끝 SDK 5.11.1 API를 사용한 토큰 로그인 및 구글 연동 로직 검증
4. **런타임 바인딩** — `BindButtonEvents`를 통한 UI 이벤트의 중복 방지 및 안전한 연결 확인

## 실행 시점

- 로그인 관련 로직(BackEndService, LoginViewModel 등) 수정 후
- 로그인 화면의 UI 버튼이나 필드를 변경했을 때
- 뒤끝 SDK 버전을 업데이트한 후

## Related Files

| 파일 경로 | 용도 |
|----------|------|
| `Assets/Scripts/Login/LoginView.cs` | 로그인 화면 View |
| `Assets/Scripts/ViewModel/LoginViewModel.cs` | 로그인 비즈니스 로직 ViewModel |
| `Assets/Scripts/BackEnd/BackEndService.cs` | 뒤끝 API 래퍼 서비스 |
| `Assets/Scripts/BackEnd/IBackEndService.cs` | 백엔드 서비스 인터페이스 |

## Workflow

### Step 1: UniTask 비동기 호출 검증
비동기 메서드 호출 시 적절한 에러 핸들링과 `Forget()` 사용을 확인합니다.

**검사:** `TryTokenLogin`, `TryGuestLogin`, `LoadSceneAsync` 호출이 비동기 안전하게 처리되는지 확인.

```bash
# LoginView에서 Forget() 호출 확인
grep -n "Forget()" Assets/Scripts/Login/LoginView.cs

# SceneNavigationService 호출이 await 되는지 확인
grep -n "await m_navigationService.LoadSceneAsync" Assets/Scripts/ViewModel/LoginViewModel.cs
```

### Step 2: 로그인 실패 이벤트 콜백 검증
토큰 로그인 실패 시 UI 활성화를 위한 콜백이 바인딩되어 있는지 확인합니다.

```bash
grep -n "OnTokenLoginFailed" Assets/Scripts/ViewModel/LoginViewModel.cs
grep -n "OnTokenLoginFailed" Assets/Scripts/Login/LoginView.cs
```

### Step 3: 버튼 런타임 바인딩 검증
인스펙터 의존성을 줄이기 위한 `BindButtonEvents` 로직이 중복 방지 코드를 포함하는지 확인합니다.

```bash
grep -n "GetPersistentEventCount() == 0" Assets/Scripts/Login/LoginView.cs
```

### Step 4: 뒤끝 API 오버로드 검증
v5.11.1 기준 `AuthorizeFederation` 등의 호출 방식이 올바른지 확인합니다.

```bash
grep -n "AuthorizeFederation" Assets/Scripts/BackEnd/BackEndService.cs
```

### Step 5: 내비게이션 서비스 주입 검증
ViewModel에서 인터페이스 기반 내비게이션 서비스를 주입받아 사용하는지 확인합니다.

```bash
# 생성자 주입 확인
grep -n "ISceneNavigationService" Assets/Scripts/ViewModel/LoginViewModel.cs
```

## Output Format

| 검사 항목 | 상태 | 상세 결과 |
|-----------|------|-----------|
| 비동기 처리 (UniTask) | PASS/FAIL | |
| MVVM 이벤트 바인딩 | PASS/FAIL | |
| 런타임 버튼 이벤트 | PASS/FAIL | |
| SDK API 오버로드 | PASS/FAIL | |
| 내비게이션 주입 | PASS/FAIL | |

## Exceptions

1. **에뮬레이터/테스트 모드**: 테스트를 위한 하드코딩된 더미 토큰 로그인은 `#if UNITY_EDITOR` 내에서 허용.
2. **로그인 생략**: 개발 편의를 위한 자동 로그인 강제 성공 로직은 주석 처리 또는 조건부 컴파일 필요.
