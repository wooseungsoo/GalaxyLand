using UnityEngine;
using DG.Tweening;

public class ItemAcquireFx : MonoBehaviour
{
    private int poolIndex; // 이 오브젝트의 풀 인덱스를 저장하기 위한 변수
    public void SetPoolIndex(int index)
    {
        poolIndex = index;
    }

    public void Explosion(Vector3 from, Vector3 to, float explo_range)
    {
        transform.position = from;
        Sequence sequence = DOTween.Sequence();

        // 시작 크기를 작게 설정하고 커지는 애니메이션 추가
        transform.localScale = Vector3.zero;
        sequence.Append(transform.DOScale(Vector3.one * 0.5f, 0.3f).SetEase(Ease.OutBack));

        // 폭발 효과
        Vector3 randomOffset = Random.insideUnitSphere * explo_range;
        randomOffset.z = 0;
        sequence.Append(transform.DOMove(from + randomOffset, 0.5f).SetEase(Ease.OutCubic));

        // 목표로 이동
        sequence.Append(transform.DOMove(to, 1f).SetEase(Ease.InOutQuad));

        // 도착 시 효과
        sequence.Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        sequence.AppendCallback(() =>
        {
            // 오브젝트를 풀에 반환
            PoolManager.Instance.Return(this.gameObject, poolIndex);
        });
    }
}