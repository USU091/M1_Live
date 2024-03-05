using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoisonSpreadAoE : AoEBase
{
    protected override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
    }

    public override bool Init()
    {
        if( base.Init() == false)
            return false;

        return true;
    }

    public override void SetInfo(int dataId, BaseObject owner, SkillBase skill)
    {
        base.SetInfo(dataId, owner, skill);

        SetSpineAnimation(_aoEDate.SkeletonDataID, SortingLayers.SPELL_INDICATOR);
        PlayAnimation(0, _aoEDate.AnimName, false);

        StartCoroutine(CoReserveDestroy());
        StartCoroutine(CoDetectTargetsPeriodically());
    }

    private IEnumerator CoDetectTargetsPeriodically()
    {
        while(true)
        {
            CoDetectTargetsPeriodically();
            yield return new WaitForSeconds(1f);
        }
    }

    private void DetectTargets()
    {
        List<Creature> detectedCreatures = new List<Creature>();
        List<Creature> rangeTargets = Managers.Object.FindCircledRangeTargets(Owner, transform.position, _radius);

        foreach(var target in rangeTargets)
        {
            Creature t = target as Creature;
            detectedCreatures.Add(target);

            if(t.IsValid() && _targets.Contains(target) == false)
            {
                _targets.Add(t);

                List<EffectBase> effects = target.Effects.GenerateEffects(_aoEDate.EnemyEffects.ToArray(), Define.EEffectSpawnType.External, _skillBase);
                _activeEffects.AddRange(effects);
            }    
        }

        //������ Ž���Ǿ����� ���� ���� �ۿ� �ִ� Creature ����
        foreach(var target in _targets.ToArray())
        {
            if(target.IsValid() && detectedCreatures.Contains(target) == false)
            {
                //���� ������ ���� Creature ó��
                _targets.Remove(target);
                RemoveEffect(target);

            }
        }    

    }

    private void RemoveEffect(Creature target)
    {
        List<EffectBase> effectsToRemove = new List<EffectBase>();

        foreach(var effect in _activeEffects)
        {
            if(target.Effects.ActiveEffects.Contains(effect))
            {
                effect.ClearEffect(Define.EEffectClearType.TriggerOutAoE);
                effectsToRemove.Add(effect);
            }
        }

        foreach(var effect in effectsToRemove)
        {
            _activeEffects.Remove(effect);
        }
    }



}
