using Godot;
using System;

public partial class Shield : WeaponBase
{
	#region 声明参数
	[Export] public Area2D DEFArea;
	[Export] private AnimatedSprite2D texture;
	public bool isRaisingShield;
	private int power = 1;
	public bool isSPCBlocking;
	public bool isSPCSuccess;
	private bool isT1Success;
	private bool isT2Success;
	public Player itself;
	[ExportGroup("音效节点接入")]
	[Export] public AudioStreamPlayer2D T2Swing;
	[Export] public AudioStreamPlayer2D ATKOnArmor;
	[Export] public AudioStreamPlayer2D ATKOnArmorHeavy;
	[Export] public AudioStreamPlayer2D breakArmor;
	[Export] public AudioStreamPlayer2D shiedlHit;
	[Export] public AudioStreamPlayer2D ATKOnShield;
	[Export] public AudioStreamPlayer2D swordBlockSuccess;
	[Export] public AudioStreamPlayer2D shieldBlockSuccess;
	[Export] public AudioStreamPlayer2D bowBlockSuccess;
	[Export] public AudioStreamPlayer2D killByShield;
	#endregion

	public override void _Ready()
	{
		itself = GetParent().GetParent() as Player;
		base._Ready();
		DEFArea.Monitoring = false;
		DEFArea.Monitorable = true;
		isRaisingShield = false;
		isSPCBlocking = false;
		isSPCSuccess = false;
		isT1Success = false;
		isT2Success = false;
		//信号连接
		DEFArea.BodyEntered += OnBodyEntered;
	}

	#region 武器具体Action
	public void TweenAnimationPlayer(Vector2 pos, float skew, float seconds, bool unlockFlip = false)
	{
		Tween tween = CreateTween().SetParallel(true);
		tween.TweenProperty(this, "position", pos, seconds)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(this, "skew", Mathf.DegToRad(skew), seconds)
			.SetTrans(Tween.TransitionType.Sine);
		if (unlockFlip)
		{
			tween.Finished += ItCanFlip;
		}
		tween.Finished += () =>
		{
			tween.Kill();
		};

	}
	public override void PrepareAction(ATKType type)
	{
		ItCanNotFlip();
		canSwitchWeapon = false;
		switch (type)
		{
			case ATKType.T1:
				DEFArea.Monitoring = true;
				TweenAnimationPlayer(GetPreparePos(type), GetPrepareSkew(type), GetPrepareDuration(type));
				break;
			case ATKType.T2:
				TweenAnimationPlayer(GetPreparePos(type), GetPrepareSkew(type), GetPrepareDuration(type));
				break;
			case ATKType.SPC:
				TweenAnimationPlayer(GetPreparePos(type), GetPrepareSkew(type), GetPrepareDuration(type));
				break;
		}
	}
	public override void ATKingAction(ATKType type)
	{
		switch (type)
		{
			case ATKType.T1:
				TweenAnimationPlayer(GetDEFPos(type), GetDEFSkew(type), GetATKDuration(type));
				break;
			case ATKType.T2:
				DEFArea.Monitoring = true;
				T2Swing.Play();
				//让自身后退
				itself.beingHitAwayTimer.WaitTime = 0.1f;
				itself.beingHitAwayTimer.Start();
				itself.isBeingHitAway = true;
				if (itself.isRight)
				{
					itself.Velocity = new Vector2(-1500, itself.Velocity.Y);
					itself.hitAwayVelocity = new Vector2(-1500, itself.Velocity.Y);
				}
				else
				{
					itself.Velocity = new Vector2(1500, itself.Velocity.Y);
					itself.hitAwayVelocity = new Vector2(1500, itself.Velocity.Y);
				}
				TweenAnimationPlayer(GetDEFPos(type), GetDEFSkew(type), GetATKDuration(type));
				break;
			case ATKType.SPC:
				DEFArea.Monitoring = true;
				TweenAnimationPlayer(GetDEFPos(type), GetDEFSkew(type), GetATKDuration(type));
				isSPCBlocking = true;
				break;
		}
	}
	public override void RecoveryAction(ATKType type)
	{
		DEFArea.Monitoring = false;
		switch (type)
		{
			case ATKType.T1:
				TweenAnimationPlayer(GetRecoveryPos(), GetRecoverySkew(), GetRecoveryDuration(type), true);
				break;
			case ATKType.T2:
				TweenAnimationPlayer(GetRecoveryPos(), GetRecoverySkew(), GetRecoveryDuration(type), true);
				break;
			case ATKType.SPC:
				TweenAnimationPlayer(GetRecoveryPos(), GetRecoverySkew(), GetRecoveryDuration(type), true);
				isSPCBlocking = false;
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
			return 0.08f;
		}

		return 0;
	}
	protected override float GetATKDuration(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return 0.2f;
		}
		else if (type == ATKType.T2)
		{
			return 0.2f;
		}
		else if (type == ATKType.SPC)
		{
			return 0.2f;
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
			return 0.2f;
		}

		return 0;
	}
	protected override float GetCoolDownDuration(ATKType type)
	{
		if (type == ATKType.T1)
		{
			if (isT1Success)
			{
				isT1Success = false;
				return 0.5f;
			}
			return 1;
		}
		if (type == ATKType.T2)
		{
			if (isT2Success)
			{
				isT2Success = false;
				return 0.3f;
			}
			return 0.5f;
		}
		if (type == ATKType.SPC)
		{
			if (isSPCSuccess)
			{
				isSPCSuccess = false;
				return 0.2f;
			}
			return 0.8f;
		}
		return 0;
	}
	protected override float GetCanNotATKDuration()
	{
		return 0f;
	}

	private Vector2 GetPreparePos(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return new Vector2(-2, 0);
		}
		else if (type == ATKType.T2)
		{
			return new Vector2(-1, 0);
		}
		else if (type == ATKType.SPC)
		{
			return new Vector2(-1, 0);
		}

		return new Vector2(0, 0);
	}
	private Vector2 GetDEFPos(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return new Vector2(4, -3);
		}
		else if (type == ATKType.T2)
		{
			return new Vector2(-1, 0);
		}
		else if (type == ATKType.SPC)
		{
			return new Vector2(0, -3);
		}

		return new Vector2(0, 0);
	}
	private Vector2 GetRecoveryPos()
	{
		return new Vector2(0, 0);
	}
	private float GetPrepareSkew(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return -18;
		}
		else if (type == ATKType.T2)
		{
			return -18;
		}
		else if (type == ATKType.SPC)
		{
			return -18;
		}

		return 0;
	}
	private float GetDEFSkew(ATKType type)
	{
		if (type == ATKType.T1)
		{
			return 10;
		}
		else if (type == ATKType.T2)
		{
			return -18;
		}
		else if (type == ATKType.SPC)
		{
			return -10;
		}

		return 0;
	}
	private float GetRecoverySkew()
	{
		return 0;
	}
	#endregion

	#region 攻击信号处理
	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			CallDeferred(nameof(DisableMonitoring));
			Vector2 dirOfPlayer = player.GlobalPosition - itself.GlobalPosition;
			dirOfPlayer.Y = player.GlobalPosition.Y > itself.GlobalPosition.Y ? 1 : -1;
			dirOfPlayer.X = itself.isRight ? 1 : -1;

			Vector2 hitAwayVec = player.Velocity;
			Vector2 itselfVec = itself.Velocity;

			#region 盾攻击剑
			if (player.weaponMgr.currentWeapon is Sword)
			{
				if (!player.sword.isSwordBlocking)
				{
					//击飞逻辑
					player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
					player.beingHitAwayTimer.Start();
					player.isBeingHitAway = true;
					if (currentType is ATKType.T1)
					{
						player.BeingHit(power * itself.GetDamageRate(this, currentType));
						if (power * itself.GetDamageRate(this, currentType) > 0)
						{
							player.weaponMgr.ShieldKick((GetParent() as WeaponManager).isFacingRight);
						}
						isT1Success = true;
						if ((itself.Velocity.X > 520 && itself.isRight) || (itself.Velocity.X < -520 && !itself.isRight))
						{
							hitAwayVec.X += dirOfPlayer.X * 1600;
							itselfVec.X = 0;
						}
						else
						{
							hitAwayVec.X += dirOfPlayer.X * 800;
							itselfVec.X = 0;
						}
						//处理声音
						if (player.HP <= 0)
						{
							player.killByShield = true;
							player.Die();
							killByShield.Play();
						}
						if (power * itself.GetDamageRate(this, currentType) == 0)
						{
							ATKOnArmor.Play();
						}
						else
						{
							if (player.armor != 0)
							{
								ATKOnArmorHeavy.Play();
							}
							if (!player.armorIsZero && player.armor <= 0)
							{
								breakArmor.Play();
								player.armorIsZero = true;
							}
							if (player.armorIsZero && player.armor <= 0 && !player.killByShield)
							{
								shiedlHit.Play();
							}
						}
					}
					if (currentType is ATKType.T2)
					{
						isT2Success = true;
						hitAwayVec.Y -= 600;
						if (player.armor > 0)
						{
							player.armor--;
						}
						//处理声音
						if (player.armor != 0)
						{
							ATKOnArmor.Play();
						}
						if (!player.armorIsZero && player.armor <= 0)
						{
							breakArmor.Play();
							player.armorIsZero = true;
						}
						if (player.armorIsZero && player.armor <= 0)
						{
							shiedlHit.Play();
						}
					}
				}
				if (player.sword.isSwordBlocking && (currentType is ATKType.T1 || currentType is ATKType.T2))
				{
					player.AddArmor(2);
					player.sword.isBlockingSuccess = true;
					itselfVec.X = itselfVec.Y = 0;
					//处理声音
					swordBlockSuccess.Play();

				}
				player.weaponMgr.HitStop(0.02f, player.sword.isBlockingSuccess);
			}
			#endregion
			#region 盾攻击盾
			if (player.shield.isRaisingShield)
			{
				Shield shield = player.shield;
				//判定是否打中正面
				if ((player.GlobalPosition.X - itself.GlobalPosition.X < 0 && player.isRight) || (player.GlobalPosition.X - itself.GlobalPosition.X > 0 && !player.isRight))
				{
					if (!shield.isSPCBlocking)
					{
						player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
						player.beingHitAwayTimer.Start();
						player.isBeingHitAway = true;
						if (currentType is ATKType.T1)
						{
							player.BeingHit(power * itself.GetDamageRate(this, currentType) / 2);
							if (power * itself.GetDamageRate(this, currentType) / 2 == 0)
							{
								player.weaponMgr.HitStop(0.02f, true);
							}
							else
							{
								player.weaponMgr.ShieldKick((GetParent() as WeaponManager).isFacingRight);
							}
							isT1Success = true;
							if ((itself.Velocity.X > 520 && itself.isRight) || (itself.Velocity.X < -520 && !itself.isRight))
							{
								hitAwayVec.X += dirOfPlayer.X * 150;
								itselfVec.X = 0;
							}
							else
							{
								hitAwayVec.X += dirOfPlayer.X * 100;
								itselfVec.X = 0;
							}
							//处理声音
							if (power * itself.GetDamageRate(this, currentType) == 0)
							{
								ATKOnShield.Play();
							}
							else
							{
								if (player.HP <= 0)
								{
									player.killByShield = true;
								}
								if (player.HP <= 0)
								{
									player.killByShield = true;
									killByShield.Play();
								}
								if (player.armor != 0)
								{
									ATKOnArmorHeavy.Play();
								}
								if (!player.armorIsZero && player.armor <= 0)
								{
									breakArmor.Play();
									player.armorIsZero = true;
								}
								if (player.armorIsZero && player.armor <= 0 && !player.killByShield && !player.killByShield)
								{
									shiedlHit.Play();
								}
							}
						}
						if (currentType is ATKType.T2)
						{
							player.weaponMgr.HitStop(0.02f, true);
							if (itself.armor > 0)
							{
								itself.armor--;
							}
							isT2Success = true;
							hitAwayVec.X = hitAwayVec.Y = itselfVec.X = itselfVec.Y = 0;

							if (player.armor > 0 && player.armor < 3)
							{
								player.armor--;
							}
							//处理声音
							if (player.armor != 0)
							{
								ATKOnArmor.Play();
							}
							if (!player.armorIsZero && player.armor <= 0)
							{
								breakArmor.Play();
								player.armorIsZero = true;
							}
							if (player.armorIsZero && player.armor <= 0)
							{
								shiedlHit.Play();
							}
						}
					}
					if (shield.isSPCBlocking && (currentType == ATKType.T1 || currentType == ATKType.T2))
					{
						hitAwayVec.X = hitAwayVec.Y = itselfVec.X = itselfVec.Y = 0;
						if (itself.armor > 0)
						{
							itself.armor--;
						}
						player.AddArmor(1);
						shield.isSPCSuccess = true;
						itself.weaponMgr.ShieldKick(player.weaponMgr.isFacingRight);
						//处理声音
						shieldBlockSuccess.Play();
					}
				}
				//打中背面
				else
				{
					player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
					player.beingHitAwayTimer.Start();
					player.isBeingHitAway = true;
					if (currentType is ATKType.T1)
					{
						player.BeingHit(power * itself.GetDamageRate(this, currentType));
						player.weaponMgr.HitStop(0.02f);
						if (power * itself.GetDamageRate(this, currentType) > 0)
						{
							player.weaponMgr.ShieldKick((GetParent() as WeaponManager).isFacingRight);
						}
						isT1Success = true;
						if ((itself.Velocity.X > 520 && itself.isRight) || (itself.Velocity.X < -520 && !itself.isRight))
						{
							hitAwayVec.X += dirOfPlayer.X * 200;
							itselfVec.X = 0;
						}
						else
						{
							hitAwayVec.X += dirOfPlayer.X * 150;
							itselfVec.X = 0;
						}
						//处理声音
						if (power * itself.GetDamageRate(this, currentType) == 0)
						{
							ATKOnArmor.Play();
						}
						else
						{
							if (player.HP <= 0)
							{
								player.killByShield = true;
								killByShield.Play();
							}
							if (player.armor != 0)
							{
								ATKOnArmorHeavy.Play();
							}
							if (!player.armorIsZero && player.armor <= 0 && !player.killByShield)
							{
								breakArmor.Play();
								player.armorIsZero = true;
							}
						}
					}
					if (currentType is ATKType.T2)
					{
						isT2Success = true;
						if (player.armor > 0)
						{
							player.armor--;
						}
						itselfVec.X = itselfVec.Y = 0;
						hitAwayVec.X -= 200;
						//处理声音
						if (player.armor != 0)
						{
							ATKOnArmor.Play();
						}
						if (!player.armorIsZero && player.armor <= 0)
						{
							breakArmor.Play();
							player.armorIsZero = true;
						}
						player.weaponMgr.HitStop(0.02f);
					}
				}
			}
			#endregion
			#region 盾攻击弓
			if (player.weaponMgr.currentWeapon is Bow)
			{
				player.weaponMgr.HitStop(0.02f, player.bow.isBlockingSuccess);
				Bow bow = player.bow;
				if (((dirOfPlayer.X < 0 && player.isRight) || (dirOfPlayer.X > 0 && !player.isRight)) && bow.isBlocking)
				{
					bowBlockSuccess.Play();
					bow.BlockSuc();
					if (currentType is ATKType.T1)
					{
						itselfVec.X -= 100;
					}
					if (currentType is ATKType.T2)
					{
						if (itself.armor > 0)
						{
							itself.armor--;
						}
						itselfVec.X = itselfVec.Y = 0;
						hitAwayVec.X += dirOfPlayer.X * 150;
					}
				}
				else
				{
					player.beingHitAwayTimer.WaitTime = player.HITAWAYTIME;
					player.beingHitAwayTimer.Start();
					if (currentType is ATKType.T1)
					{
						player.BeingHit(power * itself.GetDamageRate(this, currentType));
						if (power * itself.GetDamageRate(this, currentType) > 0)
						{
							player.weaponMgr.ShieldKick((GetParent() as WeaponManager).isFacingRight);
						}
						isT1Success = true;
						if ((itself.Velocity.X > 520 && itself.isRight) || (itself.Velocity.X < -520 && !itself.isRight))
						{
							hitAwayVec.X += dirOfPlayer.X * 1650;
							itselfVec.X = 0;
						}
						else
						{
							hitAwayVec.X += dirOfPlayer.X * 850;
							itselfVec.X = 0;
						}
						//处理声音
						if (power * itself.GetDamageRate(this, currentType) == 0)
						{
							ATKOnArmor.Play();
						}
						else
						{
							if (player.HP <= 0)
							{
								player.killByShield = true;
								player.Die();
								killByShield.Play();
							}
							if (player.armor != 0)
							{
								ATKOnArmorHeavy.Play();
							}
							if (!player.armorIsZero && player.armor <= 0 && !player.killByShield)
							{
								breakArmor.Play();
								player.armorIsZero = true;
							}
						}
					}
					if (currentType is ATKType.T2)
					{
						isT2Success = true;
						hitAwayVec.Y -= 600;
						if (player.armor > 0 && player.armor < 3)
						{
							player.armor--;
						}
						//处理声音
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
						if (player.armorIsZero && player.armor <= 0)
						{
							shiedlHit.Play();
						}
					}
				}
			}
			#endregion

			itself.Velocity = itselfVec;
			player.Velocity = hitAwayVec;
			player.hitAwayVelocity = hitAwayVec;
		}
	}

	private void DisableMonitoring()
	{
		DEFArea.Monitoring = false;
	}
	#endregion
}
