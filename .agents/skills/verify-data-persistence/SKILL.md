---
name: verify-data-persistence
description: 데이터 DTO, 서비스 의존성, 암호화 저장소 및 절대값 동기화 무결성 검증
---

# 데이터 영속성 및 서버 저장 무결성 검증

## 목적

1. **데이터 모델 일관성** — POCO 클래스 필드명이 뒤끝 서버 컬럼명과 일치하는지 검증
2. **저장 로직 무결성** — `UserDataService` 및 `BackEndServerManager`에서 누락된 필드 없이 저장되는지 확인
3. **제네릭 제약 조건** — `IBackEndService.GetGameDataAsync<T>` 호출 시 `where T : new()` 제약 준수 확인
4. **호환성 저장 메서드** — `Optionsaver`, `IAPSAVE` 등 레거시 호출부에 대한 대응 로직 검증

## 실행 시점

- 새로운 서버 테이블을 추가했을 때
- 플레이어 자산(Gold, Item 등) 모델을 수정했을 때
- 게임 종료 또는 옵션 변경 시 저장 로직을 테스트할 때

## Related Files

| 파일 경로 | 용도 |
|----------|------|
| `Assets/Scripts/BackEnd/UserDataService.cs` | 신규 데이터 저장/로드 서비스 |
| `Assets/Scripts/Login/BackEndServerManager.cs` | 서버 통신 및 레거시 저장 매니저 |
| `Assets/Scripts/BackEnd/BackEndService.cs` | 뒤끝 API 원자적 처리 서비스 |
| `Assets/Scripts/Singletons/GameLoger.cs` | 클라이언트 상태 데이터 저장소 |

## Workflow

### Step 1: 서버 테이블명 및 컬럼 대조
뒤끝 콘솔에 정의된 테이블명과 코드 내 문자열이 일치해야 합니다.

**검사:** "Player", "Item", "Stage", "Option", "IAP" 테이블명 사용 확인 (대소문자 주의).

```bash
grep -E "\"Player\"|\"Item\"|\"Stage\"|\"Option\"|\"IAP\"" Assets/Scripts/BackEnd/UserDataService.cs
```

### Step 2: 제네릭 데이터 변환 검증
`JsonData`를 객체로 변환할 때 `new()` 제약 조건이 필수입니다.

```bash
grep -n "where T : class, new()" Assets/Scripts/BackEnd/IBackEndService.cs
grep -n "where T : class, new()" Assets/Scripts/BackEnd/BackEndService.cs
```

### Step 3: 옵션 저장 필드 데이터 검증
`Optionsaver`가 `GameLoger`의 모든 필수 필드를 누락 없이 전송하는지 확인합니다.

```bash
grep -A 10 "public void Optionsaver()" Assets/Scripts/Login/BackEndServerManager.cs
```

### Step 4: IAP 데이터 형식 검증
불리언 값을 문자열로 저장하거나 파싱할 때의 형식 일관성을 확인합니다.

```bash
grep -n "bool.Parse" Assets/Scripts/BackEnd/UserDataService.cs
```

### Step 6: JsonData 안전 처리 검증
`JsonData` 미초기화 상태에서 `Rows()` 호출 시 발생하는 예외를 방지하기 위해 `SafeCount`와 같은 래핑 메서드를 사용하는지 확인합니다.

```bash
grep -n "SafeCount" Assets/Scripts/BackEnd/BackEndService.cs
```

### Step 7: 싱글톤 Null 가드 검증 (NRE 방지)
비동기 로드나 앱 종료(`OnApplicationQuit`) 시 싱글톤 인스턴스 파괴로 인한 NRE를 방지하기 위한 null 체크가 포함되어 있는지 확인합니다.

```bash
# UserDataService 내 싱글톤 null 체크 확인
grep -nE "GameLoger.Instance != null|IAP.Instance != null" Assets/Scripts/BackEnd/UserDataService.cs

# BackEndServerManager 내 싱글톤 null 체크 및 try-catch 확인
grep -nE "GameLoger.Instance != null|IAP.Instance != null|MoneyManager.Instance != null" Assets/Scripts/Login/BackEndServerManager.cs
```

### Step 8: 테이블명 오타 및 대소문자 무결성
뒤끝 서버는 테이블명의 대소문자를 구분합니다. `"ITem"`, `"STAGE"`와 같은 오타가 없는지 전수 조사합니다.

```bash
# ITem, STAGE 등 잘못된 대소문자가 검출되는지 확인
grep -E "\"ITem\"|\"STAGE\"" Assets/Scripts/Login/BackEndServerManager.cs
grep -E "\"ITem\"|\"STAGE\"" Assets/Scripts/BackEnd/UserDataService.cs
```

## Output Format

| 검사 항목 | 상태 | 상세 결과 |
|-----------|------|-----------|
| 테이블명 무결성 | PASS/FAIL | |
| 제네릭 제약 조건 | PASS/FAIL | |
| 저장 필드 누락 여부 | PASS/FAIL | |
| IAP 데이터 직렬화 | PASS/FAIL | |
| 자동 초기화 및 가드 | PASS/FAIL | |
| JsonData 안전 처리 | PASS/FAIL | |
| 싱글톤 Null 가드 | PASS/FAIL | |

## Exceptions

1. **로컬 폴백**: 서버 연결 실패 시 `PlayerPrefs` 등을 통한 임시 저장은 허용되나 경고 표시.
2. **읽기 전용 테이블**: 로그 등 쓰기 권한이 없는 테이블에 대한 저장 시도 금지.
