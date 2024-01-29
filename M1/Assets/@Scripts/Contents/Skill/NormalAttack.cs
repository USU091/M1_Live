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

    //��ų ���������
    protected override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.ToString().Contains(SkillData.AnimName))
            OnAttackEvent();
    }

    protected virtual void OnAttackEvent()
    {
        if (Owner.Target.IsValid() == false)
            return;

        //���� ����ִٸ� �����ϴ� �ڵ� �ۼ�
        if(SkillData.ProjectileId == 0)
        {
            //Melee, ����
            //Owner.Target.OnDamaged(Owner, this);
        }
        else
        {
            // Ranged, ���Ÿ�
            //GenerateProjectile(Owner, Owner.CenterPosition);
        }
    }

    protected override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if (Owner.Target.IsValid() == false)
            return;

        //���͸� �����ϴٰ� �߰��� �ٸ� �������� ����Ʈ�� ��� �̵��ϴ� ���� 1������ �ξ��� ������, Skill������ ������ �̵��ϵ��� ����
        if (Owner.CreatureState == Define.ECreatureState.Skill)
            Owner.CreatureState = Define.ECreatureState.Move;
    }

}