using Godot;
using System;

public partial class PlayerUI : Node2D
{
	#region 外部接入
	[ExportGroup("节点接入")]
	[Export] public Player player;
	[Export] public HBoxContainer heartContainer;
	[Export] public HBoxContainer armorContainer;
	[Export] public HBoxContainer arrowContainer;

	[ExportGroup("材质接入")]
	[Export] public Texture2D fullHeart;
	[Export] public Texture2D halfHeart;
	[Export] public Texture2D emptyHeart;
	[Export] public Texture2D fullArmor;
	[Export] public Texture2D halfArmor;
	[Export] public Texture2D haveArrow;
	[Export] public Texture2D withoutArrow;

	[ExportGroup("动画接入")]
	[Export] public AnimationPlayer lowHealth;
	#endregion
	public override void _Ready()
	{
		(heartContainer.GetChild(0) as TextureRect).Modulate = new Color(1,1,1,1);
	}
	public override void _PhysicsProcess(double delta)
	{
		HeartUpdate(player.HP);
		ArmorUpdate(player.armor);
		ArrowUpdate(player.bow.arrowLeft);
	}

	public void HeartUpdate(int HP)
	{
		int fullHearts = HP / 2;
        bool hasHalf = (HP % 2) == 1;
        if(HP <= 0)
		{
			heartContainer.Visible = false;
			armorContainer.Visible = false;
			player.armor = 0;
		}
		else
		{
			heartContainer.Visible = true;
		}
        // 遍历子节点
        for (int i = 0; i < heartContainer.GetChildCount(); i++)
        {
            TextureRect heart = heartContainer.GetChild(i) as TextureRect;
            
            if (i < fullHearts)
			{
				heart.Texture = fullHeart;
			}   
            else if (i == fullHearts && hasHalf)
			{
				heart.Texture = halfHeart;
			}
			else
			{
				heart.Texture = emptyHeart;
			}

			if (HP <= 1)
			{
				lowHealth.Play("LowHealth");
			}
			else
			{
				lowHealth.Stop();
				(heartContainer.GetChild(0) as TextureRect).Modulate = new Color(1,1,1,1);
			}
        }
	}

	public void ArmorUpdate(int armor)
	{
		int fullArmors = armor / 2;
        bool hasHalf = (armor % 2) == 1;
        
        // 遍历子节点
        for (int i = 0; i < armorContainer.GetChildCount(); i++)
        {
            TextureRect armorTexture = armorContainer.GetChild(i) as TextureRect;
            
            if (i < fullArmors)
			{
				armorTexture.Visible = true;
				armorTexture.Texture = fullArmor;
			}   
            else if (i == fullArmors && hasHalf)
			{
				armorTexture.Visible = true;
				armorTexture.Texture = halfArmor;
			}
			else
			{
				armorTexture.Visible = false;
			}
                   
        }
	}
	public void ArrowUpdate(int counts)
	{
		if(player.weaponMgr.currentWeapon is Bow)
		{
			arrowContainer.Visible = true;
		}
		else
		{
			arrowContainer.Visible = false;
		}
        // 遍历子节点
        for (int i = 0; i < arrowContainer.GetChildCount(); i++)
        {
            TextureRect arrowTexture = arrowContainer.GetChild(i) as TextureRect;
            
            if (i <= counts - 1)
			{
				arrowTexture.Texture = haveArrow;
			}   
            else if (i > counts - 1)
			{
				arrowTexture.Texture = withoutArrow;
			}
                   
        }
	}
}
