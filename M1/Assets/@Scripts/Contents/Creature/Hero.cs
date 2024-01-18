using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Hero : Creature
{

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = ECreatureType.Hero;
		CreatureState = ECreatureState.Idle;
		Speed = 5.0f;

		//Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
		//Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;
		//Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
		//Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

		return true;
	}
}
