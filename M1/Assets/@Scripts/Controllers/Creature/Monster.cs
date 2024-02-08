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
		//Init()에서 실행시키면 너무 빠르게 위치를 잡아주게됨. objMngr에서 instantiate할 때 transform.postion을 생성해주기 때문에 그 이후로 위치 잡아줘야됨
		//awake단계 이후인 Start함수, 즉 생명주기를 알아야 가능한 코드임
		_initPos = transform.position;
    }

    #region AI
	Vector3 _destPos;        //가야하는 위치정보
	Vector3 _initPos;		//일정거리 이상 멀어질 때 다시 돌아갈 원점 포지션 정보

    protected override void UpdateIdle()
    {
        //Patrol
        {
			//TODO
			//10% 확률로 거리를 이동하는 코드 작성

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
			//기본적인 이동상태
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
			//타겟을 쫓아가는 상태
			//Chase
			SkillBase skill = Skills.GetReadySkill();
			ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, skill);

			//너무 멀어지면 포기
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
