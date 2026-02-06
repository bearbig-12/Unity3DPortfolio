/// <summary>
/// 오브젝트 풀링 인터페이스
/// 풀에서 관리되는 객체가 구현해야 하는 규칙을 정의한다.
///
/// 사용법:
/// public class FireBall : MonoBehaviour, IPoolable
/// {
///     public void OnSpawn() { /* 초기화 코드 */ }
///     public void OnDespawn() { /* 정리 코드 */ }
/// }
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// 풀에서 꺼낼 때 호출된다.
    /// 객체를 초기 상태로 리셋하는 코드를 작성한다.
    /// 예: 속도 초기화, 데미지 초기화 등
    /// </summary>
    void OnSpawn();

    /// <summary>
    /// 풀에 반환할 때 호출된다.
    /// 객체를 정리하는 코드를 작성한다.
    /// 예: 실행 중인 코루틴 정지, 이벤트 해제 등
    /// </summary>
    void OnDespawn();
}
