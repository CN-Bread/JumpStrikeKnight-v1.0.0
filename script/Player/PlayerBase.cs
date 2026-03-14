using Godot;
using System;
using System.Diagnostics.CodeAnalysis;

public abstract partial class PlayerBase : CharacterBody2D
{
	#region 玩家区分声明
	[Export] public int playerId;
	#endregion
	#region 玩家按键声明
	[ExportGroup("玩家控键")]
	[Export] protected string leftKey;
	[Export] protected string rightKey;
	[Export] protected string jumpKey;
	[Export] protected string downKey;
	
	[Export] protected string switchKey1;
	[Export] protected string switchKey2;
	[Export] protected string switchKey3;
	[Export] protected string key1;
	[Export] protected string key2;
	[Export] protected string key3;
	[Export] public string reloadKey;
	#endregion

	#region 玩家健康状态声明
	protected int maxHP = 4;
	public int HP;
	protected int maxArmor = 4;
	public int armor;
	public Timer healTimer;
	#endregion

	#region 玩家移动声明
	[ExportGroup("移动参数")]
	public bool isCanJump = false;
	public Timer canJumpTimer;
	public bool canControl = true;
	[Export] public float moveSpeed;
	[Export] public float maxMoveSpeed;
	[Export] public float jumpForce;
	[Export] protected float gravity;
	[Export] protected float friction;
	public bool isRight;
	protected bool lastFrame;
	protected bool nowCanFlip;

	public Timer beingHitAwayTimer = new() { OneShot = true };
	public float HITAWAYTIME = 0.15f;
	[Export] public AnimatedSprite2D charaTextrue;
	public bool isBeingHitAway = false; // 明确标记是否处于击退状态
	public Vector2 hitAwayVelocity = Vector2.Zero; // 保存初始的击退速度
	#endregion

	#region 武器相关声明
	[ExportGroup("武器相关节点接入")]
	[Export] public WeaponManager weaponMgr;
	[Export] public Area2D aimArea;
	public bool killBySword = false;
	public bool killedByArrow = false;
	public bool killByShield = false;
	#endregion

	#region 其他接入
	[ExportGroup("UI接入")]
	[Export] public PlayerUI playerUI;
	[ExportGroup("粒子效果节点接入")]
	[Export] public Praticles praticles;
	[ExportGroup("WinMenu接入")]
	[Export] public WinMenu winMenu;
	[ExportGroup("音效接入")]
	[Export] public AudioStreamPlayer2D footSteps;
	[Export] public AudioStreamPlayer2D jump;
	[Export] public AudioStreamPlayer2D fallOnGround;
	#endregion
	public override void _Ready()
	{
		//确保离开摄像机依然进行
		ProcessMode = ProcessModeEnum.Always;
		//状态初始化
		HP = maxHP;
		armor = maxArmor;
		healTimer = new Timer
		{
			WaitTime = 2,
			OneShot = true
		};
		AddChild(healTimer);
		AddChild(beingHitAwayTimer);
		//信号连接
		healTimer.Timeout += () =>
		{
			AddHP(1);
			if (HP != maxHP)
			{
				healTimer.Start();
			}
		};
		//当击退计时器停止时，设置isBeingHitAway为false
		beingHitAwayTimer.Timeout += () =>
		{
			isBeingHitAway = false;
			hitAwayVelocity = Vector2.Zero;
		};
		Visible = true;
		//速度初始化
		isRight = true;
		lastFrame = isRight;
		nowCanFlip = true;
		Velocity = new Vector2(0, 0);
		moveSpeed = 50;
		maxMoveSpeed = 500;
		jumpForce = 500;
		friction = 0.2f;
		gravity = 1200;
		//土狼跳计时器初始化
		canJumpTimer = new Timer
		{
			OneShot = true,
			WaitTime = 0.05f
		};
		AddChild(canJumpTimer);
		canJumpTimer.Timeout += () =>
		{
			isCanJump = false;
		};
	}

	public override void _PhysicsProcess(double delta)
	{

	}

	protected void MoveFunc(float delta)
	{
		Vector2 velocity = Velocity;
		// 记录进入方法时的速度
		if (!isBeingHitAway)
		{
			//行走逻辑
			int direction = 0;
			if (Input.IsActionPressed(leftKey))
			{
				direction = -1;
				switch (weaponMgr.currentWeapon)
				{
					case Sword:
						if (velocity.X < 200)
						{
							isRight = false;
						}
						break;
					case Shield:
						if (velocity.X < 0)
						{
							isRight = false;
						}
						break;
					case Bow:
						isRight = false;
						aimArea.Scale = new Vector2(-1, 1);
						break;
				}
			}
			if (Input.IsActionPressed(rightKey))
			{
				direction = 1;
				switch (weaponMgr.currentWeapon)
				{
					case Sword:
						if (velocity.X > -200)
						{
							isRight = true;
						}
						break;
					case Shield:
						if (velocity.X > 0)
						{
							isRight = true;
						}
						break;
					case Bow:
						isRight = true;
						aimArea.Scale = new Vector2(1, 1);
						break;
				}

			}
			// 如果有方向输入，应用移动
			if (direction != 0)
			{
				velocity.X += direction * moveSpeed;
				if (IsOnFloor() && !footSteps.Playing)
				{
					footSteps.Play();
				}
			}

			//模拟惯性逻辑
			if (velocity.X != 0 && IsOnFloor() && direction == 0 && !isBeingHitAway)
			{
				velocity.X *= 1 - friction;
				//防止速度无限小
				if (MathF.Abs(velocity.X) < 5f)
				{
					velocity.X = 0;
				}
			}

			//限制行走最大速度
			if (velocity.X < -1 * maxMoveSpeed)
			{
				velocity.X = -1 * maxMoveSpeed;
			}
			if (velocity.X > maxMoveSpeed)
			{
				velocity.X = maxMoveSpeed;
			}
		}
		else
		{
			//禁用土狼跳
			canJumpTimer.Stop();
			isCanJump = false;
			//使用保存的击退速度作为基础
			velocity = hitAwayVelocity;
			//限制击飞最大速度
			if (velocity.X < -2000)
			{
				velocity.X = -2000;
			}
			if (velocity.X > 2000)
			{
				velocity.X = 2000;
			}
		}
		//跳跃逻辑
		if (IsOnFloor())
		{
			canJumpTimer.Stop();
			isCanJump = true;
		}
		else
		{
			canJumpTimer.Start();
		}

		if (isCanJump && Input.IsActionPressed(jumpKey))
		{
			velocity.Y -= jumpForce;
			isCanJump = false;
			jump.Play();
		}
		//模拟重力
		if (!IsOnFloor())
		{
			velocity.Y += gravity * delta;
		}
		// 记录设置速度前的速度
		Velocity = velocity;
		MoveAndSlide();
		// 记录MoveAndSlide后的速度

		//翻转逻辑
		if (nowCanFlip)
		{
			if (lastFrame != isRight)
			{

				if (isRight == false)
				{
					charaTextrue.FlipH = true;
				}
				else
				{
					charaTextrue.FlipH = false;
				}

				//更新上一帧状态
				lastFrame = isRight;
			}
		}
	}

	public void BeingHit(int damage, bool isPierce = false)
	{
		//基础受击
		if (!isPierce)
		{
			if (armor >= damage)
			{
				armor -= damage;
			}
			else
			{
				HP -= damage - armor;
				armor = 0;
			}
		}
		else
		{
			HP--;
		}

		//恢复逻辑（受击打断）
		if (HP != maxHP)
		{
			healTimer.Stop();
			healTimer.Start();
		}
	}
	public void Die()
	{
		if (!killedByArrow)
		{
			charaTextrue.Visible = false;
		}
		canControl = false;
		weaponMgr.Visible = false;
		healTimer.QueueFree();
		CollisionLayer = 0;
		praticles.Die();
		//菜单相关
		WinMenu.EndType endType = WinMenu.EndType.killedBySword;
		if (killedByArrow)
		{
			endType = WinMenu.EndType.killedByArrow;
		}
		else if (killByShield)
		{
			endType = WinMenu.EndType.killedByShield;
		}
		winMenu.ShowMenu(playerId, endType);

	}

	public void AddArmor(int count)
	{
		armor += count;
		if (armor >= maxArmor)
		{
			armor = maxArmor;
		}
		Tween shin = CreateTween();
		shin.TweenProperty(playerUI.armorContainer, "modulate", new Color(1, 1, 1, 1) * 1.25f, 0.2f);
		shin.TweenProperty(playerUI.armorContainer, "modulate", new Color(1, 1, 1, 1), 0.15f);
	}

	public void AddHP(int count)
	{
		HP += count;
		if (HP >= maxHP)
		{
			HP = maxHP;
		}
		Tween shin = CreateTween();
		shin.TweenProperty(playerUI.heartContainer, "modulate", new Color(1, 1, 1, 1) * 1.25f, 0.2f);
		shin.TweenProperty(playerUI.heartContainer, "modulate", new Color(1, 1, 1, 1), 0.15f);
	}
}