using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttack : SkillBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);
    }

    public override void DoSkill()
    {
        base.DoSkill();

        Owner.CreatureState = Define.ECreatureState.Skill;
        Owner.PlayAnimation(0, SkillData.AnimName, false);


        Owner.LookAtTarget(Owner.Target);
    }

    //스킬 사용했을때
    protected override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.ToString().Contains(SkillData.AnimName))
            OnAttackEvent();
    }

    protected virtual void OnAttackEvent()
    {
        if (Owner.Target.IsValid() == false)
            return;

        if (SkillData.ProjectileId == 0)
        {
            // Melee
            Owner.Target.OnDamaged(Owner, this);
        }
        else
        {
            // Ranged
            //GenerateProjectile(Owner, Owner.CenterPosition);
        }
    }

    protected override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if (Owner.Target.IsValid() == false)
            return;

        //몬스터를 공격하다가 중간에 다른 지점으로 포인트를 찍고 이동하는 것을 1순위로 두었기 때문에, Skill상태일 때에만 이동하도록 만듬
        if (Owner.CreatureState == Define.ECreatureState.Skill)
            Owner.CreatureState = Define.ECreatureState.Move;
    }

}
