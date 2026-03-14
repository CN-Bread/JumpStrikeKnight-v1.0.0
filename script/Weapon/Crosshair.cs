using Godot;
using System;

public partial class Crosshair : Node2D
{
	#region 外部接入
	[Export] public Sprite2D top;
	[Export] public Sprite2D left;
	[Export] public Sprite2D right;
	[Export] public Sprite2D bottom;

	[Export] public Player player;
	#endregion
	#region 机制相关
	public bool isAiming = false;
	#endregion
	public override void _Ready()
	{
		top.Position = new Vector2(0,0);
		left.Position = new Vector2(0,0);
		right.Position = new Vector2(0,0);
		bottom.Position = new Vector2(0,0);
	}

	public override void _PhysicsProcess(double delta)
	{
		
	}

	public void SpreadCrosshair(float r)
	{
		if(r > 5)
		{
			r -= 5f;
			top.Position = new Vector2(0,-r);
			left.Position = new Vector2(-r,0);
			right.Position = new Vector2(r,0);
			bottom.Position = new Vector2(0,r);
		}
	}

}
