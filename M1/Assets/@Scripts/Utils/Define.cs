using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Define
{
	public enum EScene
	{
		Unknown,
		TitleScene,
		GameScene,
	}

	public enum EUIEvent
	{
		Click,
		PointerDown,
		PointerUp,
		Drag,
	}

	public enum EJoystickState
	{
		PointerDown,
		PointerUp,
		Drag,
	}

	public enum ESound
	{
		Bgm,
		Effect,
		Max,
	}

	public enum EObjectType
	{
		None,
		Creature,
		Projectile,
		Env,
	}

	public enum ECreatureType
	{
		None,
		Hero,
		Monster,
		Npc,
	}

	public enum ECreatureState
	{
		None,
		Idle,
		Move,
		Skill,
		Dead
	}

	public const int CAMERA_PROJECTION_SIZE = 12;

	public const int HERO_WIZARD_ID = 20100;
	public const int HERO_KNIGHT_ID = 201001;

	public const int MONSTER_SLIME_ID = 202001;
	
}

public static class AnimName
{
	public const string IDLE = "idle";
	public const string ATTACK_A = "attack_a";
	public const string ATTACK_B = "attack_b";
	public const string MOVE = "move";
	public const string DEAD = "dead";
}

public static class SortingLayers
{
	public const int SPELL_INDICATOR = 200;
	public const int CREATURE = 300;
	public const int ENV = 300;
	public const int PROJECTILE = 310;
	public const int SKILL_EFFECT = 310;
	public const int DAMAGE_FONT = 410;
}