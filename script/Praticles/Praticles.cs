using Godot;
using System;

public partial class Praticles : Node2D
{
	[ExportGroup("节点接入")]
	[Export] Player player;
	[Export] GpuParticles2D bloodSplash;
	[Export] GpuParticles2D armorSplash;
	[Export] GpuParticles2D armorBroken;
	[Export] GpuParticles2D armorFixed;

	[Export] GpuParticles2D head;
	[Export] GpuParticles2D body;
	[Export] GpuParticles2D weapon;
	[Export] GpuParticles2D bodyBloodSplash;

	[ExportGroup("材质接入")]
	[Export] Texture2D redHead;
	[Export] Texture2D blueHead;

	private Player itself;
	private bool armorIsZero = false;
	private int currentArmor = 4;
	private int currentHP = 4;
	public override void _Ready()
	{
		itself = GetParent() as Player;
		head.Finished += () =>
		{
			itself.QueueFree();
		};
	}
	public override void _PhysicsProcess(double delta)
	{
		if(itself.armor < currentArmor)
		{
			armorSplash.Emitting = true;
			currentArmor = itself.armor;
		}
		else if(itself.armor > currentArmor)
		{
			currentArmor = itself.armor;
		}

		if(itself.HP < currentHP)
		{
			bloodSplash.Emitting = true;
			currentHP = itself.HP;
		}
		else if(itself.HP > currentHP)
		{
			currentHP = itself.HP;
		}


		if (armorIsZero && itself.armor > 0)
		{
			armorFixed.Emitting = true;
			armorIsZero = false;
		}
		if(!armorIsZero && itself.armor <= 0)
		{
			armorBroken.Emitting = true;
			armorIsZero = true;
		}
	}

	public void Die()
	{
		if (itself.killedByArrow)
		{
			weapon.Emitting = true;
		}
		else
		{
			if(player.playerId == 0)
			{
				head.Texture = redHead;
			}
			else if(player.playerId == 1)
			{
				head.Texture = blueHead;
			}
			head.Emitting = true;
			body.Emitting = true;
			weapon.Emitting = true;

			bodyBloodSplash.Emitting = true;

		}	
	}
}
