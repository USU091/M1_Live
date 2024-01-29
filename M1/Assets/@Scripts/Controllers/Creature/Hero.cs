using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class Hero : Creature
{
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

				switch(value)
                {
					case ECreatureState.Move:
						RigidBody.mass = CreatureData.Mass * 5.0f;
						break;
					case ECreatureState.Skill:
						RigidBody.mass = CreatureData.Mass * 500.0f;
						break;
					default:
						RigidBody.mass = CreatureData.Mass;
						break;
				}
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
					NeedArrange = true;
					break;
				case EHeroMoveState.TargetMonster:
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

		//Skill
		Skills = gameObject.GetOrAddComponent<SkillComponent>();
		Skills.SetInfo(this, CreatureData.SkillIdList);
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
	public float AttackDistance		//공격하는 스킬 범위 설정
	{
		get
		{
			float targetRadius = (Target.IsValid() ? Target.ColliderRadius : 0);
			return ColliderRadius + targetRadius + 2.0f;        //원격 스킬이냐 근접 스킬이냐에 따라 달라질 수 있는 수식
		}
	}

	protected override void UpdateIdle()
	{
		//Idle일 때 멈춰있기를 원하므로, 다른 물체와 충돌할때 밀리는 현상을 방지하기 위해서 속도를 0으로 밀어버림
		SetRigidBodyVelocity(Vector2.zero);

		//0. 이동 상태라면 강제 변경(ex. 히어로가 사냥중이던 채굴중이던 포인터다운하는 순간 그만두고 이동하는게 우선순위가 젤 높음)
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			CreatureState = ECreatureState.Move;
			return;
		}

		//0. 너무 멀어졌다면 강제로 캠프로 이동

		//1. 몬스터 사냥
		Creature creature = FindClosetInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
		if (creature != null)
		{
			Target = creature;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.TargetMonster;
			return;
		}
		//2. 주변 Env 채굴
		Env env = FindClosetInRange(HERO_SEARCH_DISTANCE, Managers.Object.Envs) as Env;
		if (env != null)
		{
			Target = env;
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
			if (Target.IsValid() == false)
			{
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
			}
			ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
			return;
		}

		// 2. 주변 Env 채굴
		if (HeroMoveState == EHeroMoveState.CollectEnv)
		{
			//몬스터가 있으면 포기
			Creature creature = FindClosetInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
			if(creature != null)
            {
				Target = creature;
				HeroMoveState = EHeroMoveState.TargetMonster;
				CreatureState = ECreatureState.Move;
				return;
            }

			//Env 이미 채집했으면 포기
			if(Target.IsValid() == false)
            {
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
				return;
            }

			ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
			return;
		}

		// 3. Camp 주변으로 모이기
		if(HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
			Vector3 dir = HeroCampDest.position - transform.position;
			float stopDistanceSqr = HERO_DEFAULT_STOP_RANGE * HERO_DEFAULT_STOP_RANGE;
			if(dir.sqrMagnitude <= stopDistanceSqr)
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
		SetRigidBodyVelocity(Vector2.zero);


		//몬스터와는 달리 스킬을 쓸 때 강제로 이동할 수도 있음. 도망갈 수도 있기 때문에
		//스킬 Ani Duration이 끝날때까지 억지로 대기시킬 수 없음. 
		if (HeroMoveState == EHeroMoveState.ForceMove)
        {
			CreatureState = ECreatureState.Move;
			return;
        }

		//때리다가 몬스터가 죽었으면 다시 캠프로  이동해야됨=> Move에서 캠프로 이동하는 state 분기잇음
		if(Target.IsValid() == false)
        {
			CreatureState = ECreatureState.Move;
			return;
        }
	}
	protected override void UpdateDead()
	{
		SetRigidBodyVelocity(Vector2.zero);

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
		if (Target.IsValid() == false)
			return;

		//_target.OnDamaged(this);
    }
}
