---
name: verify-dungeon-system
description: 던전 선택, 성공/실패 UI 흐름 및 상태 관리 무결성 검증.
---

# 던전 시스템 무결성 검증

## 목적

1. **내비게이션 흐름** — 던전 성공/실패 시 `Town` 씬으로의 안전한 복귀 확인
2. **UI 상태 관리** — 던전 선택 UI에서 데이터 로드 및 표시 무결성 검증
3. **광고 연동** — `DungeonClearUI`, `DungeonFaildUI`에서 `AdsManager.Service` 호출 정합성 확인
4. **초기화 로직** — 던전 진입/퇴장 시 필요한 데이터(인벤토리 등) 초기화 여부 확인

## 실행 시점

- 던전 UI(`DungeonClearUI`, `DungeonFaildUI`, `DungeonSelectUI`) 수정 시
- 던전 진입 또는 결과 처리 로직 변경 시

## Related Files

| 파일 경로 | 용도 |
|----------|------|
| `Assets/Scripts/Dungeon/DungeonSelectUI.cs` | 던전 선택 화면 UI |
| `Assets/Scripts/Dungeon/DungeonSelection.cs` | 던전 선택 로직 |
| `Assets/Scripts/Dungeon/DungeonClearUI.cs` | 던전 클리어 결과 UI |
| `Assets/Scripts/Dungeon/DungeonFaildUI.cs` | 던전 실패 결과 UI |
| `Assets/Scripts/Modules/SceneIndex.cs` | 씬 인덱스 정의 |

## Workflow

### Step 1: 타운 복귀 씬 인덱스 검증
던전 결과 UI에서 타운(Town) 씬으로 복귀할 때 하드코딩된 숫자 대신 `SceneIndex.Town`을 사용하는지 확인합니다.

```bash
grep -n "SceneIndex.Town" Assets/Scripts/Dungeon/DungeonClearUI.cs
grep -n "SceneIndex.Town" Assets/Scripts/Dungeon/DungeonFaildUI.cs
```

### Step 2: 광고 서비스 호출부 검증
결과 화면에서 광고 호출 시 `AdsManager.Service`를 사용하는지 확인합니다.

```bash
grep -n "AdsManager.Service" Assets/Scripts/Dungeon/DungeonClearUI.cs
grep -n "AdsManager.Service" Assets/Scripts/Dungeon/DungeonFaildUI.cs
```

### Step 3: 싱글톤 Null 가드 및 DI 확인
UI 클래스 내에서 싱글톤 접근 시 `!= null` 체크가 명시적으로 되어 있는지 확인합니다.

```bash
grep -nE "Instance != null|if (.*Instance)" Assets/Scripts/Dungeon/*.cs
```

## Output Format

| 검사 항목 | 상태 | 상세 결과 |
|-----------|------|-----------|
| Town 복귀 인덱스 | PASS/FAIL | |
| 광고 서비스 연동 | PASS/FAIL | |
| 싱글톤 가드 | PASS/FAIL | |

## Exceptions

1. **테스트 씬**: 개발용 테스트 씬 직접 로드는 `#if UNITY_EDITOR` 블록 내에서 허용.
2. **즉시 재시작**: 광고 없이 즉시 재시작 기능은 기획 의도에 따라 허용.
