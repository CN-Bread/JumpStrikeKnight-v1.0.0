using Godot;
using System;

public partial class TitleMenu : CanvasLayer
{
	#region 外部接入
	[ExportGroup("主菜单")]
	[Export] public Control main;
	[Export] public Panel title;
	[Export] public VBoxContainer buttonContainer;
	[Export] public Button PVP;
	[Export] public Button quit;
	[Export] public CheckButton isOnline;
	[ExportGroup("控件节点")]
	[Export] public Control mapSelect;
	[Export] public HBoxContainer mapRoll;
	[Export] public Button castleSelection;
	[Export] public Button fieldSelection;
	[Export] public Button campanileSelection;
	[Export] public Button forestSelection;
	[ExportGroup("动画节点")]
	[Export] public AnimationPlayer loadAnim;
	#endregion
	public override void _Ready()
	{
		//Main场景
		main.Visible = true;
		PVP.Pressed += OnPVPPressed;
		quit.Pressed += OnQuitPressed;
		//MapSelect场景
		mapSelect.Visible = false;
		castleSelection.Disabled = true;
		campanileSelection.Disabled = true;
		forestSelection.Disabled = true;
	}
    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void OnPVPPressed()
    {
        main.Visible = false;
		mapSelect.Visible = true;
		MapSelectionAble();
    }

	private void OnOnlineSwitch()
	{
		
	}
	private void MapSelectionAble()
	{
		castleSelection.Disabled = false;
		fieldSelection.Disabled = false;
		campanileSelection.Disabled = false;
		forestSelection.Disabled = false;
	}

}
