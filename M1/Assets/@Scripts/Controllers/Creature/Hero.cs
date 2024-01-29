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
				//���� destPos�� ������ ������� mass���� ���ſ�� �ȹи��ϱ�
				//�ڴʰ� �շ��ϴ� ����ε��� ��ġ�鼭 �߾ӿ� ���� �� �ֵ���
				//�߷��� ������ ���� ��ġ�鼭 ���� �� �ֵ��� �������

			}
		}
	}


	EHeroMoveState _heroMoveState = EHeroMoveState.None;

	//���ΰ��� �� �����̰� �ִ��� �����ϱ� ���� State �Լ��� �ϳ� ��������
	//ũ��ó State���� ������ �������� State������ ����
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

		//����� �ΰ����� �Լ� �����ϱ�
		StartCoroutine(CoUpdateAI());

		return true;
	}


	public override void SetInfo(int templateID)
	{
		base.SetInfo(templateID);

		//State
		CreatureState = ECreatureState.Idle;
	}
	//property�ϳ� ��������
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
	//���ΰ� AI �Լ� ����
	public float AttackDistance		//�����ϴ� ��ų ���� ����
	{
		get
		{
			float targetRadius = (Target.IsValid() ? Target.ColliderRadius : 0);
			return ColliderRadius + targetRadius + 2.0f;        //���� ��ų�̳� ���� ��ų�̳Ŀ� ���� �޶��� �� �ִ� ����
		}
	}

	protected override void UpdateIdle()
	{
		//Idle�� �� �����ֱ⸦ ���ϹǷ�, �ٸ� ��ü�� �浹�Ҷ� �и��� ������ �����ϱ� ���ؼ� �ӵ��� 0���� �о����
		SetRigidBodyVelocity(Vector2.zero);

		//0. �̵� ���¶�� ���� ����(ex. ����ΰ� ������̴� ä�����̴� �����ʹٿ��ϴ� ���� �׸��ΰ� �̵��ϴ°� �켱������ �� ����)
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			CreatureState = ECreatureState.Move;
			return;
		}

		//0. �ʹ� �־����ٸ� ������ ķ���� �̵�

		//1. ���� ���
		Creature creature = FindClosetInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
		if (creature != null)
		{
			Target = creature;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.TargetMonster;
			return;
		}
		//2. �ֺ� Env ä��
		Env env = FindClosetInRange(HERO_SEARCH_DISTANCE, Managers.Object.Envs) as Env;
		if (env != null)
		{
			Target = env;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.CollectEnv;
			return;
		}
		//3. HeroCamp�� Pivot�� ���� �𿩾� ��.
		//Idle������ �� �� �� ���� ����Ǿ�� �ϴ� �ڵ��̹Ƿ�, boolŸ�� ������Ƽ ����Ͽ� �� ���� ����ǵ��� ����
		if (NeedArrange)
		{
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.ReturnToCamp;
			return;
		}

	}
	protected override void UpdateMove()
	{
		// 0. ������ �ִٸ�, ���� �̵�
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			//�켱������ ���� ����
			Vector3 dir = HeroCampDest.position - transform.position;

			RigidBody.velocity = Vector3.zero;
			SetRigidBodyVelocity(dir.normalized * MoveSpeed);
			return;
		}

		// 1. �ֺ� ���� ��ġ
		if (HeroMoveState == EHeroMoveState.TargetMonster)
		{
			// ���Ͱ� �׾����� ����
			if (Target.IsValid() == false)
			{
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
			}
			ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
			return;
		}

		// 2. �ֺ� Env ä��
		if (HeroMoveState == EHeroMoveState.CollectEnv)
		{
			//���Ͱ� ������ ����
			Creature creature = FindClosetInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
			if(creature != null)
            {
				Target = creature;
				HeroMoveState = EHeroMoveState.TargetMonster;
				CreatureState = ECreatureState.Move;
				return;
            }

			//Env �̹� ä�������� ����
			if(Target.IsValid() == false)
            {
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
				return;
            }

			ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
			return;
		}

		// 3. Camp �ֺ����� ���̱�
		if(HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
			Vector3 dir = HeroCampDest.position - transform.position;
			float stopDistanceSqr = HERO_DEFAULT_STOP_RANGE * HERO_DEFAULT_STOP_RANGE;
			if(dir.sqrMagnitude <= HERO_DEFAULT_STOP_RANGE)
            {
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Idle;
				NeedArrange = false;
				return;
            }
			else
            {
				//�ָ� �������� �����ϴ� �ӵ��� �������ߵ�
				float ratio = MathF.Min(1, dir.magnitude);  //TEMP
				float moveSpeed = MoveSpeed * (float)MathF.Pow(ratio, 3);
				SetRigidBodyVelocity(dir.normalized * moveSpeed);
				return;
            }
        }


		// 4. ��Ÿ(������ ���� ��)
		CreatureState = ECreatureState.Idle;


	}
	protected override void UpdateSkill()
	{
		SetRigidBodyVelocity(Vector2.zero);


		//���Ϳʹ� �޸� ��ų�� �� �� ������ �̵��� ���� ����. ������ ���� �ֱ� ������
		//��ų Ani Duration�� ���������� ������ ����ų �� ����. 
		if (HeroMoveState == EHeroMoveState.ForceMove)
        {
			CreatureState = ECreatureState.Move;
			return;
        }

		//�����ٰ� ���Ͱ� �׾����� �ٽ� ķ����  �̵��ؾߵ�=> Move���� ķ���� �̵��ϴ� state �б�����
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

	//ķ���� ���϶� ���׸��ϰ� �����ϴٰ� ��� ����ε��� Idle�����̸� �ٽ� �׶��ϰ� ǥ����
	private void TryResizeCollider()
    {
		//�ϴ� �浹ü ���� �۰� ����
		ChangeColliderSize(EColliderSize.Small);

		foreach(var hero in Managers.Object.Heroes)
        {
			if (hero.HeroMoveState == EHeroMoveState.ReturnToCamp)		//ReturnToCamp�� �� ���̶� ������ ������
				return;
        }

		//ReturnToCamp�� �� �� ������ �ݶ��̴� ����
		foreach(var hero in Managers.Object.Heroes)
        {
			//�� ä���̳� �������̸� ��ŵ
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
	//�ִϸ��̼� �̺�Ʈ�� �߻��ɶ����� ��ٷȴٰ� ����ǵ��� ��
    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        base.OnAnimEventHandler(trackEntry, e);
		
		//TODO ���� �ִϸ��̼��� ���� �� �̵��� ������ ��...���� Ȯ��ġ ����
		CreatureState = ECreatureState.Move;

		//Skill
		if (Target.IsValid() == false)
			return;

		//_target.OnDamaged(this);
    }
}
