using Godot;
using System;

public partial class Arrow : CharacterBody2D
{
	#region 声明
	private int power = 1;
	[Export] public Sprite2D arrowTexture;
	[Export] public Area2D arrowArea;
	[ExportGroup("拖尾相关")]
	[Export] public Node2D brokenArrow;
	[Export] public Node2D trial;
	[Export] public Node2D tailPos;
	[Export] public Node2D praticles;
	[ExportGroup("声音相关")]
	[Export] public AudioStreamPlayer2D brokenSound;
	[Export] public AudioStreamPlayer2D arrowFly;
	[Export] public AudioStreamPlayer2D arrowHitGround;
	[Export] public AudioStreamPlayer2D arrowHitPlayer;
	[Export] public AudioStreamPlayer2D arrowKill;
	public Line2D trialLine;
	private bool isNeedTrail = true;
	public float flySpeed = 1000;
	public float gravity = 50;
	private bool needGravity;
	public Vector2 vel = new(0,0);
	public int shooterId;
	#endregion
	public override void _Ready()
	{
		trialLine = trial.GetChild(0) as Line2D;
		//绑定信号
		arrowArea.BodyEntered += OnBodyEntered;
		arrowArea.AreaEntered += OnAreaEntered;
		(brokenArrow.GetChild(1) as GpuParticles2D).Finished += QueueFree;
	}

    public override void _PhysicsProcess(double delta)
	{
		GravityEffect((float)delta);
		TrialLine(5);
		MoveAndSlide();
	}

	public void ArrowLaunch(Vector2 dir,int playerId)
	{
		shooterId = playerId;
		arrowArea.Monitoring = true;
		isNeedTrail = true;
		vel = dir * flySpeed;
		Velocity = vel;
		needGravity = true;
		Timer lifeTime = new()
        {
            WaitTime = 20,
			OneShot = true
        };
        lifeTime.Timeout += QueueFree;
		AddChild(lifeTime);
		lifeTime.Start();
		arrowFly.Play();
	}

	private void GravityEffect(float delta)
	{
		if (needGravity)
		{
			Vector2 velocity = Velocity;
			velocity.Y += gravity * delta;
			Velocity = velocity;
		}
	}
	private void TrialLine(int pointCounts)
	{
		if (isNeedTrail)
		{
			trialLine.AddPoint(trial.GlobalPosition);
			if(trialLine.GetPointCount() > pointCounts)
			{
				trialLine.RemovePoint(0);
			}
		}
		else
		{
			if(trialLine.GetPointCount() > 0)
			{
				trialLine.RemovePoint(0);
			}
		}
		
		
	}
	private void FadeAway()
	{
		isNeedTrail = false;
		Tween tween = CreateTween();
		tween.TweenProperty(this,"modulate:a",0,2.5)
			.SetEase(Tween.EaseType.Out);
		tween.Finished += QueueFree;
	}
	private void BreakArrow()
	{
		isNeedTrail = false;
		
		GpuParticles2D particle0 = brokenArrow.GetChild(0) as GpuParticles2D;
		GpuParticles2D particle1 = brokenArrow.GetChild(1) as GpuParticles2D;
		
		
		brokenArrow.GlobalPosition = GlobalPosition;
		
		particle0.Position = Vector2.Zero;
		particle1.Position = Vector2.Zero;
		
		particle0.Emitting = true;
		particle1.Emitting = true;

		brokenSound.Play();
	}
    private void OnBodyEntered(Node2D body)
    {
		if(body is StaticBody2D)
		{
			CallDeferred(nameof(DisableMonitoring));  // 延迟关闭
			arrowHitGround.Play();
			needGravity = false;
			Velocity = new(0,0);
			FadeAway();
		}
		if(body is Player player && player.playerId != shooterId)
		{
			CallDeferred(nameof(DisableMonitoring));  // 延迟关闭
			needGravity = false;
			Velocity = new(0,0);

			Vector2 dirOfPlayer = (player.GlobalPosition - GlobalPosition).Normalized();
			Vector2 hitAwayVec = player.Velocity;

			if(player.sword.isSwordBlocking || player.bow.isBlocking)
			{
				BreakArrow();
				arrowTexture.Visible = false;
			}
			else
			{
				
				if(player.armor >= 3)
				{
					player.BeingHit(power);
				}
				else
				{
					player.HP--;
					(praticles.GetChild(0) as GpuParticles2D).Emitting = true;
				}
				
				if(player.HP > 0)
				{
					hitAwayVec.X += dirOfPlayer.X * 500;
					hitAwayVec.Y += dirOfPlayer.Y * 250;
					player.Velocity = hitAwayVec;
					arrowHitPlayer.Play();
					Visible = false;
					arrowHitPlayer.Finished += QueueFree;
				}
				else if(player.HP <= 0)
				{
					player.killedByArrow = true;
					arrowKill.Play();
					(praticles.GetChild(1) as GpuParticles2D).Emitting = true;
					player.Die();
				}

			}
		}
    }
	//被格挡函数
	private void OnAreaEntered(Node2D area)
	{
		if(area is Area2D shieldArea && shieldArea.GetParent() is Shield shield && shield.isRaisingShield)
		{
			CallDeferred(nameof(DisableMonitoring));  // 延迟关闭
			BreakArrow();
			arrowTexture.Visible = false;
		}
		if(area is Area2D swordArea && swordArea.GetParent() is Sword sword && sword.isSwordBlocking)
		{
			CallDeferred(nameof(DisableMonitoring));  // 延迟关闭
			BreakArrow();
			arrowTexture.Visible = false;
			(sword.GetParent().GetParent() as Player).AddArmor(2);
		}
		if(area is Area2D bowArea && bowArea.GetParent() is Bow bow && bow.isBlocking)
		{
			CallDeferred(nameof(DisableMonitoring));  // 延迟关闭
			BreakArrow();
			arrowTexture.Visible = false;
			(bow.GetParent().GetParent() as Player).AddArmor(1);
		}
	}
	private void DisableMonitoring()
    {
        arrowArea.Monitoring = false;
    }
}
