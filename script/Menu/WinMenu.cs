using Godot;
using System;

public partial class WinMenu : CanvasLayer
{
	#region 外部接入
	[ExportGroup("节点接入")]
	[Export] public Panel title;
	[Export] public VBoxContainer buttonContainer;
	[Export] public Button retry;
	[Export] public Button quit;
	[Export] public Node2D anim;
	[Export] public Node2D arrows;

	[ExportGroup("贴图接入")]
	[Export] public Sprite2D head;
	[Export] public Sprite2D sword;
	[Export] public Sprite2D shield;
	[Export] public Texture2D redHead;
	[Export] public Texture2D blueHead;
	#endregion
	public enum EndType { killedBySword, killedByShield, killedByArrow }
	AnimationPlayer ap;
	public override void _Ready()
	{
		Visible = false;
		title.Modulate = new Color(1, 1, 1, 0);
		buttonContainer.Modulate = new Color(1, 1, 1, 0);
		sword.Visible = false;
		arrows.Visible = false;
		shield.Visible = false;
		retry.Disabled = true;
		quit.Disabled = true;
		//信号连接
		retry.Pressed += OnRetryPressed;
		quit.Pressed += OnQuitPressed;
	}
	public void ShowMenu(int playerId, EndType type)
	{
		Visible = true;
		if (playerId == 0)
		{
			head.Texture = redHead;
		}
		else if (playerId == 1)
		{
			head.Texture = blueHead;
		}
		switch (type)
		{
			case EndType.killedBySword:
				sword.Visible = true;
				//粒子效果
				GpuParticles2D ptcl = head.GetChild(0) as GpuParticles2D;
				Timer waitTimer = new()
				{
					WaitTime = 0.8,
					OneShot = true
				};
				AddChild(waitTimer);
				waitTimer.Timeout += () =>
				{
					ptcl.Emitting = true;
					waitTimer.QueueFree();
				};
				waitTimer.Start();
				//动画
				ap = sword.GetChild(0) as AnimationPlayer;
				ap.Play("Kill");
				break;
			case EndType.killedByArrow:
				arrows.Visible = true;
				ap = arrows.GetChild(0) as AnimationPlayer;
				ap.Play("Shoot");
				break;
			case EndType.killedByShield:
				shield.Visible = true;
				ap = shield.GetChild(0) as AnimationPlayer;
				ap.Play("Hit");
				break;
		}
		ap.AnimationFinished += (animName) =>
			{
				//UI动画
				Tween tweenPlayer = CreateTween().Parallel();
				tweenPlayer.TweenProperty(title, "modulate", new Color(1, 1, 1, 1), 0.5);
				tweenPlayer.TweenProperty(buttonContainer, "modulate", new Color(1, 1, 1, 1), 0.5);
				tweenPlayer.Finished += () =>
				{
					retry.Disabled = false;
					quit.Disabled = false;
					//清理动画
					tweenPlayer.Kill();
				};

			};
	}

	private void OnRetryPressed()
	{
		GetTree().Root.GetChild(0).QueueFree();
		GetTree().ReloadCurrentScene();
	}
	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
