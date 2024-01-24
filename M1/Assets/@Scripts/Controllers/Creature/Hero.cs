using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;
using static Unity.Burst.Intrinsics.X86.Avx;


public class Hero : Creature
{
	bool _needArrange = true;
	public bool NeedArrange
	{
		get { return _needArrange; }
		set
		{
			_needArrange = value;
			//if(value)

			//else

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
					break;
				case EHeroMoveState.TargetMonster:
					break;
				case EHeroMoveState.ForceMove:
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
	public float SearchDistance { get; private set; } = 8.0f;       //���� ã�� Ž�� ����
	public float AttackDistance		//�����ϴ� ��ų ���� ����
	{
		get
		{
			float targetRadius = (_target.IsValid() ? _target.ColliderRadius : 0);
			return ColliderRadius + targetRadius + 2.0f;        //���� ��ų�̳� ���� ��ų�̳Ŀ� ���� �޶��� �� �ִ� ����
		}
	}


	BaseObject _target;     //Ÿ�� ���. ����

	protected override void UpdateIdle()
	{
		//0. �̵� ���¶�� ���� ����(ex. ����ΰ� ������̴� ä�����̴� �����ʹٿ��ϴ� ���� �׸��ΰ� �̵��ϴ°� �켱������ �� ����)
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			CreatureState = ECreatureState.Move;
			return;
		}

		//0. �ʹ� �־����ٸ� ������ ķ���� �̵�

		//1. ���� ���
		Creature creature = FindClosetInRange(SearchDistance, Managers.Object.Monsters) as Creature;
		if (creature != null)
		{
			_target = creature;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.TargetMonster;
			return;
		}
		//2. �ֺ� Env ä��
		Env env = FindClosetInRange(SearchDistance, Managers.Object.Envs) as Env;
		if (env != null)
		{
			_target = env;
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
			if (_target.IsValid() == false)
			{
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
			}
			ChaseOrAttackTarget(AttackDistance, SearchDistance);
		}

		// 2. �ֺ� Env ä��
		if (HeroMoveState == EHeroMoveState.CollectEnv)
		{
			//���Ͱ� ������ ����
			Creature creature = FindClosetInRange(SearchDistance, Managers.Object.Monsters) as Creature;
			if(creature != null)
            {
				_target = creature;
				HeroMoveState = EHeroMoveState.TargetMonster;
				CreatureState = ECreatureState.Move;
				return;
            }

			//Env �̹� ä�������� ����
			if(_target.IsValid() == false)
            {
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
				return;
            }

			ChaseOrAttackTarget(AttackDistance, SearchDistance);
			return;
		}

		// 3. Camp �ֺ����� ���̱�



		// 4. ��Ÿ(������ ���� ��)
		CreatureState = ECreatureState.Idle;


	}
	protected override void UpdateSkill()
	{

	}
	protected override void UpdateDead()
	{

	}
	//���� ���� �ȿ� ���� object ã�� �Լ�(Env, Monster��)
	BaseObject FindClosetInRange(float range, IEnumerable<BaseObject> objs)
	{
		BaseObject target = null;
		float bestDistanceSqr = float.MaxValue;
		float searchDistanceSqr = range * range;

		foreach (BaseObject obj in objs)
		{
			Vector3 dir = obj.transform.position - transform.position;
			float distToTargetSqr = dir.sqrMagnitude;

			//��ġ �������� �ָ� ������ ��ŵ.
			if (distToTargetSqr > searchDistanceSqr)
				continue;

			//�̹� �� ���� �ĺ��� ã������ ��ŵ.
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
		float attackDIstanceSqr = attackRange * attackRange;

		if(distToTargetSqr <= attackDIstanceSqr)
        {
			//���� ���� �̳��� ���Դٸ� ����
			CreatureState = ECreatureState.Skill;
			return;
        }
        else
        {
			//���ݹ��� ���̶�� ����
			SetRigidBodyVelocity(dir.normalized * MoveSpeed);

			//�ʹ� �־����� ����
			float searchDistanceSqr = chaseRange * chaseRange;
			if(distToTargetSqr > searchDistanceSqr)
            {
				_target = null;
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;

            }
        }
    }
	#endregion

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
}
