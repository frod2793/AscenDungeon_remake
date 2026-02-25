---
name: verify-ingame-core
description: 인게임 핵심 시스템, 초기화 순서 및 씬 내비게이션 비동기 로직 검증
---

# 인게임 핵심 시스템 및 내비게이션 검증

## 목적

1. **씬 전환 안정성** — `SceneNavigationService`가 `NextFrame()` 및 `ToUniTask` 진행률 로그를 사용하여 안전하게 동작하는지 확인
2. **초기화 순서** — 인게임 진입 시 필요한 매니저들의 초기화 및 데이터 주입 상태 검증
3. **UI 바운더리** — 조이스틱 등 입력 인터페이스의 화면 경계 및 레이아웃 설정 검증

## 실행 시점

- 씬 전환 로직(`SceneNavigationService`)을 수정했을 때
- 인게임 매니저(GameManager, InGameViewModel 등)를 새로 추가하거나 초기화 루틴을 변경했을 때
- 조이스틱이나 HUD 레이아웃을 수정했을 때

## Related Files

| 파일 경로 | 용도 |
|----------|------|
| `Assets/Scripts/Modules/SceneNavigationService.cs` | 실시간 씬 전환 서비스 |
| `Assets/Scripts/Modules/ISceneNavigationService.cs` | 씬 전환 인터페이스 |

## Workflow

### Step 1: 씬 로딩 비동기 패턴 검증
씬 로딩 시 레이스 컨디션을 방지하기 위한 대기 및 로그 로직을 확인합니다.

```bash
# 1프레임 대기 로직 확인
grep -n "await UniTask.NextFrame()" Assets/Scripts/Modules/SceneNavigationService.cs

# 진행률(Progress) 로그 확인
grep -n "System.Progress.Create" Assets/Scripts/Modules/SceneNavigationService.cs
```

### Step 2: 씬별 전처리 로직 검증
특정 씬 진입 시 수행되는 전처리(예: 인벤토리 초기화)가 올바른 조건에서 실행되는지 확인합니다.

```bash
# Town 씬 진입 시 전처리 확인
grep -A 5 "SceneIndex.Town" Assets/Scripts/Modules/SceneNavigationService.cs
```

## Output Format

| 검사 항목 | 상태 | 상세 결과 |
|-----------|------|-----------|
| 비동기 안전성 (NextFrame) | PASS/FAIL | |
| 진행률 로깅 | PASS/FAIL | |
| 전처리 로직 일치 | PASS/FAIL | |

## Exceptions

1. **에디터 테스트**: 특정 시작 씬 고정을 위한 로직은 `UNITY_EDITOR` 내에서 허용.
2. **빠른 전환**: 로딩 UI가 없는 즉시 전환(`useLoadingUI = false`) 케이스 허용.
