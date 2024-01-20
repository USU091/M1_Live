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
		CreatureState = ECreatureState.Idle;
		Speed = 3.0f;

		StartCoroutine(CoUpdateAI());

		return true;
	}

    private void Start()
    {
		//Init()에서 실행시키면 너무 빠르게 위치를 잡아주게됨. objMngr에서 instantiate할 때 transform.postion을 생성해주기 때문에 그 이후로 위치 잡아줘야됨
		//awake단계 이후인 Start함수, 즉 생명주기를 알아야 가능한 코드임
		_initPos = transform.position;
    }

    #region AI
    public float SearchDistance { get; private set; } = 8.0f;
	public float AttackDistance { get; private set; } = 4.0f;

	Creature _target;    //타겟 정보
	Vector3 _destPos;        //가야하는 위치정보
	Vector3 _initPos;		//일정거리 이상 멀어질 때 다시 돌아갈 원점 포지션 정보

    protected override void UpdateIdle()
    {
		Debug.Log("Idle");
        //Patrol
        {
			//TODO
			//10% 확률로 거리를 이동하는 코드 작성

			int patrolPercent = 10;
			int rand = Random.Range(0, 100);
			if(rand <= patrolPercent)
            {
				_destPos = _initPos + new Vector3(Random.Range(0, 2), Random.Range(0, 2));
				CreatureState = ECreatureState.Move;
				return;
            }
        }


        //search Player
        {
			Creature target = null;
			float bestDistanceSqr = float.MaxValue; //가장 큰값으로 할당
			float searchDistanceSqr = SearchDistance * SearchDistance;

			foreach (Hero hero in Managers.Object.Heroes)
            {
				Vector3 dir = hero.transform.position - transform.position;
				float distToTargetSqr = dir.sqrMagnitude;

				Debug.Log(distToTargetSqr);

				if (distToTargetSqr > searchDistanceSqr)	//서칭범위 이상으로 멀리 있으면 스킵
					continue;

				if (distToTargetSqr > bestDistanceSqr)	//이미 더 좋은 후보를 찾았다면 스킵
					continue;

				target = hero;
				bestDistanceSqr = distToTargetSqr;
            }

			_target = target;
			if (_target != null)
				CreatureState = ECreatureState.Move;

		}
	}
	protected override void UpdateMove()
	{
		Debug.Log("Move");

		if(_target == null)
        {
			//기본적인 이동상태
			//Patrol or Return
			Vector3 dir = (_destPos - transform.position);
			float moveDist = Mathf.Min(dir.magnitude, Time.deltaTime * Speed);
			transform.TranslateEx(dir.normalized * moveDist);

			if (dir.sqrMagnitude <= 0.01f)
			{
				CreatureState = ECreatureState.Idle;
			}
		}
		else
        {
			//타겟을 쫓아가는 상태
			//Chase
			Vector3 dir = (_target.transform.position - transform.position);
			float distToTargetSqr = dir.sqrMagnitude;
			float attackDistanceSqr = AttackDistance * AttackDistance;

			if(distToTargetSqr < attackDistanceSqr)
            {
				//공격범위 이내로 들어왔으므로 공격
				CreatureState = ECreatureState.Skill;
				StartWait(2.0f);	//2초 동안 풀타임 관리함, 공격같은 경우 애니메이션타임으로 따져도됨
            }
            else
            {
				//공격 범위 밖이라면 추적
				float moveDist = Mathf.Min(dir.magnitude, Time.deltaTime * Speed);
				transform.TranslateEx(dir.normalized * moveDist);

				//너무 멀어지면 포기
				float searchDistanceSqr = SearchDistance * SearchDistance;
				if(distToTargetSqr > searchDistanceSqr)
                {
					_destPos = _initPos;
					_target = null;
					CreatureState = ECreatureState.Move;
                }

			}

		}
	}
	protected override void UpdateSkill()
	{
		Debug.Log("Skill");

		if (_coWait != null)
			return; //아직 null이 아니면 코루틴이 실행중이므로 기다려야됨

		CreatureState = ECreatureState.Move;
	}

	protected override void UpdateDead()
	{
		Debug.Log("Dead");
	}
	#endregion
}
