using Godot;
using System;

public partial class RainFall : GpuParticles2D
{
	[Export] Texture2D rainFall;
	[Export] Texture2D HeavyRainFall;
	public override void _Ready()
	{
		Texture = rainFall;
        Timer rainTimer = new()
        {
            OneShot = true,
            WaitTime = 10
        };
		AddChild(rainTimer);
		rainTimer.Start();
		rainTimer.Timeout += () =>
		{
			Texture = HeavyRainFall;
		};
    }


}
