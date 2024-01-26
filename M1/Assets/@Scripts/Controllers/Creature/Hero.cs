using Data;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;



public class Hero : Creature
{
	ESkillID _heroSkill = ESkillID.Melee;

	public ESkillID HeroSkill
    {
        get { return _heroSkill; }
        set
        {
			if (_heroSkill != value)
			{
				_heroSkill = value;
				UpdateAnimation();
			}
		}
    }

	bool _needArrange = true;
	public bool NeedArrange
	{
		get { return _needArrange; }
		set
		{
			_needArrange = value;
			if (value)
				ChangeColliderSize(EColliderSize.Big);
			else
				TryResizeCollider();

		}
	}
	public override ECreatureState CreatureState
	{
		get { return _creatureState; }
		set
		{
			if (_creatureState != value)
			{
				base.CreatureState = value;

				if (value == ECreatureState.Move)
					RigidBody.mass = CreatureData.Mass;
				else
					RigidBody.mass = CreatureData.Mass * 0.1f;		
				//먼저 destPos에 도달한 히어로의 mass값이 무거우면 안밀리니까
				//뒤늦게 합류하는 히어로들이 밀치면서 중앙에 들어올 수 있도록
				//중량을 가볍게 만들어서 밀치면서 모일 수 있도록 만들었음

			}
		}
	}


	EHeroMoveState _heroMoveState = EHeroMoveState.None;

	//주인공이 왜 움직이고 있는지 관리하기 위한 State 함수를 하나 생성해줌
	//크리처 State랑은 별개로 이중으로 State관리를 해줌
	public EHeroMoveState HeroMoveState
	{
		get { return _heroMoveState; }
		private set
		{
			_heroMoveState = value;
			switch (value)
			{
				case EHeroMoveState.CollectEnv:
					HeroSkill = ESkillID.Skill2;
					NeedArrange = true;
					break;
				case EHeroMoveState.TargetMonster:
					HeroSkill = ESkillID.Melee;
					NeedArrange = true;
					break;
				case EHeroMoveState.ForceMove:
					NeedArrange = true;
					break;

			}
		}
	}

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = ECreatureType.Hero;


		Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
		Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;


		//히어로 인공지능 함수 구현하기
		StartCoroutine(CoUpdateAI());

		return true;
	}


	public override void SetInfo(int templateID)
	{
		base.SetInfo(templateID);

		//State
		CreatureState = ECreatureState.Idle;
	}
	//property하나 생성해줌
	public Transform HeroCampDest
	{
		get
		{
			HeroCamp camp = Managers.Object.Camp;
			if (HeroMoveState == EHeroMoveState.ReturnToCamp)
				return camp.Pivot;

			return camp.Destination;
		}
	}

	#region AI
	//주인공 AI 함수 구현
	public float SearchDistance { get; private set; } = 8.0f;       //몬스터 찾는 탐색 범위
	public float AttackDistance		//공격하는 스킬 범위 설정
	{
		get
		{
			float targetRadius = (_target.IsValid() ? _target.ColliderRadius : 0);
			return ColliderRadius + targetRadius + 2.0f;        //원격 스킬이냐 근접 스킬이냐에 따라 달라질 수 있는 수식
		}
	}

	public float StopDistance { get; private set; } = 1.0f;
	BaseObject _target;     //타겟 상대. 몬스터

	protected override void UpdateIdle()
	{
		//0. 이동 상태라면 강제 변경(ex. 히어로가 사냥중이던 채굴중이던 포인터다운하는 순간 그만두고 이동하는게 우선순위가 젤 높음)
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			CreatureState = ECreatureState.Move;
			return;
		}

		//0. 너무 멀어졌다면 강제로 캠프로 이동

		//1. 몬스터 사냥
		Creature creature = FindClosetInRange(SearchDistance, Managers.Object.Monsters) as Creature;
		if (creature != null)
		{
			_target = creature;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.TargetMonster;
			return;
		}
		//2. 주변 Env 채굴
		Env env = FindClosetInRange(SearchDistance, Managers.Object.Envs) as Env;
		if (env != null)
		{
			_target = env;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.CollectEnv;
			return;
		}
		//3. HeroCamp의 Pivot을 향해 모여야 함.
		//Idle상태일 때 단 한 번만 실행되어야 하는 코드이므로, bool타입 프로퍼티 사용하여 한 번만 실행되도록 구현
		if (NeedArrange)
		{
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.ReturnToCamp;
			return;
		}

	}
	protected override void UpdateMove()
	{
		// 0. 누르고 있다면, 강제 이동
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			//우선순위가 제일 높음
			Vector3 dir = HeroCampDest.position - transform.position;

			RigidBody.velocity = Vector3.zero;
			SetRigidBodyVelocity(dir.normalized * MoveSpeed);
			return;
		}

		// 1. 주변 몬스터 서치
		if (HeroMoveState == EHeroMoveState.TargetMonster)
		{
			// 몬스터가 죽었으면 포기
			if (_target.IsValid() == false)
			{
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
				return;
			}
			ChaseOrAttackTarget(AttackDistance, SearchDistance);
			return;
		}

		// 2. 주변 Env 채굴
		if (HeroMoveState == EHeroMoveState.CollectEnv)
		{
			//몬스터가 있으면 포기
			Creature creature = FindClosetInRange(SearchDistance, Managers.Object.Monsters) as Creature;
			if(creature != null)
            {
				_target = creature;
				HeroMoveState = EHeroMoveState.TargetMonster;
				CreatureState = ECreatureState.Move;
				return;
            }

			//Env 이미 채집했으면 포기
			if(_target.IsValid() == false)
            {
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
				return;
            }

			ChaseOrAttackTarget(AttackDistance, SearchDistance);
			return;
		}

		// 3. Camp 주변으로 모이기
		if(HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
			Vector3 dir = HeroCampDest.position - transform.position;
			float stopDistanceSqr = StopDistance * StopDistance;
			if(dir.sqrMagnitude <= StopDistance)
            {
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Idle;
				NeedArrange = false;
				return;
            }
			else
            {
				//멀리 있을수록 응집하는 속도가 빨라져야됨
				float ratio = MathF.Min(1, dir.magnitude);  //TEMP
				float moveSpeed = MoveSpeed * (float)MathF.Pow(ratio, 3);
				SetRigidBodyVelocity(dir.normalized * moveSpeed);
				return;
            }
        }


		// 4. 기타(누르다 뗐을 때)
		CreatureState = ECreatureState.Idle;


	}
	protected override void UpdateSkill()
	{
		//몬스터와는 달리 스킬을 쓸 때 강제로 이동할 수도 있음. 도망갈 수도 있기 때문에
		//스킬 Ani Duration이 끝날때까지 억지로 대기시킬 수 없음. 

		if(HeroMoveState == EHeroMoveState.ForceMove)
        {
			CreatureState = ECreatureState.Move;
			return;
        }

		//때리다가 몬스터가 죽었으면 다시 캠프로  이동해야됨=> Move에서 캠프로 이동하는 state 분기잇음
		if(_target.IsValid() == false)
        {
			CreatureState = ECreatureState.Move;
			return;
        }
	}
	protected override void UpdateDead()
	{

	}
	//일정 범위 안에 들어온 object 찾는 함수(Env, Monster등)
	BaseObject FindClosetInRange(float range, IEnumerable<BaseObject> objs)
	{
		BaseObject target = null;
		float bestDistanceSqr = float.MaxValue;
		float searchDistanceSqr = range * range;

		foreach (BaseObject obj in objs)
		{
			Vector3 dir = obj.transform.position - transform.position;
			float distToTargetSqr = dir.sqrMagnitude;

			//서치 범위보다 멀리 있으면 스킵.
			if (distToTargetSqr > searchDistanceSqr)
				continue;

			//이미 더 좋은 후보를 찾았으면 스킵.
			if (distToTargetSqr > bestDistanceSqr)
				continue;

			target = obj;
			bestDistanceSqr = distToTargetSqr;
		}
		return target;
	}


	void ChaseOrAttackTarget(float attackRange, float chaseRange)
	{
		Vector3 dir = (_target.transform.position - transform.position);
		float distToTargetSqr = dir.sqrMagnitude;
		float attackDistanceSqr = attackRange * attackRange;

		if (distToTargetSqr <= attackDistanceSqr)
		{
			// 공격 범위 이내로 들어왔다면 공격.
			CreatureState = ECreatureState.Skill;
			return;
		}
		else
		{
			// 공격 범위 밖이라면 추적.
			SetRigidBodyVelocity(dir.normalized * MoveSpeed);

			// 너무 멀어지면 포기.
			float searchDistanceSqr = chaseRange * chaseRange;
			if (distToTargetSqr > searchDistanceSqr)
			{
				_target = null;
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
			}
			return;
		}
	}
	#endregion

	//캠프로 모일때 조그마하게 응집하다가 모든 히어로들이 Idle상태이면 다시 뚱뚱하게 표현함
	private void TryResizeCollider()
    {
		//일단 충돌체 아주 작게 만듬
		ChangeColliderSize(EColliderSize.Small);

		foreach(var hero in Managers.Object.Heroes)
        {
			if (hero.HeroMoveState == EHeroMoveState.ReturnToCamp)		//ReturnToCamp가 한 명이라도 있으면 리턴함
				return;
        }

		//ReturnToCamp가 한 명도 없으면 콜라이더 조정
		foreach(var hero in Managers.Object.Heroes)
        {
			//단 채집이나 전투중이면 스킵
			if (hero.CreatureState == ECreatureState.Idle)
				hero.ChangeColliderSize(EColliderSize.Big);
        }
    }


	private void HandleOnJoystickStateChanged(EJoystickState joystickState)
	{
		switch (joystickState)
		{
			case EJoystickState.PointerDown:
				HeroMoveState = EHeroMoveState.ForceMove;
				break;
			case EJoystickState.Drag:
				HeroMoveState = EHeroMoveState.ForceMove;
				break;
			case EJoystickState.PointerUp:
				HeroMoveState = EHeroMoveState.None;
				break;
			default:
				break;
		}
	}
	//애니메이션 이벤트가 발생될때까지 기다렸다가 실행되도록 함
    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        base.OnAnimEventHandler(trackEntry, e);
		
		//TODO 공격 애니메이션이 끝난 뒤 이동할 것인지 등...아직 확실치 않음
		CreatureState = ECreatureState.Move;

		//Skill
		if (_target.IsValid() == false)
			return;

		_target.OnDamaged(this);
    }


	#region TestSkill
	protected void SkillTempFunc()
	{
		if (HeroSkill == ESkillID.Melee)
        {
			Skills.AddSkill<Melee>(transform.position);
			
			//Managers.Data.SkillDic.TryGetValue((int)HeroSkill, out SkillData skill);
			//PlayAnimation(0, skill.AniName, true);

		}
		else if(HeroSkill == ESkillID.Skill2)
        {
			Managers.Data.SkillDic.TryGetValue((int)HeroSkill, out SkillData skill);
			PlayAnimation(0, skill.AniName, true);
		}

	}

	protected override void UpdateAnimation()
	{
		switch (CreatureState)
		{
			case ECreatureState.Idle:
				PlayAnimation(0, AnimName.IDLE, true);
				break;
			case ECreatureState.Skill:
				SkillTempFunc();		
				break;
			case ECreatureState.Move:
				PlayAnimation(0, AnimName.MOVE, true);
				break;
			case ECreatureState.Dead:
				PlayAnimation(0, AnimName.DEAD, true);
				RigidBody.simulated = false;
				break;
			default:
				break;
		}
	}

	#endregion
}
