using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Creature : BaseObject
{
	public Data.CreatureData CreatureData { get; private set; }
    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;
	public SkillBook Skills { get; protected set; }


	#region Stats
	public float Hp { get; set; }
	public float MaxHp { get; set; }
	public float MaxHpBonusRate { get; set; }
	public float HealBonusRate { get; set; }
	public float HpRegen { get; set; }
	public float Atk { get; set; }
	public float AttackRate { get; set; }
	public float Def { get; set; }
	public float DefRate { get; set; }
	public float CriRate { get; set; }
	public float CriDamage { get; set; }
	public float DamageReduction { get; set; }
	public float MoveSpeedRate { get; set; }
	public float MoveSpeed { get; set; }
	#endregion

	protected ECreatureState _creatureState = ECreatureState.None;
	public virtual ECreatureState CreatureState
	{
		get { return _creatureState; }
		set
		{
			if (_creatureState != value)
			{
				_creatureState = value;
				UpdateAnimation();
			}
		}
	}
	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		ObjectType = EObjectType.Creature;
		Skills = gameObject.GetOrAddComponent<SkillBook>();


		return true;
	}

	public virtual void SetInfo(int templateID)
    {
		DataTemplateID = templateID;

		CreatureData = Managers.Data.CreatureDic[templateID];
		gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";        //디테일을 위하여 이름을 붙여줌

		//Collider, 데이터시트로 관리하기로 함.
		Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffstY);
		Collider.radius = CreatureData.ColliderRadius;

		//RigidBody
		RigidBody.mass = CreatureData.Mass;

		//Spine
		SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(CreatureData.SkeletonDataID);
		SkeletonAnim.Initialize(true);

		// Register AnimEvent
		if(SkeletonAnim.AnimationState != null)
        {
			SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
			SkeletonAnim.AnimationState.Event += OnAnimEventHandler;
		}

		// Spine SkeletonAnimation은 SpriteRenderer를 사용하지 않고 MeshRenderer를 사용함
		// 그렇기 때문에 2D Sort Axis가 안 먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을 같이 계산함.
		SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
		sg.sortingOrder = SortingLayers.CREATURE;

		// Skills
		// CreatureData.SkillIdList

		//stat
		MaxHp = CreatureData.MaxHp;
		Hp = CreatureData.MaxHp;
		Atk = CreatureData.Atk;
		MoveSpeed = CreatureData.MoveSpeed;

		//State
		CreatureState = ECreatureState.Idle;
	}

	protected override void UpdateAnimation()
	{
		switch (CreatureState)
		{
			case ECreatureState.Idle:
				PlayAnimation(0, AnimName.IDLE, true);
				break;
			case ECreatureState.Skill:
				PlayAnimation(0, AnimName.ATTACK_A, true);
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

	public void ChangeColliderSize(EColliderSize size = EColliderSize.Normal)
    {
		switch(size)
        {
			case EColliderSize.Small:
				Collider.radius = CreatureData.ColliderRadius * 0.8f;
				break;
			case EColliderSize.Normal:
				Collider.radius = CreatureData.ColliderRadius;
				break;
			case EColliderSize.Big:
				Collider.radius = CreatureData.ColliderRadius * 1.2f;
				break;
		}
    }

	#region AI

	public float UpdateAITick { get; protected set; }

	protected IEnumerator CoUpdateAI()
    {
		while(true)
        {
			switch (CreatureState)
			{
				case ECreatureState.Idle:
					UpdateIdle();
					break;
				case ECreatureState.Move:
					UpdateMove();
					break;
				case ECreatureState.Skill:
					UpdateSkill();
					break;
				case ECreatureState.Dead:
					UpdateDead();
					break;
			}

			if (UpdateAITick > 0)
				yield return new WaitForSeconds(UpdateAITick);
			else
				yield return null;
		}
    }

	protected virtual void UpdateIdle() { }
	protected virtual void UpdateMove() { }
	protected virtual void UpdateSkill() { }
	protected virtual void UpdateDead() { }

    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker)
	{
		//크리처가 뎀지 입는 함수
		base.OnDamaged(attacker);

		//공격자가 살아있는지 체크
		if (attacker.IsValid() == false)
			return;     //죽었다면 리턴

		//데미지를 주는건 크리처뿐임. 채집물은 공격력 X
		Creature creature = attacker as Creature;
		if (creature == null)
			return;

		float finalDamage = creature.Atk;
		Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp);
		//Clamp 함수 사용하는 이유는 2,3번째 인자가 범위를 지정해줄 수 있음.
		//0 ~ MaxHp사이로만 반환됨

		if(Hp <= 0)
        {
			OnDead(attacker);
			CreatureState = ECreatureState.Dead;
        }
	}

	public override void OnDead(BaseObject attacker)
	{
		//크리처가 죽을때 실행되는 함수
		base.OnDead(attacker);


	}

    #endregion



    #region Wait
    protected Coroutine _coWait;
	//_coWait == null이면 끝난 상태라고 인지, != null이면 기다려야 하는 상태라고 인지
	//_coWait이 널인지 아닌지만 판단하면됨

	protected void StartWait(float seconds)
    {
		CancelWait();
		_coWait = StartCoroutine(CoWait(seconds));
    }

	IEnumerator CoWait(float seconds)
    {
		yield return new WaitForSeconds(seconds);
		_coWait = null;
    }

	//취소하는 함수도 생성

	protected void CancelWait()
    {
		if (_coWait != null)
			StopCoroutine(_coWait);
		_coWait = null;
    }
    #endregion
}
