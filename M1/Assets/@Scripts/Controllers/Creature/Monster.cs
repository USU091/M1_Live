using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Monster : Creature
{
	public Data.MonsterData MonsterData { get { return (Data.MonsterData)CreatureData; } }

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

		ObjectType = EObjectType.Monster;

		StartCoroutine(CoUpdateAI());

		return true;
	}

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

		//State
		CreatureState = ECreatureState.Idle;

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

		if(Target.IsValid() == false)
        {
			//기본적인 이동상태
			//Patrol or Return

			Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
			if(creature != null)
            {
				Target = creature;
				CreatureState = ECreatureState.Move;
				return;
            }

			//Move
			FindPathAndMoveToCellPos(_destPos, MONSTER_DEFAULT_MOVE_DEPTH);
			if(LerpCellPosCompleted)
            {
				CreatureState = ECreatureState.Idle;
				return;
            }

		}
		else
        {
			//타겟을 쫓아가는 상태
			//Chase
			ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, AttackDistance);

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
		//Base 의 UpdateSkill()을 반드시 실행해야함. 스킬이 무조건 지속적으로 실행되도록
		base.UpdateSkill();

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
		int dropItemId = MonsterData.DropItemId;

		RewardData rewardData = GetRandomReward();
		if (rewardData != null)
		{
			ItemHolder itemHolder = Managers.Object.Spawn<ItemHolder>(transform.position, dropItemId);
			Vector2 ran = new Vector2(transform.position.x + Random.Range(-10, -15) * 0.1f, transform.position.y);
			Vector2 ran2 = new Vector2(transform.position.x + Random.Range(10, 15) * 0.1f, transform.position.y);
			Vector2 dropPos = Random.value < 0.5 ? ran : ran2;
			itemHolder.SetInfo(0, rewardData.ItemTemplateId, dropPos);
		}


		Managers.Object.Despawn(this);
	}

	RewardData GetRandomReward()
    {
		if (MonsterData == null)
			return null;

		if (Managers.Data.DropTableDic.TryGetValue(MonsterData.DropItemId, out DropTableData dropTableData) == false)
			return null;

		if (dropTableData.Rewards.Count <= 0)
			return null;

		// 50% 1 , 25% 2

		// 0 ~ 100

		int sum = 0;

		int randValue = UnityEngine.Random.Range(0, 100);

		foreach (RewardData item in dropTableData.Rewards)
		{
			sum += item.Probability;

			if (randValue <= sum)
				return item;

		}

		//return dropTableData.Rewards.RandomElementByWeight(e => e.Probability);   //가중치 쓰려면 이 버전 리턴 해줘야됨, 100% 확률로 아이템이 떨궈짐

		return null;
    }

	int GetRewardExp()
    {
		if (MonsterData == null)
			return 0;
		if (Managers.Data.DropTableDic.TryGetValue(MonsterData.DropItemId, out DropTableData dropTableData) == false)
			return 0;

		return dropTableData.RewardExp;
    }


	#endregion
}
