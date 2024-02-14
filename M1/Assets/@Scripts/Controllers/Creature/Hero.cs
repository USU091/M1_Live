using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Hero : Creature
{
	public bool NeedArrange{ get; set;}

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


		//Map
		Collider.isTrigger = true;
		RigidBody.simulated = false;
		StartCoroutine(CoUpdateAI());

		return true;
	}

	public override void SetInfo(int templateID)
	{
		base.SetInfo(templateID);

		// State
		CreatureState = ECreatureState.Idle;

	}

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
	protected override void UpdateIdle()
	{
		// 0. �̵� ���¶�� ���� ����
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			CreatureState = ECreatureState.Move;
			return;
		}

		// 0. �ʹ� �־����ٸ� ������ �̵�

		// 1. ����
		Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
		if (creature != null)
		{
			Target = creature;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.TargetMonster;
			return;
		}

		// 2. �ֺ� Env ä��
		Env env = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Envs) as Env;
		if (env != null)
		{
			Target = env;
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.CollectEnv;
			return;
		}

		// 3. Camp �ֺ����� ���̱�
		if (NeedArrange)
		{
			CreatureState = ECreatureState.Move;
			HeroMoveState = EHeroMoveState.ReturnToCamp;
			return;
		}
	}

	protected override void UpdateMove()
	{

		// ����ε��� �ʹ� �־����ٸ� ���� �̵���Ŵ, ForcePath�� ���� �켱������,
		if (HeroMoveState == EHeroMoveState.ForcePath)
		{
			MoveByForcePath();
			return;
		}

		if (CheckHeroCampDistanceAndForcePath())
			return;


		// 0. ������ �ִٸ�, ���� �̵�
		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			EFindPathResult result = FindPathAndMoveToCellPos(HeroCampDest.position, HERO_DEFAULT_MOVE_DEPTH);
			return;
		}

		// 1. �ֺ� ���� ��ġ
		if (HeroMoveState == EHeroMoveState.TargetMonster)
		{
			// ���� �׾����� ����.
			if (Target.IsValid() == false)
			{
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
				return;
			}

			ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, AttackDistance);
			return;
		}

		// 2. �ֺ� Env ä��
		if (HeroMoveState == EHeroMoveState.CollectEnv)
		{
			// ���Ͱ� ������ ����.
			Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
			if (creature != null)
			{
				Target = creature;
				HeroMoveState = EHeroMoveState.TargetMonster;
				CreatureState = ECreatureState.Move;
				return;
			}

			// Env �̹� ä�������� ����.
			if (Target.IsValid() == false)
			{
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Move;
				return;
			}

			ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, AttackDistance);
			return;
		}

		// 3. Camp �ֺ����� ���̱�
		if (HeroMoveState == EHeroMoveState.ReturnToCamp)
		{

			Vector3 destPos = HeroCampDest.position;
			if (FindPathAndMoveToCellPos(destPos, HERO_DEFAULT_MOVE_DEPTH) == EFindPathResult.Success)
				return;

			//���� ���� �˻�.
			BaseObject obj = Managers.Map.GetObject(destPos);
			if(obj.IsValid())
            {
				//���� �� �ڸ��� �����ϰ� �ִٸ�.
				if(obj == this)
                {
					HeroMoveState = EHeroMoveState.None;
					NeedArrange = false;
					return;
                }
				//�ٸ� ������ �����ִٸ�.
				Hero hero = obj as Hero;
				if(hero != null && hero.CreatureState == ECreatureState.Idle)
                {
					HeroMoveState = EHeroMoveState.None;
					NeedArrange = false;
					return;
                }
            }
		}

		// 4. ��Ÿ (������ ���� ��)
		if(LerpCellPosCompleted)
			CreatureState = ECreatureState.Idle;
	}

	Queue<Vector3Int> _forcePath = new Queue<Vector3Int>();

	bool CheckHeroCampDistanceAndForcePath()
	{
		// �ʹ� �־ �� ����.
		Vector3 destPos = HeroCampDest.position;
		Vector3Int destCellPos = Managers.Map.World2Cell(destPos);
		if ((CellPos - destCellPos).magnitude <= 10)
			return false;

		if (Managers.Map.CanGo(destCellPos, ignoreObjects: true) == false)
			return false;

		List<Vector3Int> path = Managers.Map.FindPath(CellPos, destCellPos, 100);
		if (path.Count < 2)
			return false;

		HeroMoveState = EHeroMoveState.ForcePath;

		_forcePath.Clear();
		foreach (var p in path)
		{
			_forcePath.Enqueue(p);
		}
		_forcePath.Dequeue();

		return true;
	}

	void MoveByForcePath()
	{
		if (_forcePath.Count == 0)
		{
			HeroMoveState = EHeroMoveState.None;
			return;
		}

		Vector3Int cellPos = _forcePath.Peek();

		if (MoveToCellPos(cellPos, 2))
		{
			_forcePath.Dequeue();
			return;
		}

		// ���� ������ �����̶��.
		Hero hero = Managers.Map.GetObject(cellPos) as Hero;
		if (hero != null && hero.CreatureState == ECreatureState.Idle)
		{
			HeroMoveState = EHeroMoveState.None;
			return;
		}
	}

	protected override void UpdateSkill()
	{
		//�ݵ�� base�� UpdateSkill()�� �����ؾ��� ��ų�� ���ӽð���ŭ ���������� ������ �� �� ����;
		base.UpdateSkill();


		if (HeroMoveState == EHeroMoveState.ForceMove)
		{
			CreatureState = ECreatureState.Move;
			return;
		}

		if (Target.IsValid() == false)
		{
			CreatureState = ECreatureState.Move;
			return;
		}
	}

	protected override void UpdateDead()
	{

	}
	#endregion

	private void HandleOnJoystickStateChanged(EJoystickState joystickState)
	{
		switch (joystickState)
		{
			case Define.EJoystickState.PointerDown:
				HeroMoveState = EHeroMoveState.ForceMove;
				break;
			case Define.EJoystickState.Drag:
				HeroMoveState = EHeroMoveState.ForceMove;
				break;
			case Define.EJoystickState.PointerUp:
				HeroMoveState = EHeroMoveState.None;
				break;
			default:
				break;
		}
	}

	public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
	{
		base.OnAnimEventHandler(trackEntry, e);
	}
}
