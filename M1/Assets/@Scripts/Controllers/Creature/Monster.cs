using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Monster : Creature
{
    public override ECreatureState CreatureState 
	{ 
		get { return base.CreatureState;  }
		set
        {
			if(_creatureState != value)
            {
				base.CreatureState = value;
				switch(value)
                {
					case ECreatureState.Idle:
						UpdateAITick = 0.5f;
                        break;
                    case ECreatureState.Move:
						UpdateAITick = 0.0f;
						break;
					case ECreatureState.Skill:
						UpdateAITick = 0.0f;
						break;
					case ECreatureState.Dead:
						UpdateAITick = 1.0f;
						break;
				}
            }
        }
	}

    public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = ECreatureType.Monster;

		StartCoroutine(CoUpdateAI());

		return true;
	}

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

		//State
		CreatureState = ECreatureState.Idle;

		// Skill
		Skills = gameObject.GetOrAddComponent<SkillComponent>();
		Skills.SetInfo(this, CreatureData.SkillIdList);
    }

    private void Start()
    {
		//Init()���� �����Ű�� �ʹ� ������ ��ġ�� ����ְԵ�. objMngr���� instantiate�� �� transform.postion�� �������ֱ� ������ �� ���ķ� ��ġ �����ߵ�
		//awake�ܰ� ������ Start�Լ�, �� �����ֱ⸦ �˾ƾ� ������ �ڵ���
		_initPos = transform.position;
    }

    #region AI
	Vector3 _destPos;        //�����ϴ� ��ġ����
	Vector3 _initPos;		//�����Ÿ� �̻� �־��� �� �ٽ� ���ư� ���� ������ ����

    protected override void UpdateIdle()
    {
        //Patrol
        {
			//TODO
			//10% Ȯ���� �Ÿ��� �̵��ϴ� �ڵ� �ۼ�

			int patrolPercent = 10;
			int rand = Random.Range(0, 100);
			if(rand <= patrolPercent)
            {
				_destPos = _initPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2));
				CreatureState = ECreatureState.Move;
				return;
            }
        }


		//search Player
		Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
		if(creature != null)
        {
			Target = creature;
			CreatureState = ECreatureState.Move;
			return;
        }
	}
	protected override void UpdateMove()
	{

		if(Target == null)
        {
			//�⺻���� �̵�����
			//Patrol or Return
			Vector3 dir = (_destPos - transform.position);

			if (dir.sqrMagnitude <= 0.01f)
			{
				CreatureState = ECreatureState.Idle;
				return;
			}

			//SetRigidBodyVelocity(dir.normalized * MoveSpeed);
		}
		else
        {
			//Ÿ���� �Ѿư��� ����
			//Chase
			SkillBase skill = Skills.GetReadySkill();
			ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, skill);

			//�ʹ� �־����� ����
			if(Target.IsValid() == false)
            {
				Target = null;
				_destPos = _initPos;
				return;
            }

		}
	}
	protected override void UpdateSkill()
	{
		if(Target.IsValid() == false)
        {
			Target = null;
			_destPos = _initPos;
			CreatureState = ECreatureState.Move;
			return;
        }
	}

	protected override void UpdateDead()
	{
	}
	#endregion

	#region Battle
	public override void OnDamaged(BaseObject attacker, SkillBase skill)
	{
		base.OnDamaged(attacker, skill);
	}

	public override void OnDead(BaseObject attacker, SkillBase skill)
	{
		base.OnDead(attacker, skill);

		// TODO : Drop Item

		Managers.Object.Despawn(this);
	}
	#endregion
}
