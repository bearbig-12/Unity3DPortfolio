# 3D RPG Game Portfolio

Unity 기반 3D RPG 게임 포트폴리오 프로젝트

## 주요 시스템

- **State Pattern 기반 캐릭터 시스템**: 플레이어/적 AI 상태 관리
- **퍼지 로직 보스 AI**: 상황 기반 지능형 공격 선택
- **콤보 공격 시스템**: 프레임 타이밍 기반 입력 처리
- **인벤토리 시스템**: 드래그 앤 드롭, 아이템 스태킹
- **퀘스트 시스템**: 다중 목표, 선행 퀘스트 조건
- **스킬트리 시스템**: 계층적 스킬 잠금 해제
- **상점 시스템**: 구매/판매 인터페이스
- **세이브/로드 시스템**: JSON 기반 게임 상태 저장

## 기술 스택

- Unity 2022.3 LTS
- C#
- NavMesh (AI 경로 탐색)
- JSON 직렬화

## 성능 테스트 결과

### Object Pooling 시스템

#### 오브젝트 풀링이란?

게임에서 오브젝트(총알, 이펙트, 적 등)를 자주 생성/파괴하면 다음 문제가 발생합니다:

1. **Instantiate/Destroy 비용**: 매번 메모리 할당/해제 발생
2. **GC(가비지 컬렉션) 스파이크**: 파괴된 객체 정리 시 프레임 드랍
3. **메모리 파편화**: 반복적인 할당/해제로 메모리 비효율

**오브젝트 풀링**은 미리 오브젝트를 생성해두고 **재사용**하는 방식입니다:

```
[풀링 없음]
발사 → Instantiate() → 충돌 → Destroy() → GC 부담

[풀링 사용]
발사 → Pool.Get() → 충돌 → Pool.Return() → 재사용 대기
```

#### 테스트 결과 (FireBall 100개 기준)

| 항목 | 풀링 OFF | 풀링 ON | 개선 |
|------|----------|---------|------|
| Instantiate 호출 | 100 | **0** | -100% |
| Destroy 호출 | 100 | **0** | -100% |
| 메모리 사용량 | 19,904 KB | **8,424 KB** | **-57%** |

#### 풀링 사용 효과

- **메모리 절약**: 약 11.5MB 감소 (57%)
- **GC 스파이크 제거**: Instantiate/Destroy 0회
- **프레임 안정성**: 대량 오브젝트 발사 시에도 프레임 드랍 없음
- **확장성**: 보스전, 탄막 패턴 등 대량 오브젝트 처리에 적합

#### 적용 대상

- FireBall (플레이어/보스 발사체)
- ExplosionVFX (폭발 이펙트)
- DamagePopup (데미지 숫자 UI)

### Save/Load 시스템 (JSON)

| 몬스터 수 | Save 시간 | Save 메모리 | Load 시간 | Load 메모리 |
|-----------|-----------|-------------|-----------|-------------|
| 3마리 | 2ms | 24KB | 2ms | 12KB |
| 8마리 | <1ms | 20KB | <1ms | 32KB |
| 50마리 | 1ms | 60KB | 1ms | 112KB |
| 100마리 | <1ms | 104KB | <1ms | 212KB |

- **100개 오브젝트 기준 Save/Load 1ms 미만**
- **메모리 사용량: 약 1~2KB/오브젝트**
- 프레임 드랍 없이 실시간 저장 가능

### ScriptableObject + JSON 최적화

정적 데이터(몬스터 스탯)는 ScriptableObject로, 동적 데이터(현재 HP, 위치)만 JSON으로 저장하는 방식 적용

| 방식 | Save 메모리 | Load 메모리 | 개선율 |
|------|-------------|-------------|--------|
| Pure JSON | 104KB | 212KB | - |
| SO + JSON | 96KB | 196KB | **8% 감소** |

**SO + JSON 방식의 장점:**
- 메모리 사용량 감소 (정적 데이터 공유)
- 에디터에서 몬스터 밸런싱 용이
- 새 몬스터 추가 시 코드 수정 불필요
- 기획자가 직접 수치 조정 가능

## 아키텍처

```
Assets/Scripts/
├── Player/           - 플레이어 상태 및 컨트롤러
├── EnemyScript/      - 적 AI 및 보스 시스템
├── Inventory/        - 인벤토리 관리
├── Quest/            - 퀘스트 시스템
├── Skills/           - 스킬트리 시스템
├── Shop/             - 상점 시스템
├── Save/             - 세이브/로드 시스템
├── QuickSlot/        - 퀵슬롯 시스템
├── Pool/             - 오브젝트 풀링 시스템
├── UI/               - 데미지 팝업 등 UI 시스템
├── SceneManagement/  - 비동기 씬 로딩 시스템
└── Environment/      - 환경 상호작용
```

## 사용된 디자인 패턴

- **State Pattern**: 플레이어/적 상태 관리
- **Singleton Pattern**: 게임 매니저 클래스
- **Observer Pattern**: 이벤트 기반 통신
- **Strategy Pattern**: 퍼지 로직 공격 선택
- **Object Pool Pattern**: 오브젝트 재사용으로 GC 부담 감소
- **Data-Driven Design**: ScriptableObject 기반 몬스터 정의

## 버그 수정 기록

### FireBall 플레이어 데미지 미적용 버그

**문제**: 보스가 발사한 FireBall이 플레이어에게 충돌해도 데미지가 적용되지 않음

**원인**: `ApplyExplosionDamage()` 메서드에서 거리/장애물 체크만 하고 실제 `TakeDamage()` 호출이 누락됨

**파일**: `Assets/Scripts/EnemyScript/FireBall.cs`

**수정 전**:
```csharp
private void ApplyExplosionDamage(Vector3 position)
{
    if (damagePlayer)
    {
        if (_player == null) return;

        Vector3 playerPos = _player.transform.position;
        Vector3 dir = playerPos - position;
        float dist = Vector3.Distance(position, playerPos);

        if (dist > explosionRadius) return;

        if (Physics.Raycast(position, dir.normalized, dist, obstacleMask))
            return;

        // 여기서 끝 - TakeDamage() 호출 없음!
    }
    // ...
}
```

**수정 후**:
```csharp
private void ApplyExplosionDamage(Vector3 position)
{
    if (damagePlayer)
    {
        if (_player == null) return;

        Vector3 playerPos = _player.transform.position;
        Vector3 dir = playerPos - position;
        float dist = Vector3.Distance(position, playerPos);

        if (dist > explosionRadius) return;

        if (Physics.Raycast(position, dir.normalized, dist, obstacleMask))
            return;

        // 플레이어에게 데미지 적용 (추가됨)
        _player.TakeDamage(_damage);
    }
    // ...
}
```

**결과**: 보스 FireBall이 플레이어에게 정상적으로 데미지 적용됨
