using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class BaseObject : InitBase
{
	public EObjectType ObjectType { get; protected set; } = EObjectType.None;
	public CircleCollider2D Collider { get; private set; }
	public SkeletonAnimation SkeletonAnim { get; private set; }
	public Rigidbody2D RigidBody { get; private set; }

	public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }
	public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }		//캐릭터의 발위치가 아닌 몸통 중앙부분을 포지션으로 잡음

	public int DataTemplateID { get; set; }

	bool _lookLeft = true;
	public bool LookLeft
	{
		get { return _lookLeft; }
		set
		{
			_lookLeft = value;
			Flip(!value);
		}
	}


	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
		SkeletonAnim = GetComponent<SkeletonAnimation>();
		RigidBody = GetComponent<Rigidbody2D>();

		return true;
	}

	public void TranslateEx(Vector3 dir)
    {
		transform.Translate(dir);

		if (dir.x < 0)
			LookLeft = true;
		else if (dir.x > 0)
			LookLeft = false;
	}

	#region Spine

	protected virtual void SetSpineAnimation(string dataLabel, int sortingOrder)
    {
		if (SkeletonAnim == null)
			return;

		SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(dataLabel);
		SkeletonAnim.Initialize(true);

		//Spine SkeletonAnimation은 Sprite Renderer를 사용하지 않고 Mesh Renderer을 사용함
		//그렇기 때문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을 같이 계산함

		SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
		sg.sortingOrder = sortingOrder;
    }
	protected virtual void UpdateAnimation()
	{
	}

	public void SetRigidBodyVelocity(Vector2 velocity)
    {
		if (RigidBody == null)
			return;

		RigidBody.velocity = velocity;

		if (velocity.x < 0)
			LookLeft = true;
		if (velocity.x > 0)
			LookLeft = false; 

	}


	public void PlayAnimation(int trackIndex, string AnimName, bool loop)
	{
		if (SkeletonAnim == null)
			return;

		SkeletonAnim.AnimationState.SetAnimation(trackIndex, AnimName, loop);
	}

	public void AddAnimation(int trackIndex, string AnimName, bool loop, float delay)
	{
		if (SkeletonAnim == null)
			return;

		SkeletonAnim.AnimationState.AddAnimation(trackIndex, AnimName, loop, delay);
	}

	public void Flip(bool flag)
	{
		if (SkeletonAnim == null)
			return;

		SkeletonAnim.Skeleton.ScaleX = flag ? -1 : 1;
	}
	#endregion

}
