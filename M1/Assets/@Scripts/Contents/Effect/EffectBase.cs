using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using static Define;

//Buff, Debuff, CC 등
//만약 Buff, Debuff등에 걸리게 되면 머리 위에 이펙트 효과가 나와야 함. 그거를 스폰하기 위해서는 BaseObject를 상속받아야 될 것으로 보임
public class EffectBase : BaseObject
{
    public Creature Owner;
    public SkillBase Skill;
    public EffectData EffectData;
    public EEffectType EffectType;

    //얼마동안 들고 있어야 하는지
    protected float Remains { get; set; }
    protected EEffectSpawnType _spawnType;
    protected bool Loop { get; set; } = true;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public virtual void SetInfo(int templateID, Creature owner, EEffectSpawnType spawnType, SkillBase skill)
    {
        DataTemplateID = templateID;
        EffectData = Managers.Data.EffectDic[templateID];

        Owner = owner;
        Skill = skill;
        _spawnType = spawnType;

        if (string.IsNullOrEmpty(EffectData.SkeletonDataID) == false)
            SetSpineAnimation(EffectData.SkeletonDataID, SortingLayers.SKILL_EFFECT);


        EffectType = EffectData.EffectType;

        if (_spawnType == EEffectSpawnType.External)
            Remains = float.MaxValue;
        else
            Remains = EffectData.TickTime * EffectData.TickCount;
    }

    /* 
    도트 뎀
    도트 힐
    패시브 영구적
    힘 버프
    체력 버프
    민첩 디버프
    ...
     */
    //이펙트를 적용하기 위한 함수. 만약 effect실행 오브젝트가 있다면 스폰하는 함수와 코루틴을 사용하여 실제로 DotDamage를 줄 수있도록 함수 구현
    public virtual void ApplyEffect()
    {
        //이펙트 보여주는 함수 호출
        ShowEffect();
        //실제 DotDamage(Debuff, Buff, CC등)주는 함수 호출
        StartCoroutine(CoStartTimer());
    }

    protected virtual void ShowEffect()
    {
        if (SkeletonAnim != null && SkeletonAnim.skeletonDataAsset != null)
            PlayAnimation(0, AnimName.IDLE, Loop);
    }

    protected void AddModifier(CreatureStat stat, object source, int order = 0)
    {
        if(EffectData.Amount != 0)
        {
            StatModifier add = new StatModifier(EffectData.Amount, EStatModType.Add, order, source);
            stat.AddModifier(add);
        }

        if(EffectData.PercentAdd != 0)
        {
            StatModifier percentAdd = new StatModifier(EffectData.PercentAdd, EStatModType.PercentAdd, order, source);
            stat.AddModifier(percentAdd);
        }

        if(EffectData.PercentMult != 0)
        {
            StatModifier percentMult = new StatModifier(EffectData.PercentMult, EStatModType.PercentMult, order, source);
            stat.AddModifier(percentMult);
        }
    }

    protected void RemoveModifier(CreatureStat stat, object source)
    {
        stat.ClearModifiersFromSource(source);
    }

    public virtual bool ClearEffect(EEffectClearType clearType)
    {
        Debug.Log($"ClearEffect - {gameObject.name} {EffectData.ClassName} -> {clearType}");

        switch(clearType)
        {
            case EEffectClearType.TimeOut:
            case EEffectClearType.TriggerOutAoE:
            case EEffectClearType.EndOfAirborne:
                Managers.Object.Despawn(this);
                break;
            case EEffectClearType.ClearSkill:
                if(_spawnType != EEffectSpawnType.External)
                {
                    Managers.Object.Despawn(this);
                    return true;
                }
                break;
        }
        return false;
    }


    protected virtual void ProcessDot()
    {

    }

    //Remain, TickTime을 사용하여 도트데미지를 구현할 수 있도록 코루틴을 사용함
    protected virtual IEnumerator CoStartTimer()
    {
        float sumTime = 0f;

        ProcessDot();

        while(Remains > 0)
        {
            Remains -= Time.deltaTime;
            sumTime += Time.deltaTime;

            // 틱마다 ProcessDotTick 호출
            if(sumTime >=  EffectData.TickTime)
            {
                ProcessDot();
                sumTime -= EffectData.TickTime;
            }

            yield return null;
        }

        Remains = 0;
        ClearEffect(EEffectClearType.TimeOut);
    }



}
