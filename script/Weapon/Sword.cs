using Godot;
using System;
using System.Diagnostics.CodeAnalysis;

public partial class Sword : WeaponBase
{
	#region 声明
	Player itself;
	[Export] public Area2D ATKArea;
	private int power = 1;
	public bool isSwordBlocking;
	public bool isBlockingSuccess;
	[ExportGroup("音效节点接入")]
	[Export] public AudioStreamPlayer2D swing;
	[Export] public AudioStreamPlayer2D ATKOnArmor;
	[Export] public AudioStreamPlayer2D breakArmor;
	[Export] public AudioStreamPlayer2D bloodSplash;
	[Export] public AudioStreamPlayer2D swordBlockSuccess;
	[Export] public AudioStreamPlayer2D ATKOnShield;
	[Export] public AudioStreamPlayer2D shieldBlockSuccess;
	[Export] public AudioStreamPlayer2D bowBlockSuccess;
	[Export] public AudioStreamPlayer2D killBySword;
	#endregion
	public override void _Ready()
	{
		//基础初始化
		base._Ready();
		itself = GetParent().GetParent() as Player;
		isSwordBlocking = false;
		isBlockingSuccess = false;
		ATKArea.Monitorable = false;
		ATKArea.Monitoring = false;
		//信号连接
		ATKArea.BodyEntered += OnBodyEntered;
	}

	#region 武器具体Action
	public void TweenAnimationPlayer(float degree, float seconds, bool unlockFlip = false)
	{
		//使用Tween动画
		Tween tween = CreateTween();
		tween.TweenProperty(this, "rotation", Mathf.DegToRad(degree), seconds)
			.SetEase(Tween.EaseType.In);
		if (unlockFlip)
		{
			ItCanFlip();
		}
		tween.Finished += tween.Kill;
	}

	public override void PrepareAction(ATKType type)
	{
		ItCanNotFlip();
		canSwitchWeapon = false;
		currentStates = States.CanATK;
		switch (type)
		{
			case ATKType.T1:
				TweenAnimationPlayer(GetPrepareDegree(type), GetPrepareDuration(type));
				break;

			case ATKType.T2:
				TweenAnimationPlayer(GetPrepareDegree(type), GetPrepareDuration(type));
				break;

			case ATKType.SPC:
				TweenAnimationPlayer(GetPrepareDegree(type), GetPrepareDuration(type));
				break;
		}
	}
	public override void ATKingAction(ATKType type)
	{
		ATKArea.Monitoring = true;
		ATKArea.Monitorable = true;
		switch (type)
		{
			case ATKType.T1:
				TweenAnimationPlayer(GetATKDegree(type), GetATKDuration(type));
				swing.Play();
				break;
			case ATKType.T2:
				TweenAnimationPlayer(GetATKDegree(type), GetATKDuration(type));
				swing.Play();
				break;
			case ATKType.SPC:
				TweenAnimationPlayer(GetATKDegree(type), GetATKDuration(type));
				isSwordBlocking = true;
				break;
		}
	}
	public override void RecoveryAction(ATKType type)
	{
		ATKArea.Monitoring = false;
		ATKArea.Monitorable = false;
		canSwitchWeapon = true;
		switch (type)
		{
			case ATKType.T1:
			case ATKType.T2:
				TweenAnimationPlayer(GetRecoveryDegree(), GetRecoveryDuration(type), true);
				break;

			case ATKType.SPC:
				TweenAnimationPlayer(GetRecoveryDegree(), GetRecoveryDuration(type), true);
				isSwordBlocking = false;
				break;
		}
	}
	public override void CoolDownAction(ATKType type)
	{

	}

	#endregion

	#region 武器动画参数
	protected override float GetPrepareDuration(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return 0.05f;
		}
		else if (type == ATKType.T2)
		{
			return 0.1f;
		}
		else if (type == ATKType.SPC)
		{
			return 0.05f;
		}

		return 0;
	}
	protected override float GetATKDuration(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return 0.1f;
		}
		else if (type == ATKType.T2)
		{
			return 0.1f;
		}
		else if (type == ATKType.SPC)
		{
			return 0.15f;
		}

		return 0;
	}
	protected override float GetRecoveryDuration(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return 0.15f;
		}
		else if (type == ATKType.T2)
		{
			return 0.05f;
		}
		else if (type == ATKType.SPC)
		{
			return 0.1f;
		}

		return 0;
	}
	protected override float GetCoolDownDuration(ATKType type)
	{
		if(type is ATKType.T1 || type is ATKType.T2)
		{
			return 0.1f;
		}
		if (type is ATKType.SPC)
		{
			if (isBlockingSuccess)
			{
				isBlockingSuccess = false;
				return 0.1f;
			}
			if (!isBlockingSuccess)
			{
				return 0.5f;
			}
		}
		return 0.1f;
	}
	protected override float GetCanNotATKDuration()
	{
		return 0f;
	}

	private float GetPrepareDegree(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return -45;
		}
		else if (type == ATKType.T2)
		{
			return 135;
		}
		else if (type == ATKType.SPC)
		{
			return -45;
		}

		return 0;
	}
	private float GetATKDegree(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return 135;
		}
		else if (type == ATKType.T2)
		{
			return -45;
		}
		else if (type == ATKType.SPC)
		{
			return -45;
		}

		return 0;
	}
	private float GetRecoveryDegree()
	{
		return 0;
	}
	#endregion

	#region 攻击信号处理
	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			CallDeferred(nameof(DisableMonitoring));
			Vector2 dirOfPlayer = player.GlobalPosition - itself.GlobalPosition;
			dirOfPlayer.Y = player.GlobalPosition.Y > itself.GlobalPosition.Y ? 1 : -1;
			dirOfPlayer.X = itself.isRight ? 1 : -1;

			Vector2 hitAwayVec = player.Velocity;
			Vector2 itselfVec = itself.Velocity;

			// 记录攻击前的速度

			#region 剑攻击剑
			if (player.weaponMgr.currentWeapon is Sword)
			{
				if (!player.sword.isSwordBlocking)
				{
					//下面是击飞逻辑
					player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
					player.beingHitAwayTimer.Start();
					player.isBeingHitAway = true; // 明确标记为击退状态
					if (currentType is ATKType.T1)
					{
						if (itself.Velocity.Y > 0)
						{
							hitAwayVec.Y += dirOfPlayer.Y * 400;
							hitAwayVec.X += dirOfPlayer.X * 1200;
							itselfVec.Y = dirOfPlayer.Y * -500;
							//处理声音
							ATKOnArmor.VolumeDb = 25f;
						}
						else
						{
							hitAwayVec.X += dirOfPlayer.X * 1000;
							//处理声音
							ATKOnArmor.VolumeDb = 7.15f;
						}
					}
					if (currentType is ATKType.T2)
					{
						if (itself.Velocity.Y < 0)
						{
							hitAwayVec.Y -= dirOfPlayer.Y * 500;
							hitAwayVec.X += dirOfPlayer.X * 1200;
							itselfVec.X = 0;
							//处理声音
							ATKOnArmor.VolumeDb = 25f;
						}
						else
						{
							hitAwayVec.X += dirOfPlayer.X * 1000;
							//处理声音
							ATKOnArmor.VolumeDb = 7.15f;
						}
					}
					if (currentType != ATKType.SPC)
					{
						player.BeingHit(power * itself.GetDamageRate(this, currentType));
						player.weaponMgr.SwordSlash((GetParent() as WeaponManager).isFacingRight);
						//处理声音
						if (player.HP <= 0)
						{
							player.killBySword = true;
							player.Die();
							killBySword.Play();
						}
						if (player.armor > 0)
						{
							ATKOnArmor.Play();
							player.armorIsZero = false;
						}
						if (!player.armorIsZero && player.armor <= 0)
						{
							breakArmor.Play();
							player.armorIsZero = true;
						}
						if (player.armorIsZero && player.armor <= 0 && !player.killBySword)
						{
							bloodSplash.Play();
						}
					}
				}
				if (player.sword.isSwordBlocking && (currentType is ATKType.T1 || currentType is ATKType.T2))
				{
					swordBlockSuccess.Play();
					player.AddArmor(2);
					player.sword.isBlockingSuccess = true;
					itselfVec.X = itselfVec.Y = 0;
				}
				player.weaponMgr.HitStop(0.015f, player.sword.isBlockingSuccess);
			}
			#endregion
			#region 剑攻击盾
			if (player.shield.isRaisingShield)
			{
				Shield shield = player.shield;
				//判定是否打中正面
				if ((dirOfPlayer.X < 0 && player.isRight) || (dirOfPlayer.X > 0 && !player.isRight))
				{
					if (!shield.isSPCBlocking)
					{
						player.BeingHit(power * itself.GetDamageRate(this, currentType) / 2);
						if (power * itself.GetDamageRate(this, currentType) / 2 == 0)
						{
							player.weaponMgr.HitStop(0.015f, true);
						}
						else
						{
							player.weaponMgr.HitStop(0.015f);
						}
						player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
						player.beingHitAwayTimer.Start();
						player.isBeingHitAway = true; // 明确标记为击退状态
						if (currentType is ATKType.T1)
						{
							if (itself.Velocity.Y > 0)
							{
								hitAwayVec.Y += dirOfPlayer.Y * 200;
								hitAwayVec.X += dirOfPlayer.X * 500;
								itselfVec.Y = dirOfPlayer.Y * -600;
								itselfVec.X = 0;
								//处理声音
								ATKOnShield.VolumeDb = -5;
							}
							else
							{
								hitAwayVec.X += dirOfPlayer.X * 200;
								//处理声音
								ATKOnShield.VolumeDb = -10;
							}
						}
						if (currentType is ATKType.T2)
						{
							if (itself.Velocity.Y < 0)
							{
								hitAwayVec.Y -= dirOfPlayer.Y * 300;
								hitAwayVec.X += dirOfPlayer.X * 500;
								itselfVec.Y += 500;
								itselfVec.X = 0;
								//处理声音
								ATKOnShield.VolumeDb = -5;
							}
							else
							{
								hitAwayVec.X += dirOfPlayer.X * 200;
								//处理声音
								ATKOnShield.VolumeDb = -10;
							}
						}
						//处理声音
						if (currentType != ATKType.SPC)
						{
							if (player.HP <= 0)
							{
								player.killBySword = true;
								player.Die();
								killBySword.Play();
							}
							if (player.armor > 0)
							{
								ATKOnShield.Play();
								player.armorIsZero = false;
							}
							if (!player.armorIsZero && player.armor <= 0)
							{
								breakArmor.Play();
								player.armorIsZero = true;
							}
							if (player.armorIsZero && player.armor <= 0 && !player.killBySword)
							{
								if (power * itself.GetDamageRate(this, currentType) / 2 > 0)
								{
									bloodSplash.Play();
								}
								else
								{
									ATKOnShield.Play();
								}
							}
						}
					}
					else
					{
						if (currentType is ATKType.T1 || currentType is ATKType.T2)
						{
							itselfVec.X -= dirOfPlayer.X * 800;
							itselfVec.Y -= dirOfPlayer.X * 100;
							if (itself.armor > 0)
							{
								itself.armor -= 2;
								if(itself.armor <= 0)
								{
									itself.armor = 0;
								}
							}
							player.AddArmor(1);
							shield.isSPCBlocking = true;
							itself.weaponMgr.ShieldKick(player.weaponMgr.isFacingRight);
							//处理声音
							shieldBlockSuccess.Play();
						}
					}
				}
				else
				{
					if (currentType != ATKType.SPC)
					{
						player.BeingHit(power * itself.GetDamageRate(this, currentType));
						player.weaponMgr.HitStop(0.015f);
					}
					player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
					player.beingHitAwayTimer.Start();
					player.isBeingHitAway = true; // 明确标记为击退状态
					hitAwayVec.X += dirOfPlayer.X * 400;
					hitAwayVec.Y += itselfVec.Y * 0.5f;
					itselfVec.Y = 0;
					//处理声音
					if (player.HP <= 0)
					{
						player.killBySword = true;
						player.Die();
						killBySword.Play();
					}
					if (player.armor > 0)
					{
						ATKOnArmor.Play();
						player.armorIsZero = false;
					}
					if (!player.armorIsZero && player.armor <= 0)
					{
						breakArmor.Play();
						player.armorIsZero = true;
					}
					if (player.armorIsZero && player.armor <= 0 && !player.killBySword)
					{
						bloodSplash.Play();
					}
				}

			}
			#endregion
			#region 剑攻击弓
			if (player.weaponMgr.currentWeapon is Bow)
			{
				Bow bow = player.bow;
				if (((dirOfPlayer.X < 0 && player.isRight) || (dirOfPlayer.X > 0 && !player.isRight)) && bow.isBlocking)
				{
					bowBlockSuccess.Play();
					bow.BlockSuc();
					if (currentType is ATKType.T1)
					{
						itselfVec.X -= 400 * dirOfPlayer.X;
						hitAwayVec.X += 200 * dirOfPlayer.X;
						player.beingHitAwayTimer.WaitTime = 0.2f;
						player.beingHitAwayTimer.Start();
						player.isBeingHitAway = true; // 明确标记为击退状态
						if (itself.Velocity.Y > 0)
						{
							hitAwayVec.Y += dirOfPlayer.Y * 100;
							hitAwayVec.X += dirOfPlayer.X * 300;
							itselfVec.Y = dirOfPlayer.Y * -500;
							itselfVec.X = 0;
						}
					}
					if (currentType is ATKType.T2)
					{
						itselfVec.X -= 400 * dirOfPlayer.X;
						hitAwayVec.X += 200 * dirOfPlayer.X;
						player.beingHitAwayTimer.WaitTime = 0.2f;
						player.beingHitAwayTimer.Start();
						player.isBeingHitAway = true; // 明确标记为击退状态
						if (itself.Velocity.Y < 0)
						{
							hitAwayVec.Y -= dirOfPlayer.Y * 100;
							hitAwayVec.X += dirOfPlayer.X * 300;
							itselfVec.X = 0;
						}
					}
				}
				else
				{
					if (currentType is ATKType.T1)
					{
						if (player.armor > 0)
						{
							ATKOnArmor.Play();
						}
						player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
						player.beingHitAwayTimer.Start();
						player.isBeingHitAway = true; // 明确标记为击退状态
						hitAwayVec.X += 600 * dirOfPlayer.X;
						//处理声音
						ATKOnArmor.VolumeDb = 7.15f;
						if (itself.Velocity.Y > 0)
						{
							hitAwayVec.Y += dirOfPlayer.Y * 400;
							hitAwayVec.X += dirOfPlayer.X * 300;
							itselfVec.Y = dirOfPlayer.Y * 300;
							itselfVec.X = 0;
							//处理声音
							ATKOnArmor.VolumeDb = 25f;
						}
					}
					if (currentType is ATKType.T2)
					{
						player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
						player.beingHitAwayTimer.Start();
						player.isBeingHitAway = true; // 明确标记为击退状态
						hitAwayVec.X += 600 * dirOfPlayer.X;
						//处理声音
						ATKOnArmor.VolumeDb = 7.15f;
						if (itself.Velocity.Y < 0)
						{
							hitAwayVec.Y -= dirOfPlayer.Y * 400;
							hitAwayVec.X += dirOfPlayer.X * 300;
							itselfVec.Y = dirOfPlayer.Y * 300;
							itselfVec.X = 0;
							//处理声音
							ATKOnArmor.VolumeDb = 25f;
						}
					}
					if (currentType != ATKType.SPC)
					{
						player.BeingHit(power * itself.GetDamageRate(this, currentType));
						player.weaponMgr.HitStop(0.015f);
						//处理声音
						if (player.HP <= 0)
						{
							player.killBySword = true;
							player.Die();
							killBySword.Play();
						}
						if (player.armor > 0)
						{
							ATKOnArmor.Play();
							player.armorIsZero = false;
						}
						if (!player.armorIsZero && player.armor <= 0)
						{
							breakArmor.Play();
							player.armorIsZero = true;
						}
						if (player.armorIsZero && player.armor <= 0 && !player.killBySword)
						{
							bloodSplash.Play();
						}
					}
				}
			}

			#endregion

			// 记录攻击后的速度
			itself.Velocity = itselfVec;
			player.Velocity = hitAwayVec;
			player.hitAwayVelocity = hitAwayVec;
		}
	}
	public void OnAreaEntered(Node2D area)
	{
		if (area is Area2D && area.GetParent() is Arrow)
		{
			if (currentType == ATKType.SPC)
			{
				itself.AddArmor(2);
			}
		}
	}
	private void DisableMonitoring()
	{
		ATKArea.Monitoring = false;
	}
	#endregion
}