# 3D RPG Game Portfolio

Unity 기반 3D RPG 게임 포트폴리오 프로젝트

> **개발 기간**: 2024
> **엔진**: Unity 2022.3 LTS
> **언어**: C#

---

## 목차

1. [프로젝트 개요](#1-프로젝트-개요)
2. [구현 시스템](#2-구현-시스템)
   - [State Machine 기반 캐릭터 시스템](#21-state-machine-기반-캐릭터-시스템)
   - [Fuzzy Logic 적 AI](#22-fuzzy-logic-적-ai)
   - [퀘스트 시스템](#23-퀘스트-시스템)
   - [인벤토리 & 드래그앤드롭](#24-인벤토리--드래그앤드롭)
   - [스킬 트리 시스템](#25-스킬-트리-시스템)
   - [상점 시스템](#26-상점-시스템)
   - [오브젝트 풀링](#27-오브젝트-풀링)
   - [Save / Load 시스템](#28-save--load-시스템)
   - [데미지 팝업 시스템](#29-데미지-팝업-시스템)
   - [미니맵 시스템](#210-미니맵-시스템)
   - [카메라 락온 시스템](#211-카메라-락온-시스템)
3. [사용된 디자인 패턴](#3-사용된-디자인-패턴)
4. [성능 최적화](#4-성능-최적화)
5. [프로젝트 구조](#5-프로젝트-구조)
6. [버그 수정 기록](#6-버그-수정-기록)

---

## 1. 프로젝트 개요

### 핵심 구현 목표

- 유지보수 가능한 **아키텍처** 설계 (State Pattern, 컴포넌트 분리)
- **GC 부담 최소화**를 위한 오브젝트 풀링 적용
- **데이터 주도 설계** (ScriptableObject, JSON) 로 기획 변경에 유연하게 대응
- 실무에서 사용되는 **디자인 패턴** 직접 구현 및 적용

### 주요 시스템 요약

| 시스템 | 핵심 기술 | 관련 파일 |
|--------|-----------|-----------|
| 캐릭터 제어 | State Machine | `StateMachine.cs`, `Player/*State.cs` |
| 적 AI | FSM + Fuzzy Logic | `EnemyAI.cs`, `BossAI.cs`, `FuzzyAttack.cs` |
| 퀘스트 | 다중 목표, 선행 조건 | `QuestManager.cs`, `QuestGiver.cs` |
| 인벤토리 | 드래그앤드롭, 스태킹 | `InventorySystem.cs`, `DragDrop.cs` |
| 스킬 트리 | 계층 잠금 해제, JSON | `SkillTreeSystem.cs`, `SkillDatabase.cs` |
| 상점 | 구매/판매 인터페이스 | `ShopKepper.cs`, `GoldManager.cs` |
| 오브젝트 풀링 | Queue 기반 재사용 | `ObjectPoolManager.cs` |
| Save/Load | JSON 직렬화 | `SaveManager.cs`, `SaveData.cs` |
| 데미지 팝업 | 풀링 + 애니메이션 | `DamagePopupManager.cs`, `DamagePopup.cs` |
| 미니맵 | Orthographic Camera, 마커 | `MinimapManager.cs` |
| 카메라 | 락온, 3인칭 | `CameraMovement.cs` |

---

## 2. 구현 시스템

---

### 2.1 State Machine 기반 캐릭터 시스템

#### 설계 배경

`if/else` 분기로 상태를 관리하면 상태가 늘어날수록 코드가 복잡해지고 버그가 생기기 쉽습니다.
State Pattern을 적용해 각 상태를 독립 클래스로 분리하여 **단일 책임 원칙(SRP)** 을 지켰습니다.

#### 구조

```
StateMachine
├── Enter()   : 상태 진입 시 1회 실행
├── Execute() : 매 프레임 실행 (Update)
└── Exit()    : 상태 종료 시 1회 실행

플레이어 상태 목록
├── PlayerIdleState     - 대기
├── PlayerWalkState     - 걷기
├── PlayerRunState      - 달리기
├── PlayerRollState     - 구르기 (무적 판정)
├── PlayerAttack1State  - 1타
├── PlayerAttack2State  - 2타
└── PlayerAttack3State  - 3타 (피니셔)
```

#### 핵심 코드

```csharp
// StateMachine.cs
public class StateMachine
{
    private State _currentState;

    public void ChangeState(State next)
    {
        if (_currentState == next) return;  // 동일 상태 전환 방지
        _currentState?.Exit();
        _currentState = next;
        _currentState.Enter();
    }

    public void Update() => _currentState?.Execute();
}
```

#### 콤보 공격 구현

공격 상태 내에 **입력 수용 구간(Input Buffer Window)** 을 두어 프레임 타이밍 기반 콤보를 구현했습니다.

```csharp
// PlayerAttack1State.cs - 콤보 입력 수용 구간
private void CheckComboInput()
{
    if (!_comboInputWindow) return;          // 수용 구간이 아니면 무시
    if (Input.GetMouseButtonDown(0))
        _player.StateMachine.ChangeState(_player.Attack2State);
}
```

---

### 2.2 Fuzzy Logic 적 AI

#### 설계 배경

일반 적은 FSM(유한 상태 기계)으로 구현하고, **보스 AI**에는 퍼지 로직을 적용했습니다.
단순한 조건문(HP < 30%이면 강공격)보다 **연속적이고 자연스러운 의사결정**이 가능합니다.

#### 일반 적 AI - FSM

```
EnemyAI 상태 전이
Idle → (플레이어 FOV 감지) → Chase → (공격 사거리) → Attack
              ↓ (추적 실패)
           Return → Patrol → Idle
```

- FOV(시야각) + 거리 조건으로 플레이어 감지
- NavMesh Agent로 경로 탐색
- 웨이포인트 순환 패트롤

#### 보스 AI - Fuzzy Logic

| 입력 변수 | 설명 |
|-----------|------|
| HP 비율 | 보스/플레이어 각각의 현재 HP |
| 거리 | 플레이어와의 현재 거리 |

```csharp
// FuzzyAttack.cs - 퍼지 멤버십 함수
float heavyScore = 0f;
float basicScore = 0f;

// 보스 HP 낮을수록 강공격 점수 상승
heavyScore += FuzzyLow(bossHpRatio);

// 플레이어 HP 높을수록 강공격 점수 상승
heavyScore += FuzzyHigh(playerHpRatio);

// 두 점수를 비교해 공격 방식 결정
return heavyScore >= basicScore ? AttackType.Heavy : AttackType.Basic;
```

---

### 2.3 퀘스트 시스템

#### 기능

- **다중 목표 타입**: Kill(처치), RequiredItem(아이템 제출)
- **선행 퀘스트 조건**: `prerequisiteQuestId`로 특정 퀘스트 완료 후 해금
- **퀘스트 순서 관리**: NPC별로 퀘스트 목록을 순서대로 제공
- **상태 기계**: Available → InProgress → Completed

#### 구조

```
QuestData (ScriptableObject)
└── questId, title, description
└── objectives[] (목표 목록)
    ├── type: Kill / RequiredItem
    ├── targetId, requiredCount
    └── currentCount

QuestManager (Singleton)
├── StartQuest()        - 퀘스트 시작
├── UpdateKillCount()   - 처치 수 갱신
├── SubmitRequiredItem()- 아이템 제출
├── CanStartQuest()     - 선행 조건 확인
└── GetCurrentQuest()   - 현재 진행 가능 퀘스트 반환

QuestGiver (NPC)
└── E키 상호작용 → QuestManager에 위임
```

#### 데이터 흐름

```
플레이어 E키 → QuestGiver.Interact()
    → QuestManager.GetCurrentQuest()
    → QuestStatus 확인 (Available / InProgress / Completed)
    → DialogueUI.Show() 로 결과 표시

몬스터 처치 → EnemyAI.OnDie()
    → QuestManager.UpdateKillCount(monsterId)
    → 퀘스트 목표 달성 여부 체크
```

---

### 2.4 인벤토리 & 드래그앤드롭

#### 기능

- 아이템 획득/스태킹/소비
- 드래그앤드롭으로 퀵슬롯에 스킬/아이템 등록
- 우클릭 컨텍스트 메뉴 (장착, 버리기, 사용)
- PickableItem - 씬에서 아이템 습득

#### 드래그앤드롭 구조

Unity EventSystem 인터페이스를 구현해 드래그를 처리합니다.

```csharp
// IBeginDragHandler - 드래그 시작
// IDragHandler     - 드래그 중 (위치 갱신)
// IEndDragHandler  - 드래그 종료 (드롭 처리)
// IDropHandler     - 드롭 대상에서 수신

// 드래그 아이콘 위치 계산 (Overlay / Camera 모드 모두 대응)
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    canvas.transform as RectTransform,
    eventData.position,
    eventData.pressEventCamera,
    out Vector2 localPos);
```

`CanvasGroup.blocksRaycasts = false` 설정으로 드래그 아이콘이 드롭 슬롯의 Raycast를 막지 않도록 처리했습니다.

#### 퀵슬롯 연동

```
스킬 아이콘 드래그 → CurrentDragSkill 컴포넌트에 skillId 태그
    → 퀵슬롯 드롭 → QuickSlot.OnDrop()
    → skillId 저장 → 단축키(1~4) 로 PlayerSkillCaster.TryUseSkill() 호출
```

---

### 2.5 스킬 트리 시스템

#### 기능

- JSON 파일 기반 스킬 데이터 정의
- 계층적 선행 스킬 조건 (parentId)
- 레벨 / 스킬 포인트 / 선행 스킬 3단계 해금 조건
- 스킬 랭크 시스템 (maxRank까지 반복 습득)
- 드래그앤드롭으로 퀵슬롯에 스킬 등록
- 마우스 호버 툴팁

#### 데이터 구조

```json
// Resources/SkillTree/skills.json
{
  "skills": [
    {
      "id": "energy_ball",
      "name": "Energy Ball",
      "parentId": "",
      "unlockLevel": 1,
      "cost": 1,
      "maxRank": 1,
      "cooldown": 3.0,
      "damage": 30,
      "description": "Launches a magic projectile."
    },
    {
      "id": "advanced_energy_ball",
      "name": "Advanced Energy Ball",
      "parentId": "energy_ball",
      "unlockLevel": 5,
      "cost": 2,
      "maxRank": 1,
      "cooldown": 2.0,
      "damage": 60
    }
  ]
}
```

#### 습득 조건 체크 로직

```csharp
// SkillTreeSystem.cs
public bool CanLearn(string id)
{
    // 1. 레벨 조건
    if (progress.Level < skill.unlockLevel) return false;

    // 2. 스킬 포인트 조건
    if (progress.SkillPoints < skill.cost) return false;

    // 3. 선행 스킬 조건
    if (!string.IsNullOrEmpty(skill.parentId))
        if (!_learned.ContainsKey(skill.parentId)) return false;

    // 4. 최대 랭크 초과 여부
    if (_learned.TryGetValue(id, out var state))
        return state.rank < skill.maxRank;

    return true;
}
```

#### 시스템 연결 구조

```
skills.json → SkillDatabase (딕셔너리 캐싱)
                    ↓ Get(id)
SkillTreeSystem ← CanLearn / Learn / IsLearned
                    ↓
SkillUIController → K키로 패널 토글, 노드 일괄 갱신
                    ↓
SkillNodeUI → 색상(잠김/가능/완료), 클릭, 드래그, 툴팁
                    ↓
PlayerSkillCaster → 실제 스킬 발동 (쿨다운, 이펙트, 애니메이션)
```

---

### 2.6 상점 시스템

#### 기능

- 구매: 인벤토리 아이템 DB 기반 아이템 목록 표시
- 판매: 현재 인벤토리 아이템 목록 표시, 선택 판매
- GoldManager로 골드 통합 관리
- E키 상호작용 범위 진입 시 활성화

---

### 2.7 오브젝트 풀링

#### 설계 배경

파이어볼, 폭발 이펙트, 데미지 팝업처럼 자주 생성/파괴되는 오브젝트를 반복적으로 Instantiate/Destroy하면 GC 스파이크로 프레임 드랍이 발생합니다.

#### 구조

```
ObjectPoolManager (Singleton, DontDestroyOnLoad)
├── Dictionary<string, Queue<GameObject>> _pools
│                           ↑ 키 → 대기 중인 오브젝트 큐
├── Get(key)    : 큐에서 꺼내 활성화, 비었으면 자동 확장
├── Return(key) : 비활성화 후 큐에 반환
└── RegisterPrefab() : 런타임 풀 등록 (미니맵 마커 등)
```

#### IPoolable 인터페이스

풀에서 꺼낼 때/반환할 때 초기화가 필요한 컴포넌트는 `IPoolable`을 구현합니다.

```csharp
public interface IPoolable
{
    void OnSpawn();    // Get() 시 호출 - 초기화
    void OnDespawn();  // Return() 시 호출 - 정리
}
```

#### 적용 대상

| 오브젝트 | 풀 키 | 용도 |
|----------|-------|------|
| FireBall | `"fireball"` | 플레이어/보스 발사체 |
| ExplosionVFX | `"explosion"` | 폭발 이펙트 |
| DamagePopup | `"damagePopup"` | 데미지 숫자 UI |
| EnemyMarker | `"minimap_enemy"` | 미니맵 적 마커 |
| NPCMarker | `"minimap_npc"` | 미니맵 NPC 마커 |

#### 성능 측정 결과 (FireBall 100발 기준)

| 항목 | 풀링 OFF | 풀링 ON | 개선 |
|------|----------|---------|------|
| Instantiate 호출 | 100 | **0** | -100% |
| Destroy 호출 | 100 | **0** | -100% |
| 메모리 사용량 | 19,904 KB | **8,424 KB** | **-57%** |

---

### 2.8 Save / Load 시스템

#### 저장 대상

```
SaveData
├── playerData       - 위치, HP, 레벨, 경험치, 스킬포인트, 골드
├── inventoryItems   - 아이템 ID + 수량 목록
├── learnedSkills    - 스킬 ID + 랭크 목록
├── questSaveData    - 퀘스트 상태 + 목표 진행도
└── monsterStates    - 몬스터별 생사 여부 + 현재 HP
```

#### ScriptableObject + JSON 분리 전략

```
정적 데이터 (변하지 않음) → ScriptableObject
  MonsterDefinition: 이름, 최대 HP, 공격력, 이동속도 등

동적 데이터 (런타임에 변함) → JSON 저장
  MonsterSaveData: 현재 HP, 사망 여부, 위치
```

| 방식 | Save 메모리 | Load 메모리 |
|------|-------------|-------------|
| 전부 JSON | 104 KB | 212 KB |
| SO + JSON | **96 KB** | **196 KB** |

#### 세이브 포인트

씬에 배치된 `SavePointTrigger`에 진입 시 자동 저장. `SaveManager.Save()` → JSON → `Application.persistentDataPath`.

---

### 2.9 데미지 팝업 시스템

#### 기능

- 일반 데미지 / 치명타 / 힐 타입별 색상 구분
- 오브젝트 풀링으로 팝업 재사용
- 위로 떠오르는 애니메이션 + 페이드 아웃
- 카메라를 향하는 빌보드 처리

#### 타입별 색상

| 타입 | 색상 | 크기 |
|------|------|------|
| Normal | 흰색 | 기본 |
| Critical | 빨간색 | 1.5배 |
| Heal | 초록색 | 기본 |

#### 사용법

```csharp
// 데미지 팝업 표시
DamagePopupManager.Instance.Show(
    position: transform.position,
    amount: 150,
    type: DamageType.Critical
);
```

---

### 2.10 미니맵 시스템

#### 구조

```
MinimapManager (Singleton)
├── MinimapCamera (Orthographic, 위에서 아래 촬영)
│     └── RenderTexture → UI RawImage에 표시
├── PlayerMarker  - 플레이어 위치 + 방향 표시
├── EnemyMarkers  - 적 위치 (사망 시 자동 제거)
└── NPCMarkers    - NPC 위치 (정적)
```

#### 마커 풀링

적 마커는 적이 많은 씬에서 잦은 생성/제거가 발생하므로 ObjectPoolManager와 연동합니다.

```csharp
// 적 사망 시 마커 반환
ObjectPoolManager.Instance.Return("minimap_enemy", marker);

// 새 적 등록 시 마커 꺼내기
ObjectPoolManager.Instance.Get("minimap_enemy");
```

#### 성능 최적화

- 적 스캔은 매 프레임이 아닌 **1.5초 간격**으로 제한 (`FindObjectsOfType` 비용 절감)
- NPC 마커는 정적이므로 **초기화 시 1회**만 등록

#### M키 줌 3단계 순환

```
minZoom(15) → defaultZoom(30) → maxZoom(60) → minZoom ...
```

---

### 2.11 카메라 락온 시스템

#### 기능

- TAB키로 가장 가까운 적 락온/해제
- 락온 중 카메라가 적을 중심으로 회전
- 적 사망 시 자동으로 락온 해제
- 락온 해제 시 기본 3인칭 카메라로 복귀

#### 구현

```csharp
// 가장 가까운 적 탐색
private Transform FindNearestEnemy()
{
    EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
    Transform nearest = null;
    float minDist = float.MaxValue;

    foreach (var enemy in enemies)
    {
        if (enemy.isDead) continue;
        float dist = Vector3.Distance(transform.position, enemy.transform.position);
        if (dist < minDist) { minDist = dist; nearest = enemy.transform; }
    }
    return nearest;
}
```

---

## 3. 사용된 디자인 패턴

| 패턴 | 적용 위치 | 목적 |
|------|-----------|------|
| **State Pattern** | PlayerMovement, EnemyAI, BossAI | 상태별 로직 분리, 전환 명확화 |
| **Singleton Pattern** | QuestManager, ObjectPoolManager, DamagePopupManager, MinimapManager | 전역 접근이 필요한 매니저 클래스 |
| **Observer Pattern** | 퀘스트 목표 갱신, 스킬 UI 갱신 | 느슨한 결합으로 변경 전파 |
| **Strategy Pattern** | FuzzyAttack, FuzzyBossAttack | 공격 방식 선택 로직 교체 가능 |
| **Object Pool Pattern** | ObjectPoolManager | GC 부담 감소, 오브젝트 재사용 |
| **Data-Driven Design** | MonsterDefinition(SO), SkillDatabase(JSON) | 코드 수정 없이 데이터 변경 |
| **Dependency Injection** | SkillNodeUI.SetController() | Inspector 할당 없이 참조 주입 |

---

## 4. 성능 최적화

### 4.1 Camera.main 캐싱

`Camera.main`은 내부적으로 `FindGameObjectWithTag("MainCamera")`를 매번 호출합니다.

```csharp
// Before: 매 프레임 탐색
void LateUpdate() => healthBar.transform.forward = Camera.main.transform.forward;

// After: Awake에서 1회 캐싱
private Camera _mainCamera;
void Awake() => _mainCamera = Camera.main;
void LateUpdate() => healthBar.transform.forward = _mainCamera.transform.forward;
```

적용 파일: `EnemyAI.cs`, `ShopKepper.cs`, `DamagePopup.cs`

### 4.2 HP바/경험치바 Update 최적화

```csharp
// Before: 값 변화 없어도 매 프레임 Lerp 연산
void Update() => _slider.value = Mathf.Lerp(_slider.value, _target, Time.deltaTime * 10f);

// After: 애니메이션 중일 때만 실행
private bool _isAnimating;
void Update()
{
    if (!_isAnimating) return;
    _slider.value = Mathf.Lerp(_slider.value, _target, Time.deltaTime * 10f);
    if (Mathf.Abs(_slider.value - _target) < 0.01f) { _slider.value = _target; _isAnimating = false; }
}
```

적용 파일: `HealthBar.cs`, `ExpBar.cs`

### 4.3 미니맵 스캔 주기 제한

`FindObjectsOfType<EnemyAI>()`는 씬 전체를 탐색하는 비용이 큰 함수입니다.

```csharp
private float _enemyScanTimer;
private const float EnemyScanInterval = 1.5f; // 1.5초마다 1회 스캔

void Update()
{
    _enemyScanTimer += Time.deltaTime;
    if (_enemyScanTimer < EnemyScanInterval) return;
    _enemyScanTimer = 0f;
    // FindObjectsOfType 실행
}
```

### 4.4 오브젝트 풀링

파이어볼 100발 기준 메모리 57% 절감, GC 스파이크 제거.
→ [2.7 오브젝트 풀링](#27-오브젝트-풀링) 참고

### 4.5 ScriptableObject + JSON 분리

정적 데이터는 SO로 공유, 동적 상태만 JSON 저장.
→ 메모리 8% 절감, 에디터에서 밸런싱 용이

---

## 5. 프로젝트 구조

```
Assets/Scripts/
├── Player/
│   ├── PlayerMovement.cs       - 캐릭터 컨트롤러, 상태 머신 관리
│   ├── PlayerProgress.cs       - 레벨, 경험치, 스킬포인트
│   ├── PlayerSkillCaster.cs    - 스킬 발동 (쿨다운, 이펙트)
│   ├── PlayerIdleState.cs
│   ├── PlayerWalkState.cs
│   ├── PlayerRunState.cs
│   ├── PlayerRollState.cs
│   ├── PlayerAttack1State.cs
│   ├── PlayerAttack2State.cs
│   └── PlayerAttack3State.cs
│
├── EnemyScript/
│   ├── EnemyAI.cs              - 일반 적 FSM, FOV 감지
│   ├── BossAI.cs               - 보스 FSM, 페이즈 전환
│   ├── FuzzyAttack.cs          - 퍼지 로직 공격 선택
│   ├── FuzzyBossAttack.cs
│   ├── MonsterDefinition.cs    - 몬스터 ScriptableObject
│   ├── FireBall.cs             - 발사체 로직
│   ├── EnemyIdleState.cs
│   ├── EnemyChaseState.cs
│   ├── EnemyPatrolState.cs
│   ├── EnemyAttackState.cs
│   ├── EnemyReturnState.cs
│   ├── BossAttackState.cs
│   └── MonsterSpawnManager.cs
│
├── Quest/
│   ├── QuestData.cs            - 퀘스트 ScriptableObject
│   ├── QuestManager.cs         - 퀘스트 상태 관리 (Singleton)
│   ├── QuestGiver.cs           - NPC 상호작용
│   ├── QuestGate.cs            - 퀘스트 완료 후 문 열기
│   └── DialogueUI.cs           - 대화창 UI
│
├── Inventory/
│   ├── InventorySystem.cs      - 아이템 저장소
│   ├── InventoryItemUI.cs      - 아이템 슬롯 UI
│   ├── InventoryContextMenu.cs - 우클릭 메뉴
│   ├── DragDrop.cs             - 드래그앤드롭 처리
│   └── PickableItem.cs         - 씬 아이템 습득
│
├── Skills/
│   ├── SkillDefinition.cs      - 스킬 데이터 구조체
│   ├── SkillDatabase.cs        - JSON → 딕셔너리 캐싱
│   ├── SkillTreeSystem.cs      - 습득 로직
│   ├── SkillUIController.cs    - 스킬창 UI 관리
│   ├── SkillNodeUI.cs          - 개별 노드 UI, 드래그
│   └── SkillTooltipPanel.cs    - 마우스 오버 툴팁
│
├── QuickSlot/
│   ├── QuickSlotBar.cs         - 슬롯 바 관리
│   └── QuickSlot.cs            - 단축키 실행, 드롭 수신
│
├── Shop/
│   ├── ShopKepper.cs           - 상점 NPC
│   ├── ShopBuyItemUI.cs        - 구매 아이템 UI
│   ├── ShopSellItemUI.cs       - 판매 아이템 UI
│   └── GoldManager.cs          - 골드 관리
│
├── Pool/
│   ├── ObjectPoolManager.cs    - 풀 매니저 (Singleton)
│   ├── PooledObject.cs         - 풀 키 보유 컴포넌트
│   ├── PooledVFX.cs            - VFX 자동 반환
│   └── IPoolable.cs            - 초기화 인터페이스
│
├── Save/
│   ├── SaveData.cs             - 저장 데이터 구조
│   ├── SaveManager.cs          - JSON 직렬화/역직렬화
│   ├── SavePointTrigger.cs     - 세이브 포인트
│   └── MonsterSaveData.cs      - 몬스터 동적 상태
│
├── UI/
│   ├── DamagePopup.cs          - 데미지 팝업 애니메이션
│   ├── DamagePopupManager.cs   - 팝업 풀링 관리
│   └── DamageType.cs           - 데미지 타입 열거형
│
├── Minimap/
│   ├── MinimapManager.cs       - 미니맵 카메라, 마커 관리
│   └── MinimapSetup.cs         - 초기 설정 도우미
│
├── CameraMovement.cs           - 3인칭 카메라, 락온
├── StateMachine.cs             - 범용 상태 머신
├── State.cs                    - 상태 기반 클래스
├── HealthBar.cs                - HP바 UI (Lerp 애니메이션)
├── ExpBar.cs                   - 경험치바 UI
├── StaminaBar.cs               - 스태미나바 UI
├── WeaponHitPoints.cs          - 무기 판정 포인트
└── WeaponTrail.cs              - 무기 트레일 렌더러
```

---

## 6. 버그 수정 기록

### FireBall 플레이어 데미지 미적용 버그

**증상**: 보스가 발사한 FireBall이 플레이어에게 충돌해도 데미지가 적용되지 않음

**원인**: `ApplyExplosionDamage()` 에서 거리/장애물 체크 후 `TakeDamage()` 호출 누락

```csharp
// Before - TakeDamage 호출 없음
if (Physics.Raycast(position, dir.normalized, dist, obstacleMask)) return;
// 여기서 끝

// After - 데미지 적용 추가
if (Physics.Raycast(position, dir.normalized, dist, obstacleMask)) return;
_player.TakeDamage(_damage); // 추가
```

**파일**: `Assets/Scripts/EnemyScript/FireBall.cs`
