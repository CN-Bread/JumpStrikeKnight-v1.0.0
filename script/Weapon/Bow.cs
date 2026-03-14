using Godot;
using Godot.Bridge;
using System;

public partial class Bow : WeaponBase
{
	#region 外部接入
	public PackedScene arrowPrefab;
	[Export] public Area2D aimArea;
	[Export] public Area2D blockingArea;
	[Export] public Node2D arrowSpawnPos;
	[Export] public Node2D reloadArrow;
	[Export] public AnimatedSprite2D bowTexture;
	[Export] public Node2D crosshair;
	[ExportCategory("音效接入")]
	[Export] public AudioStreamPlayer2D reload;
	[Export] public AudioStreamPlayer2D dragBow;
	[Export] public AudioStreamPlayer2D addArrow;
	#endregion
	#region 机制相关声明
	public int arrowLeft = 0;
	public int MAXARROWCOUNTS = 5;
	public bool isArrowOnBow = false;
	public bool isNeedAim = false;
	public bool autoAimMode = true;
	public bool autoAim = false;
	public bool isLoading = false;
	public bool isChargeFull = false;
	public bool hasCharged = false;
	public bool hasShot = false;
	//T2逻辑声明
	public bool canAddArrow = true;
	public bool addAnimFinished = true;
	//SPC逻辑声明
	public bool canBlock = true;
	public bool isBlocking = false;
	public bool isBlockingSuccess = false;
	private bool isRotatingBack = false;
	private bool afterShootRotatingBack = false;
	private Tween rotatingBackTween;
	private Tween blockingPrepareTween;

	//散布逻辑声明
	public float spreadRate;
	public float spreadDegree;

	//准心逻辑声明
	public bool isCrosshairOn = false;
	private Player itself;
	#endregion
	public override void _Ready()
	{
		//加入预制体
		arrowPrefab = GD.Load<PackedScene>("res://scene/Arrow.tscn");
		//初始化参数
		crosshair.Visible = false;
		reloadArrow.Visible = false;
		aimArea.Monitoring = false;
		aimArea.Monitorable = false;
		arrowLeft = 3;
		itself = GetParent().GetParent() as Player;
		//连接信号
		aimArea.BodyEntered += AutoAimOn;
		aimArea.BodyExited += AutoAimOutOfRange;
		blockingArea.AreaEntered += Blocking;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (autoAim && aimArea.Monitoring)
		{
			AutoAim();
		}
		IsHideCrosshairOnEnemy();
		if (isArrowOnBow)
		{
			bowTexture.Animation = "charge";
		}
		else
		{
			bowTexture.Animation = "default";
		}
		SpreadResult(itself.Velocity.X);
	}

	public void ShootArrow()
	{
		Arrow arrow = arrowPrefab.Instantiate() as Arrow;
		GetNode<Node2D>("/root/Root/BulletContainer").AddChild(arrow);
		Vector2 dir;
		spreadDegree = GD.RandRange(-1, 1) * spreadRate;
		if (isBlockingSuccess)
		{
			isBlockingSuccess = false;
			arrow.GlobalPosition = GlobalPosition;
			if ((GetParent() as WeaponManager).isFacingRight)
			{
				dir = new(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
				arrow.RotationDegrees = RotationDegrees + spreadDegree;
			}
			else
			{
				dir = new(-1 * Mathf.Cos(Rotation), Mathf.Sin(Rotation));
				arrow.RotationDegrees = 180 - RotationDegrees - spreadDegree;
			}
		}
		else
		{
			arrow.GlobalPosition = arrowSpawnPos.GlobalPosition;
			if ((GetParent() as WeaponManager).isFacingRight)
			{
				dir = new(Mathf.Cos(Rotation + Mathf.DegToRad(spreadDegree)), Mathf.Sin(Rotation + Mathf.DegToRad(spreadDegree)));
				arrow.RotationDegrees = RotationDegrees + spreadDegree;
			}
			else
			{
				dir = new(-1 * Mathf.Cos(Rotation + Mathf.DegToRad(spreadDegree)), Mathf.Sin(Rotation + Mathf.DegToRad(spreadDegree)));
				arrow.RotationDegrees = 180 - RotationDegrees - spreadDegree;
			}
		}
		arrow.ArrowLaunch(dir, itself.playerId);
		canSwitchWeapon = true;
	}
	public void PlayReloadAnim()
	{
		reloadArrow.Visible = true;
		reload.Play();
		//初始化参数
		RotationDegrees = 45;
		reloadArrow.RotationDegrees = 45;
		(GetChild(0).GetChild(0) as Node2D).Position = new Vector2(-4, 4);
		(GetChild(0).GetChild(0) as Sprite2D).RotationDegrees = -180;
		(GetChild(0).GetChild(0) as Node2D).Modulate = new Color(1, 1, 1, 0.2f);
		//使用Tween动画
		Tween modulateTween = CreateTween();
		modulateTween.TweenProperty(GetChild(0).GetChild(0) as Node2D, "modulate:a", 1, 0.2)
					.SetEase(Tween.EaseType.Out);
		Tween rotateTween = CreateTween();
		rotateTween.TweenProperty(reloadArrow, "rotation", Mathf.DegToRad(225), 0.2)
					.SetEase(Tween.EaseType.Out);

		rotateTween.Finished += () =>
		{
			Tween loadTween = CreateTween();
			loadTween.TweenProperty(GetChild(0).GetChild(0) as Sprite2D, "position", new Vector2((float)-1.275, (float)1.275), 0.1);
			//控制播放完后消失
			loadTween.Finished += () =>
			{
				reloadArrow.Visible = false;

				canSwitchWeapon = true;
				isArrowOnBow = true;
				isLoading = false;
				if (!canBlock)
				{
					canBlock = true;
				}
				//清理tween
				modulateTween.Kill();
				rotateTween.Kill();
				loadTween.Kill();
			};
		};


	}
	public void PlayAddArrowAnim()
	{
		reloadArrow.Visible = true;
		addArrow.Play();
		//初始化参数
		RotationDegrees = 45;
		reloadArrow.RotationDegrees = 45;
		(GetChild(0).GetChild(0) as Node2D).Position = new Vector2(-7, 7);
		(GetChild(0).GetChild(0) as Node2D).RotationDegrees = -180;
		(GetChild(0).GetChild(0) as Node2D).Modulate = new Color(1, 1, 1, 0.2f);
		//播放动画
		Tween modulateTween = CreateTween();
		modulateTween.TweenProperty(GetChild(0).GetChild(0) as Sprite2D, "modulate", new Color(1, 1, 1, 1), 0.15)
						.SetEase(Tween.EaseType.Out);
		Tween rotationTween = CreateTween();
		rotationTween.TweenProperty(GetChild(0).GetChild(0) as Sprite2D, "rotation_degrees", 405, 0.3)
						.SetEase(Tween.EaseType.Out);
		Tween positionTween = CreateTween();
		positionTween.TweenProperty(GetChild(0).GetChild(0) as Sprite2D, "position", new Vector2(-5, 5), 0.3)
						.SetEase(Tween.EaseType.In);
		rotationTween.Finished += () =>
		{
			modulateTween.Kill();
			rotationTween.Kill();
			Timer stay = new()
			{
				WaitTime = 0.1,
				OneShot = true
			};
			AddChild(stay);
			stay.Timeout += () =>
			{
				Tween positionTween = CreateTween().Parallel();
				positionTween.TweenProperty(GetChild(0).GetChild(0) as Sprite2D, "position", new Vector2(-0.5f, 5), 0.1)
								.SetEase(Tween.EaseType.In);
				positionTween.TweenProperty(GetChild(0).GetChild(0) as Sprite2D, "modulate", new Color(1, 1, 1, 0), 0.1)
						.SetEase(Tween.EaseType.Out);
				positionTween.Finished += () =>
				{
					arrowLeft++;
					addAnimFinished = true;
					reloadArrow.Visible = false;
					canBlock = true;
					//清理tween和timer
					positionTween.Kill();
					stay.QueueFree();
				};
			};
			stay.Start();
		};

	}
	public void FullCharged()
	{
		isChargeFull = true;
		bowTexture.AnimationFinished -= FullCharged;
		canSwitchWeapon = false;
		//开始检测瞄准区域
		aimArea.Monitoring = true;
		//检测是否需要准心
		if (autoAimMode)
		{
			isCrosshairOn = true;
		}
		else
		{
			isCrosshairOn = false;
		}
	}

	//弓箭状态机
	public override void EnterBowFlow(ATKType type)
	{
		//按键状态
		switch (type)
		{
			case ATKType.dft:
				//默认状态
				isNeedAim = false;
				isArrowOnBow = false;
				isChargeFull = false;
				RotationDegrees = 45;
				bowTexture.Animation = "default";
				break;
			case ATKType.Reload:
				if (arrowLeft > 0 && !isArrowOnBow && !isLoading && canBlock && addAnimFinished)
				{
					PlayReloadAnim();
					isLoading = true;
					canBlock = false;
				}
				//切换自动/手动瞄准功能
				if (isArrowOnBow)
				{
					if (autoAimMode)
					{
						//手动瞄准固定角度，但是扩散小
						autoAimMode = false;
						autoAim = false;
						if (isChargeFull)
						{
							Tween rotateTween = CreateTween();
							rotateTween.TweenProperty(this, "rotation", Mathf.DegToRad(0), 0.1);
						}
					}
					else
					{
						//自动瞄准会改变扩散，和角色移速有关
						autoAimMode = true;
						autoAim = true;
					}
				}
				break;
			//拉弓/射箭逻辑
			case ATKType.T1:
				if (isArrowOnBow && !hasCharged && !isLoading && canBlock && addAnimFinished)
				{
					hasCharged = true;
					canBlock = false;
					canAddArrow = false;
					//拉弓动画
					Tween rotateTween = CreateTween();
					rotateTween.TweenProperty(this, "rotation", Mathf.DegToRad(0), 0.2);

					bowTexture.AnimationFinished += FullCharged;
					bowTexture.Play();
					//拉弓音效
					dragBow.Play();
				}
				if (isChargeFull && !hasShot)
				{
					ShootArrow();
					canAddArrow = true;
					afterShootRotatingBack = true;
					autoAim = false;
					arrowLeft--;
					isArrowOnBow = false;
					aimArea.Monitoring = false;
					hasShot = true;
					hasCharged = false;
					isChargeFull = false;
					//模拟后坐力
					if ((GetParent() as WeaponManager).isFacingRight && RotationDegrees < 90)
					{
						RotationDegrees += 20;
					}
					else if (!(GetParent() as WeaponManager).isFacingRight && RotationDegrees > -90)
					{
						RotationDegrees -= 20;
					}
					//弓复位
					Tween rotateTween = CreateTween();
					rotateTween.TweenProperty(this, "rotation", Mathf.DegToRad(45), 0.1);
					rotateTween.Finished += () =>
						{
							hasShot = false;
							afterShootRotatingBack = false;
							if (!canBlock)
							{
								canBlock = true;
							}
						};
				}
				break;
			//增加箭逻辑
			case ATKType.T2:
				if (canAddArrow && arrowLeft < MAXARROWCOUNTS && !isLoading && !hasCharged)
				{
					PlayAddArrowAnim();
					addAnimFinished = false;
					canAddArrow = false;
					canBlock = false;
					Timer cooling = new Timer
					{
						WaitTime = 2,
						OneShot = true
					};
					AddChild(cooling);
					cooling.Timeout += () => { canAddArrow = true; };
					cooling.Start();
				}
				break;
			//弓反逻辑
			case ATKType.SPC:
				if (!canBlock)
				{
					break;
				}
				if (canBlock)
				{
					blockingPrepareTween?.Kill();
					canBlock = false;
					canAddArrow = false;
					canSwitchWeapon = false;
					ItCanNotFlip();
					blockingArea.Monitoring = true;
					RotationDegrees = 45;
					blockingPrepareTween = CreateTween();

					blockingPrepareTween.TweenProperty(this, "rotation", Mathf.DegToRad(90), 0.05);

					blockingPrepareTween.Finished += () =>
					{
						isBlocking = true;
						blockingArea.Monitorable = true;
						blockingArea.Monitoring = true;
						Tween blockingTween = CreateTween();
						blockingTween.SetParallel(true);
						blockingTween.TweenProperty(this, "rotation", Mathf.DegToRad(-45), 0.2);
						blockingTween.TweenProperty(this, "position", new Vector2((float)-1.245, (float)-3.38), 0.2);

						blockingTween.Finished += () =>
						{
							blockingTween.Kill();
							isBlocking = false;
							blockingArea.Monitorable = false;
							blockingArea.Monitoring = false;
							Tween back = CreateTween();
							back.SetParallel(true);
							back.TweenProperty(this, "rotation", Mathf.DegToRad(45), 0.15);
							back.TweenProperty(this, "position", new Vector2((float)-3.085, (float)-0.875), 0.15);

							canSwitchWeapon = true;
							ItCanFlip();

							back.Finished += () =>
							{
								if (isBlockingSuccess)
								{
									isBlockingSuccess = false;
								}
								back.Kill();
								Timer t = new()
								{
									WaitTime = 0.5,
									OneShot = true
								};
								AddChild(t);
								t.Timeout += () => { canBlock = true; canAddArrow = true; t.QueueFree(); };
								t.Start();
							};
						};
					};

				}
				break;
		}
	}

	public void AutoAimOn(Node2D body)
	{
		//当敌人出现在瞄准范围内
		if (body is Player enemy && enemy != itself && autoAimMode)
		{
			autoAim = true;
			if (isRotatingBack)
			{
				rotatingBackTween?.Kill();
				rotatingBackTween = null;
				isRotatingBack = false;
			}
		}
	}
	public void AutoAimOutOfRange(Node2D body)
	{
		//当敌人离开瞄准范围
		if (body is Player enemy && enemy != itself && autoAimMode)
		{
			enemy.bow.crosshair.Visible = false;
			autoAim = false;
			if (afterShootRotatingBack)
			{
				return;
			}
			isRotatingBack = true;  // 标记正在复位
			if (isRotatingBack)
			{
				// 停止之前的Tween
				rotatingBackTween?.Kill();
				//开始复位
				rotatingBackTween = CreateTween();
				rotatingBackTween.TweenProperty(this, "rotation", Mathf.DegToRad(0), 0.1);
				rotatingBackTween.Finished += () =>
				{
					isRotatingBack = false;  // 复位完成
					rotatingBackTween = null;
				};
			}

		}
	}
	public void AutoAim()
	{
		if (afterShootRotatingBack)
		{
			return;
		}

		Player itself = GetParent().GetParent() as Player;
		// 获取瞄准区域内所有碰撞体
		var bodies = aimArea.GetOverlappingBodies();
		if (bodies.Count == 0) return;

		foreach (var body in bodies)
		{
			// 排除自己
			if (body == itself) continue;

			// 瞄准敌人
			if (body is Player enemy && enemy != itself)
			{
				//准心逻辑
				Vector2 vecOfEnemy = enemy.GlobalPosition - arrowSpawnPos.GlobalPosition;
				float r = vecOfEnemy.Length() * Mathf.Sin(Mathf.DegToRad(spreadRate));
				(enemy.bow.crosshair as Crosshair).SpreadCrosshair(r);

				//转向逻辑
				Vector2 dirOfEnemy = vecOfEnemy.Normalized();
				if (itself.isRight && dirOfEnemy.X > 0)
				{
					Rotation = Mathf.Atan2(dirOfEnemy.Y, dirOfEnemy.X);
				}
				if (!itself.isRight && dirOfEnemy.X < 0)
				{
					Rotation = Mathf.Atan2(dirOfEnemy.Y, -1 * dirOfEnemy.X);
				}
			}
		}

	}
	public void SpreadResult(float speed)
	{
		if (autoAim)
		{
			if (Mathf.Abs(speed) > 60 && Mathf.Abs(speed) <= 300)
			{
				spreadRate = Mathf.Abs(speed) / 300 * 5;
			}
			else if (Mathf.Abs(speed) > 300)
			{
				spreadRate = 5;
			}
			else if (Mathf.Abs(speed) <= 60)
			{
				spreadRate = 1;
			}
		}
		else
		{
			spreadRate = 0.5f;
		}

	}
	public void IsHideCrosshairOnEnemy()
	{
		if (autoAimMode && aimArea.Monitoring)
		{
			foreach (var body in aimArea.GetOverlappingBodies())
			{
				if (body is Player enemy)
				{
					enemy.bow.crosshair.Visible = true;
				}
			}
		}
		else if (!autoAimMode && aimArea.Monitoring)
		{
			foreach (var body in aimArea.GetOverlappingBodies())
			{
				if (body is Player enemy)
				{
					enemy.bow.crosshair.Visible = false;
				}
			}
		}

	}
	public void Blocking(Node2D area)
	{
		if (area is Area2D swordArea && swordArea.GetParent() is Sword sword)
		{
			if (sword.currentType is ATKType.T1 || sword.currentType is ATKType.T2)
			{
				isBlockingSuccess = true;
				itself.Velocity = new Vector2(0, itself.Velocity.Y);
				isArrowOnBow = true;
				if (arrowLeft < MAXARROWCOUNTS)
				{
					arrowLeft++;
				}
			}
		}
		if (area is Area2D && area.Owner is Arrow)
		{
			isBlockingSuccess = true;
			itself.Velocity = new Vector2(0, itself.Velocity.Y);
			isArrowOnBow = true;
			if (arrowLeft < MAXARROWCOUNTS)
			{
				arrowLeft++;
			}
		}
		if (area is Area2D shieldArea && shieldArea.GetParent() is Shield shield && shield.isRaisingShield)
		{
			if (shield.currentType is ATKType.T1 || shield.currentType is ATKType.T2)
			{
				isBlockingSuccess = true;
				itself.Velocity = new Vector2(0, itself.Velocity.Y);
				isArrowOnBow = true;
				if (arrowLeft < MAXARROWCOUNTS)
				{
					arrowLeft++;
				}
			}
		}
	}
	public void BlockSuc()
	{
		(GetParent() as WeaponManager).HitStop(0.02f, true);
	}
	#region 武器具体Action(abandoned)
	public override void PrepareAction(ATKType type)
	{
		throw new NotImplementedException();
	}

	public override void ATKingAction(ATKType type)
	{
		throw new NotImplementedException();
	}

	public override void RecoveryAction(ATKType type)
	{
		throw new NotImplementedException();
	}

	public override void CoolDownAction(ATKType type)
	{
		throw new NotImplementedException();
	}

	#endregion
	#region Action参数(abandoned)
	protected override float GetPrepareDuration(ATKType type)
	{
		throw new NotImplementedException();
	}

	protected override float GetATKDuration(ATKType type)
	{
		throw new NotImplementedException();
	}

	protected override float GetRecoveryDuration(ATKType type)
	{
		throw new NotImplementedException();
	}

	protected override float GetCoolDownDuration(ATKType type)
	{
		throw new NotImplementedException();
	}

	protected override float GetCanNotATKDuration()
	{
		throw new NotImplementedException();
	}
	#endregion
}
