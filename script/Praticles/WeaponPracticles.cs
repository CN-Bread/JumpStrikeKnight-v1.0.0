using Godot;
using System;

public partial class WeaponPracticles : GpuParticles2D
{
	[Export] Texture2D sword;
	[Export] Texture2D shield;
	[Export] Texture2D bow; 
	[Export] WeaponManager weaponManager;
	public override void _Ready()
	{
		weaponManager = (Owner.GetParent() as Player).weaponMgr;
	}
	public override void _Process(double delta)
	{
		if(weaponManager.currentWeapon is Sword)
		{
			Texture = sword;
		}
		if(weaponManager.currentWeapon is Shield)
		{
			Texture = shield;
		}
		if(weaponManager.currentWeapon is Bow)
		{
			Texture = bow;
		}
	}
}
