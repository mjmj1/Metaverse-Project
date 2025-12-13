# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Meta Quest 3 VR/AR game where players use a hammer to hit moles and score points (Whac-A-Mole style).

## Technology Stack

- **Unity 6.0** (6000.1.12f1)
- **Meta XR SDK** v78.0.0 for Quest 3 optimization
- **XR Interaction Toolkit** v3.1.2 for VR controller interactions
- **OpenXR** for cross-platform VR support
- **URP** (Universal Render Pipeline) for performance optimization
- **DOTween** for smooth animations

## Architecture Overview

### System Architecture

```
┌─────────────────────────────────────────────────┐
│         GameManager (Singleton)                 │
│  - 게임 상태 관리 (Menu/Playing/Paused/GameOver) │
│  - 게임 루프 제어 (타이머, 점수)                   │
│  - 이벤트 시스템 (UnityAction)                    │
└─────────────────┬───────────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
┌───────▼────────┐  ┌──────▼────────┐
│  MoleManager   │  │  UIManager    │
│  - 오브젝트 풀  │  │  - UI 전환     │
│  - 반구형 배치  │  │  - 타이머/점수  │
│  - 난이도 조절  │  │  - 카운트다운   │
└───────┬────────┘  └───────────────┘
        │
   ┌────▼────┐
   │  Mole   │
   │ (상태머신) │
   └─────────┘
```

### Core Game Systems

**1. GameManager** (`Assets/Scripts/Game/GameManager.cs`)
- Singleton pattern으로 전역 접근
- State Machine: `Menu` → `Playing` → `Paused` / `GameOver`
- Game Loop: Coroutine 기반 (`GameSequenceRoutine`)
  - 초기화 → 카운트다운 → 게임 진행 → 종료
- 이벤트 시스템: `OnStateChanged`, `OnGameStart`, `OnGameOver` 등
- 점수 관리: `AddScore(int)`, `GetScore()`
- 타이머 관리: `gameDuration` (기본 30초)

**2. MoleManager** (`Assets/Scripts/Game/MoleManager.cs`)
- Object Pooling: `Queue<Mole>` (기본 20개)
- 반구형(Dome) 배치 시스템
  - Pitch: -20° ~ 40° (플레이어 시야 범위)
  - Yaw: 0° ~ 360° (전방위)
  - Radius: 2.0m (플레이어 중심)
- Progressive Difficulty
  - 초기 스폰 간격: 1.5초
  - 시간당 0.02초씩 감소 (최소 0.5-1.5초)
  - 그룹 스폰: 1-2마리 동시 출현
- 구멍 생성: 15개 랜덤 위치 (`holeCount`)

**3. Mole** (`Assets/Scripts/Game/Mole/Mole.cs`)
- State Machine: `None` → `Warning` → `Rising` → `Idle` → `Hiding` → `None`
  - 타격 시: → `Hit` → `None`
- 라이프사이클 (Coroutine)
  1. Warning: 전조 애니메이션 (흔들림)
  2. Rising: 상승
  3. Idle: 타격 대기
  4. Hiding: 하강 (자동 복귀)
- 충돌 감지: `OnTriggerEnter` → "Hammer" 태그 체크
- 콜백 패턴: `returnToPoolCallback` → 풀로 복귀 알림

**4. MoleAnimation** (`Assets/Scripts/Game/Mole/MoleAnimation.cs`)
- DOTween 기반 애니메이션
- `PlayWarning()`: DOShakePosition (흔들림)
- `PlayRise()`: DOLocalMoveY + Ease.OutBack (상승)
- `PlayHide()`: DOLocalMoveY + Ease.InBack (하강)
- `PlayHit()`: DOPunchScale + DOLocalMoveY (찌그러짐 + 빠른 하강)

**5. UIManager** (`Assets/Scripts/Game/UI/UIManager.cs`)
- 4개 캔버스 관리
  - Main Menu: 게임 시작, 설정 (시간/범위 슬라이더)
  - Game Info: 타이머, 점수, 카운트다운
  - Result: 최종 점수
  - Pause: 일시정지 메뉴
- GameManager 이벤트 구독/해제
- 상태 기반 UI 전환 (`SwitchUI`)

**6. ScoreManager** (`Assets/Scripts/Game/ScoreManager.cs`) ⚠️
- **현재 미사용** - GameManager가 점수 직접 관리
- 콤보 시스템 구현되어 있음
  - 1.5초 내 연속 타격 시 콤보
  - 콤보 보너스: (콤보 수 - 1) × 10점
- UnityEvent 기반 점수/콤보 변경 알림

### VR Interaction

**Hammer System**
- Prefab: `Assets/Prefabs/Game/Hammer.prefab`
- 게임 시작 시 동적 생성 (playerHead 기준)
  - 위치: forward × 0.5m + up × -0.3m
- Physics-based collision (Rigidbody + Collider)
- 태그: "Hammer"

**Input System**
- OVRInput 사용
- `OVRInput.Button.Start`: 일시정지 토글

### Project Structure

```
Assets/
├── Scripts/
│   ├── Game/
│   │   ├── GameManager.cs       # 게임 상태/루프 관리
│   │   ├── MoleManager.cs       # 두더지 스폰/풀링
│   │   ├── ScoreManager.cs      # 점수/콤보 (미사용)
│   │   ├── Mole/
│   │   │   ├── Mole.cs          # 두더지 상태머신
│   │   │   └── MoleAnimation.cs # DOTween 애니메이션
│   │   └── UI/
│   │       ├── UIManager.cs     # UI 전환/업데이트
│   │       └── HUDFollow.cs     # VR HUD 추적
│   ├── Metaverse/Interactions/  # 추가 VR 인터랙션
│   └── TutorialInfo/            # 튜토리얼 시스템
├── Prefabs/
│   └── Game/
│       ├── Hammer.prefab        # VR 망치
│       └── Cell.prefab          # 두더지 셀
├── Scenes/
│   ├── Metaverse.unity          # 메인 씬
│   └── Game.unity               # 게임 씬
├── XR/                          # XR 설정
├── MetaXR/                      # Meta Quest SDK
└── Rubber Play Hammer/          # 망치 에셋
```

## Data Flow

### Hit Detection Flow
```
사용자 망치 타격
    ↓
Mole.OnTriggerEnter (Collider with "Hammer" tag)
    ↓
Mole.Hit() → State = Hit
    ↓
GameManager.AddScore(1)
    ↓
UIManager.UpdateScore(currentScore)
    ↓
UI 업데이트 (scoreText)
```

### Game Loop Flow
```
GameManager.StartGame()
    ↓
GameState = Playing
    ↓
GameSequenceRoutine 시작
    ↓
1. MoleManager.Initialize() - 풀 초기화, 구멍 생성
2. 망치 Instantiate
3. 카운트다운 (3-2-1-START)
4. MoleManager.StartGameLoop() - 두더지 스폰 시작
5. 타이머 감소 루프
    ↓
타이머 = 0
    ↓
MoleManager.StopGameLoop()
    ↓
GameState = GameOver
    ↓
최종 점수 표시
```

## Development Workflow

### Building for Meta Quest 3
1. **Platform**: Switch to Android in Build Settings
2. **XR Setup**: Configure OpenXR with Meta OpenXR provider
3. **Build**: Create APK for device testing

### Key Development Settings
- **Input System**: Unity Input System (not legacy)
- **Scripting Backend**: IL2CPP for performance
- **Target Architecture**: ARM64
- **Graphics API**: Vulkan (recommended for Quest 3)

### Performance Considerations
- Maintain 90fps target for smooth VR experience
- Use object pooling to avoid garbage collection spikes
- Optimize draw calls with URP batching
- Test on actual Quest 3 device for validation

### Testing Without VR Headset

**Meta XR Simulator** (권장)
- Unity Editor에서 Play 버튼으로 바로 테스트
- 키보드 + 마우스 조작:
  - **마우스 이동**: 헤드셋 회전
  - **우클릭 + 드래그**: 컨트롤러 이동
  - **좌클릭**: 트리거 (타격)
  - **WASD**: 플레이어 이동
  - **Q/E**: 상하 이동

**XR Device Simulator**
- Window → Analysis → Input Debugger
- Scene 뷰와 Game 뷰 동시 확인

## Common Development Tasks

### Adding New Mole Types
1. Create new prefabs in `Assets/Prefabs/Game/` based on Cell.prefab
2. Update `Mole.cs` enum with new states if needed
3. Modify `MoleManager.cs` spawning logic
4. Configure collision layers and physics materials
5. Adjust animation timings in MoleAnimation.cs

### Modifying Game Difficulty
Edit difficulty progression in `MoleManager.cs`:
- `initialSpawnInterval` (Line 22): 초기 스폰 간격
- `groupCountRange` (Line 23): 동시 출현 두더지 수
- Line 156: 난이도 증가 속도 (`-0.02f`)
- `holeCount` (Line 16): 구멍 개수
- `radius` (Line 17): 플레이어로부터 거리

### Integrating ScoreManager (Currently Unused)
ScoreManager의 콤보 시스템을 활성화하려면:

1. GameManager.cs에 ScoreManager 참조 추가:
```csharp
[SerializeField] private ScoreManager scoreManager;
```

2. Mole.cs:150 수정:
```csharp
// 기존
GameManager.Instance.AddScore(1);

// 변경
GameManager.Instance.ScoreManager.AddMoleHit();
```

3. GameManager.AddScore() 메서드를 ScoreManager로 위임

### Testing VR Interactions
1. Use Unity's XR Simulator for basic testing (마우스 + 키보드)
2. Deploy to Quest 3 for proper controller testing
3. Verify hammer collision detection
4. Check Gizmos in Scene view for spawn positions

## Implementation Status

| Feature | Status | Notes |
|---------|--------|-------|
| 게임 상태 관리 | ✅ 완료 | Menu/Playing/Paused/GameOver |
| 두더지 스폰 시스템 | ✅ 완료 | 반구형 배치, 오브젝트 풀링 |
| VR 타격 감지 | ✅ 완료 | 물리 기반 충돌 |
| 점수 시스템 | ⚠️ 부분 완료 | 기본 점수만, **콤보 미구현** |
| 난이도 조절 | ✅ 완료 | 자동 증가 + 수동 설정 |
| UI 시스템 | ✅ 완료 | 4개 캔버스 (Menu/Game/Result/Pause) |
| 애니메이션 | ✅ 완료 | DOTween 기반 부드러운 전환 |
| 일시정지 | ✅ 완료 | Time.timeScale + OVRInput |
| 카운트다운 | ✅ 완료 | 3-2-1-START 연출 |
| 효과음/햅틱 | ❌ 미구현 | - |
| 튜토리얼 | ❌ 미구현 | - |
| 리더보드 | ❌ 미구현 | - |

## Known Issues & Improvement Points

### Critical
1. **ScoreManager 미사용**
   - 위치: `Assets/Scripts/Game/ScoreManager.cs`
   - 문제: 콤보 시스템 구현되어 있으나 통합 안 됨
   - 해결: Mole.cs:150에서 ScoreManager.AddMoleHit() 호출하도록 변경

2. **메모리 최적화**
   - 위치: `GameManager.cs:199, 253`
   - 문제: 매 게임마다 망치 Instantiate/Destroy
   - 해결: 망치도 오브젝트 풀링 적용 권장

### Minor
3. **하드코딩된 값들**
   - `GameManager.cs:27`: `gameDuration = 30f` → SerializeField로 변경
   - `MoleManager.cs:156`: `-0.02f` 난이도 증가량 → 설정 가능하게

4. **디버그 로그 정리**
   - `Mole.cs:81`: `print(other.gameObject.name)` → 릴리즈 빌드 전 제거
   - `GameManager.cs:127`: `print()` → `Debug.Log()` 권장

5. **GameManager 책임 과다**
   - 게임 루프, 점수, UI 업데이트 모두 담당
   - 제안: GameLoopController 분리

## Next Steps (우선순위)

1. **ScoreManager 통합** (High)
   - 콤보 시스템 활성화
   - 연속 타격 시 보너스 점수
   - 예상 작업: 30분

2. **효과음 추가** (Medium)
   - 타격 사운드
   - 배경음악
   - UI 클릭 사운드

3. **햅틱 피드백** (Medium)
   - 타격 시 진동
   - OVRInput.SetControllerVibration() 사용

4. **난이도 프리셋** (Low)
   - Easy/Normal/Hard 모드
   - 슬라이더 대신 버튼으로 선택

5. **리더보드** (Low)
   - PlayerPrefs로 로컬 저장
   - 최고 점수 표시

## Important Configuration

### Android SDK Setup
- **SDK Location**: `/Users/user/Library/Android/sdk`
- **NDK Version**: 27.1.12297006 (권장)
- **Required API Level**: Android 10.0 (API 29) 이상
- Unity Hub에서 Android Build Support 모듈 필요

### XR Settings (Edit > Project Settings > XR Plug-in Management)
- **OpenXR**: Enabled with Meta OpenXR provider
- **Interaction Toolkit**: Configured for Quest controllers
- **Stereo Rendering**: Enabled for VR
- **Meta XR Simulator**: Desktop 플랫폼에서도 활성화 (테스트용)

### Input Actions
- Hammer trigger mapped to primary button
- Grip interactions for holding hammer
- Physics-based movement for natural feeling
- **OVRInput.Button.Start**: Pause toggle

### Build Settings (Meta Quest 3)
- **Platform**: Android
- **Scripting Backend**: IL2CPP
- **Target Architecture**: ARM64
- **Minimum API Level**: Android 10.0 (API 29)
- **Target API Level**: Automatic (Highest installed)
- **Graphics API**: Vulkan

## Code Style Guidelines

- Follow C# naming conventions (PascalCase for methods, camelCase for variables)
- Use `[SerializeField]` for inspector-exposed private fields
- Implement `IDisposable` for objects using Unity resources
- Use Coroutines for timed operations instead of Update loops
- Add `[Header]` attributes for organized Inspector layout
- **DOTween**: 애니메이션은 MoleAnimation 클래스에서 관리
- **State Machine**: 명확한 상태 전환과 이벤트 발생
- **Event System**: UnityAction으로 느슨한 결합 유지

## Testing Strategy

- **Unit Tests**: Use Unity Test Framework for game logic
- **VR Testing**:
  - Development: Meta XR Simulator (마우스 + 키보드)
  - Final: Quest 3 실기기 테스트 필수
- **Performance Profiling**: Monitor frame rate (90fps 목표)
- **Collision Testing**: Scene 뷰에서 Gizmos로 시각화
- **Memory Profiling**: 오브젝트 풀 효율성 확인

## Reference Code Locations

### Key Methods
- Game Loop: `GameManager.cs:187` - `GameSequenceRoutine()`
- Mole Spawn: `MoleManager.cs:148` - `SpawnRoutine()`
- Hit Detection: `Mole.cs:79` - `OnTriggerEnter()`
- Score Update: `GameManager.cs:87` - `AddScore()`
- State Change: `GameManager.cs:125` - `OnGameStateChanged()`

### Configuration Points
- Game Duration: `GameManager.cs:27`
- Spawn Interval: `MoleManager.cs:22`
- Difficulty Curve: `MoleManager.cs:156`
- Pool Size: `MoleManager.cs:12`
- Dome Radius: `MoleManager.cs:17`