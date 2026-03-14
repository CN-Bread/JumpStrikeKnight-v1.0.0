using Godot;
using System;
using System.Collections.Generic;

public partial class Player : PlayerBase
{
	public bool armorIsZero = false;
	private bool isFalling = false;

	#region 武器相关声明
	private int damageRate;
	public Sword sword;
	public Shield shield;
	public Bow bow;
	#endregion
	public override void _Ready()
    {
		base._Ready();
		Skin(playerId);
		damageRate = 1;
		sword = weaponMgr.weapons[0] as Sword;
		shield = weaponMgr.weapons[1] as Shield;
		bow = weaponMgr.weapons[2] as Bow;
		//信号连接
		for(int i = 0; i < weaponMgr.weapons.Count; i++)
        {
            weaponMgr.weapons[i].CanFlip += OnCanFlip;
            weaponMgr.weapons[i].CanNotFlip += OnCanNotFlip;
        }
		InitCollisionLayer(playerId);
    }
	public override void _PhysicsProcess(double delta)
	{
		if (canControl)
		{
			MoveFunc((float)delta);
			CombatFunc();
			//落地音效逻辑
			if (isFalling && IsOnFloor())
			{
				isFalling = false;
				fallOnGround.Play();
			}
			if (!IsOnFloor())
			{
				isFalling = true;
			}
			//穿越平台逻辑
			if (Input.IsActionPressed(downKey) && isCanJump)
			{
				isCanJump = false;
				canJumpTimer.Stop();
				GlobalPosition = new Vector2(GlobalPosition.X,GlobalPosition.Y + 0.2f);
			}

		}
	}

	void CombatFunc()
	{
		if (Input.IsActionJustPressed(switchKey1))
		{
			weaponMgr?.TrySwitchWeapon(0);
		}
		else if (Input.IsActionJustPressed(switchKey2))
		{
			weaponMgr?.TrySwitchWeapon(1);
		}
		else if (Input.IsActionJustPressed(switchKey3))
		{
			weaponMgr?.TrySwitchWeapon(2);
		}

		if (Input.IsActionPressed(key1))
		{
			weaponMgr?.Try1();
		}
		else if (Input.IsActionPressed(key2))
		{
			weaponMgr?.Try2();
		}
		else if (Input.IsActionPressed(key3))
		{
			weaponMgr?.TrySPC();
		}
		else if (Input.IsActionJustPressed(reloadKey) && weaponMgr.currentWeapon is Bow)
		{
			weaponMgr.currentWeapon.EnterBowFlow(WeaponBase.ATKType.Reload);
		}
	}
	public int GetDamageRate(WeaponBase weaponType , WeaponBase.ATKType ATKType)
	{
		if(weaponType == sword)
		{
			if(ATKType == WeaponBase.ATKType.T1 && Velocity.Y > 10)
			{
				return 2;
			}
			if(ATKType == WeaponBase.ATKType.T2 && Velocity.Y < -10)
			{
				return 2;
			}
		}
		if(weaponType == shield)
		{
			if(ATKType == WeaponBase.ATKType.T1)
			{
				if(isRight && Velocity.X > 500)
				{
					return 3; 
				}
				if(!isRight && Velocity.X < -500)
				{
					return 3;
				}
				return 0;
			}
			if(ATKType == WeaponBase.ATKType.T2)
			{
				return 1;
			}
		}

		return 1;
	}
	public void Skin(int playerId)
	{
		if(playerId == 0)
		{
			charaTextrue.Animation = "red";
		}
		else if(playerId == 1)
		{
			charaTextrue.Animation = "blue";
		}
	}
	public void InitCollisionLayer(int playerId)
	{
		if(playerId == 0)
		{
			CollisionLayer = CollisionMask = 1;
			sword.ATKArea.CollisionLayer = shield.DEFArea.CollisionLayer = bow.blockingArea.CollisionLayer = 1;
			aimArea.CollisionMask = sword.ATKArea.CollisionMask = shield.DEFArea.CollisionMask = bow.blockingArea.CollisionMask = 2;
		}
		if(playerId == 1)
		{
			CollisionLayer = CollisionMask = 2;
			sword.ATKArea.CollisionLayer = shield.DEFArea.CollisionLayer = bow.blockingArea.CollisionLayer = 2;
			aimArea.CollisionMask = sword.ATKArea.CollisionMask = shield.DEFArea.CollisionMask = bow.blockingArea.CollisionMask = 1;
		}
	}

	#region 信号相关函数
    void OnCanFlip()
    {
        nowCanFlip = true;
    }
    void OnCanNotFlip()
    {
        nowCanFlip = false;
    }
    #endregion
}