using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class Airborne : CCBase
{
    [SerializeField]
    private float _airborneDistance = 5.0f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void ApplyEffect()
    {
        base.ApplyEffect();

        StopCoroutine(DoAirBorn(lateState));
        StartCoroutine(DoAirBorn(lateState));
    }

    //TODO
    //��� �߿� �� ����� �´� ���
    //��� �߿� �˹� ���ϴ� ���
    //�˹��߿� ��� �ϴ� ���

    IEnumerator DoAirBorn(ECreatureState lastState)
    {
        Vector3 originalPosition = Owner.SkeletonAnim.transform.localPosition;
        Vector3 upPosition = originalPosition + Vector3.up * _airborneDistance;

        float halfTickTime = EffectData.TickTime * 0.5f;

        //���� �ö� ��
        for(float t = 0; t < halfTickTime; t += Time.deltaTime)
        {
            float normalizedTime = t / halfTickTime;
            Owner.SkeletonAnim.transform.localPosition = Vector3.Lerp(originalPosition, upPosition, normalizedTime);
            yield return null;
        }

        //�Ʒ��� ������ �� 
        for (float t = 0; t < halfTickTime; t += Time.deltaTime)
        {
            float normalizedTime = t / halfTickTime;
            Owner.SkeletonAnim.transform.localPosition = Vector3.Lerp(upPosition, originalPosition, normalizedTime);
            yield return null;
        }

        Owner.SkeletonAnim.transform.localPosition = originalPosition;

        //��� ���°� ���� �� ���� ����
        if (Owner.CreatureState == ECreatureState.OnDamaged)
            Owner.CreatureState = lastState;

        ClearEffect(EEffectClearType.EndOfAirborne);
    }

    protected override IEnumerator CoStartTimer()
    {
        //����� Ÿ�̸� ����

        yield break;
    }
}
