using Godot;
using System;

public partial class Camera : Camera2D
{
	[Export] Player p1;
	[Export] Player p2;

	[ExportGroup("Zoom Settings")]
    [Export] public float MinZoom = 0.1f;      // 最小缩放（最远距离）
    [Export] public float MaxZoom = 1.1f;      // 最大缩放（最近距离）
    [Export] public float ZoomMargin = 200f;   // 边缘留白（像素）
	public Vector2 centerPos;
	public float width;
	public float height;
	float zoomLevelX;
	float zoomLevelY;
	float zoomLevel;
	public override void _Ready()
	{
		Zoom = new Vector2(1,1);
		GlobalPosition = new Vector2(0,0);
	}

	public override void _PhysicsProcess(double delta)
	{
		CalculateCenterPos((float)delta);
		CalculateZoomSize((float)delta);
	}

	public void CalculateCenterPos(float delta)
	{
		if(p1.canControl && p2.canControl)
		{
			centerPos = (p1.GlobalPosition + p2.GlobalPosition) / 2;
			GlobalPosition = GlobalPosition.Lerp(centerPos,5f * delta);
		}
		if(p1.canControl && !p2.canControl)
		{
			centerPos = p1.GlobalPosition;
			centerPos.Y -= 100;
			GlobalPosition = GlobalPosition.Lerp(centerPos,5f * delta);
		}
		if(!p1.canControl && p2.canControl)
		{
			centerPos = p2.GlobalPosition;
			centerPos.Y -= 100;
			GlobalPosition = GlobalPosition.Lerp(centerPos,5f * delta);
		}
	}

	public void CalculateZoomSize(float delta)
	{
		if(p1.canControl && p2.canControl)
		{
			Vector2 distance = p1.GlobalPosition - p2.GlobalPosition;
			width = Mathf.Abs(distance.X) + ZoomMargin * 2;
			height = Mathf.Abs(distance.Y) + ZoomMargin * 2;

			zoomLevelX = GetViewportRect().Size.X / width;
			zoomLevelX = Mathf.Clamp(zoomLevelX,MinZoom,MaxZoom);
			zoomLevelY = GetViewportRect().Size.Y / height;
			zoomLevelY = Mathf.Clamp(zoomLevelY,MinZoom,MaxZoom);

			zoomLevel = Mathf.Min(zoomLevelX, zoomLevelY);
			Zoom = Zoom.Lerp(new Vector2(zoomLevel,zoomLevel),5f * delta);
		}
		else
		{
			Zoom = Zoom.Lerp(new Vector2(1.5f,1.5f),5f * delta);
		}
	}
}
