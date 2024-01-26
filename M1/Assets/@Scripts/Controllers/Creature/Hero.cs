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

	public float StopDistance { get; private set; } = 1.0f;
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
				return;
			}
			ChaseOrAttackTarget(AttackDistance, SearchDistance);
			return;
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
		//���Ϳʹ� �޸� ��ų�� �� �� ������ �̵��� ���� ����. ������ ���� �ֱ� ������
		//��ų Ani Duration�� ���������� ������ ����ų �� ����. 

		if(HeroMoveState == EHeroMoveState.ForceMove)
        {
			CreatureState = ECreatureState.Move;
			return;
        }

		//�����ٰ� ���Ͱ� �׾����� �ٽ� ķ����  �̵��ؾߵ�=> Move���� ķ���� �̵��ϴ� state �б�����
		if(_target.IsValid() == false)
        {
			CreatureState = ECreatureState.Move;
			return;
        }
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
		float attackDistanceSqr = attackRange * attackRange;

		if (distToTargetSqr <= attackDistanceSqr)
		{
			// ���� ���� �̳��� ���Դٸ� ����.
			CreatureState = ECreatureState.Skill;
			return;
		}
		else
		{
			// ���� ���� ���̶�� ����.
			SetRigidBodyVelocity(dir.normalized * MoveSpeed);

			// �ʹ� �־����� ����.
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
