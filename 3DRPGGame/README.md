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
├── Player/          - 플레이어 상태 및 컨트롤러
├── EnemyScript/     - 적 AI 및 보스 시스템
├── Inventory/       - 인벤토리 관리
├── Quest/           - 퀘스트 시스템
├── Skills/          - 스킬트리 시스템
├── Shop/            - 상점 시스템
├── Save/            - 세이브/로드 시스템
├── QuickSlot/       - 퀵슬롯 시스템
└── Environment/     - 환경 상호작용
```

## 사용된 디자인 패턴

- **State Pattern**: 플레이어/적 상태 관리
- **Singleton Pattern**: 게임 매니저 클래스
- **Observer Pattern**: 이벤트 기반 통신
- **Strategy Pattern**: 퍼지 로직 공격 선택
- **Data-Driven Design**: ScriptableObject 기반 몬스터 정의
