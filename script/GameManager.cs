using Godot;
using System;

public partial class GameManager : Node2D
{
	#region 地图预制体
	public PackedScene castle;
	public PackedScene field;
	public PackedScene campanile;
	public PackedScene forest;
	#endregion
	[Export] public TitleMenu titleMenu;
	[ExportGroup("玩家管理节点")]
	[Export] public Node2D player;
	[Export] public Player redKnight;
	[Export] public Player blueKnight;
	public override void _Ready()
	{
		(titleMenu.loadAnim.GetParent() as Node2D).Visible = false;
		//初始化
		titleMenu.Visible = true;
		player.Visible = false;
		redKnight.canControl = false;
		blueKnight.canControl = false;
		//信号绑定
		titleMenu.castleSelection.Pressed += OnSelectCastle;
		titleMenu.fieldSelection.Pressed += OnSelectField;
		titleMenu.campanileSelection.Pressed += OnSelectCampanile;
		titleMenu.forestSelection.Pressed += OnSelectForest;
		//map预制体
		castle = GD.Load<PackedScene>("res://scene/Maps/Map_Castle.tscn");
		field = GD.Load<PackedScene>("res://scene/Maps/Map_Field.tscn");
		campanile = GD.Load<PackedScene>("res://scene/Maps/Map_Campanile.tscn");
		forest = GD.Load<PackedScene>("res://scene/Maps/Map_Forest.tscn");
	}
	public void LoadAnim()
	{
		(titleMenu.loadAnim.GetParent() as Node2D).Visible = true;
		titleMenu.loadAnim.Play("Load");
	}
	public void InitPlayers()
	{
		player.Visible = true;
		redKnight.isRight = true;
		blueKnight.isRight = false;
		redKnight.Modulate = new Color(1, 1, 1, 1);
		blueKnight.Modulate = new Color(1, 1, 1, 1);
	}
	public void OnSelectCastle()
	{
		LoadAnim();
		titleMenu.loadAnim.AnimationFinished += (Animnane) =>
		{
			if (Animnane == "Load")
			{
				titleMenu.loadAnim.Play("LoadFinished");
				//地图初始化
				titleMenu.mapSelect.Visible = false;
				TileMapLayer Map = castle.Instantiate() as TileMapLayer;
				GetNode<Node2D>("/root/Root/MapPool").AddChild(Map);
				Map.Position = new(0, 0);
				//玩家状态初始化
				redKnight.Position = new(-300, 31);
				blueKnight.Position = new(300, 31);
				InitPlayers();
				titleMenu.loadAnim.AnimationFinished += (anim) =>
				{
					redKnight.canControl = true;
					blueKnight.canControl = true;
				};
			}
		};
	}
	public void OnSelectField()
	{
		LoadAnim();
		titleMenu.loadAnim.AnimationFinished += (Animnane) =>
		{
			if (Animnane == "Load")
			{
				//地图初始化
				titleMenu.mapSelect.Visible = false;
				TileMapLayer Map = field.Instantiate() as TileMapLayer;
				GetNode<Node2D>("/root/Root/MapPool").AddChild(Map);
				Map.Position = new(0, 0);
				//玩家状态初始化
				InitPlayers();
				redKnight.Position = new(-360, 0);
				blueKnight.Position = new(360, 0);
				titleMenu.loadAnim.Play("LoadFinished");
				titleMenu.loadAnim.AnimationFinished += (anim) =>
				{
					redKnight.canControl = true;
					blueKnight.canControl = true;
				};
			}
		};
	}
	public void OnSelectCampanile()
	{
		LoadAnim();
		titleMenu.loadAnim.AnimationFinished += (Animnane) =>
		{
			if (Animnane == "Load")
			{
				//地图初始化
				titleMenu.mapSelect.Visible = false;
				TileMapLayer Map = campanile.Instantiate() as TileMapLayer;
				GetNode<Node2D>("/root/Root/MapPool").AddChild(Map);
				Map.Position = new(0, 0);
				//玩家状态初始化
				InitPlayers();
				redKnight.Position = new(-360, -80);
				blueKnight.Position = new(360, 80);
				redKnight.Modulate = new Color(0.7f, 0.7f, 0.7f, 1);
				blueKnight.Modulate = new Color(0.7f, 0.7f, 0.7f, 1);
				titleMenu.loadAnim.Play("LoadFinished");
				titleMenu.loadAnim.AnimationFinished += (anim) =>
				{
					redKnight.canControl = true;
					blueKnight.canControl = true;
				};
			}
		};
	}
	public void OnSelectForest()
	{
		LoadAnim();
		titleMenu.loadAnim.AnimationFinished += (Animnane) =>
		{
			if (Animnane == "Load")
			{
				//地图初始化
				titleMenu.mapSelect.Visible = false;
				TileMapLayer Map = forest.Instantiate() as TileMapLayer;
				GetNode<Node2D>("/root/Root/MapPool").AddChild(Map);
				Map.Position = new(0, 0);
				//玩家状态初始化
				InitPlayers();
				redKnight.Position = new(-360, 0);
				blueKnight.Position = new(360, 0);
				redKnight.Modulate = new Color(0.4f, 0.4f, 0.4f, 1);
				blueKnight.Modulate = new Color(0.4f, 0.4f, 0.4f, 1);
				titleMenu.loadAnim.Play("LoadFinished");
				titleMenu.loadAnim.AnimationFinished += (anim) =>
				{
					redKnight.canControl = true;
					blueKnight.canControl = true;
				};
			}
		};
	}
}
