using Godot;
using System;

public partial class DeadZone : Area2D
{
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

    private void OnBodyEntered(Node2D body)
    {
        if(body is Player p)
		{
			p.Die();
		}
		if(body is Arrow a)
		{
			a.QueueFree();
		}
    }

}
