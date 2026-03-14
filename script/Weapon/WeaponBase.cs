using Godot;
using System;
using System.Diagnostics.CodeAnalysis;

public abstract partial class WeaponBase : Node2D
{
	#region 状态相关声明
	public enum States {CanATK,CanNotATK,Prepare,ATKing,Recovery,CoolDown};
	public enum ATKType{dft,T1,T2,SPC,Reload}
	public States currentStates;
	public ATKType currentType;
	public bool canSwitchWeapon;
    protected Timer timer;
	public bool useBow;
	#endregion
	#region 材质相关声明
	[Export] public AnimatedSprite2D weaponTexture;
	#endregion
	#region 信号声明
	[Signal]public delegate void CanNotFlipEventHandler();
	[Signal]public delegate void CanFlipEventHandler();
	#endregion

	#region 攻击函数调用
	public void OnT1()
	{
		TryStartATK(ATKType.T1);
	}
    public void OnT2()
	{
		TryStartATK(ATKType.T2);
	}
    public void OnSPC()
	{
		TryStartATK(ATKType.SPC);
	}
	#endregion

	public override void _Ready()
	{
		InitBaseic();
	}

	protected void InitBaseic()
	{
		currentStates = States.CanATK;
		canSwitchWeapon = true;
		SetupTimer();
	}
	private void SetupTimer()
    {
		if (!useBow)
		{
			timer = new Timer
			{
				OneShot = true
			};
			timer.Timeout += OnPhaseEnd;
			//AddChild加入节点树才能工作
			AddChild(timer);
		}
    }
	public void TryStartATK(ATKType type)
	{
		currentType = type;
		if(currentStates != States.CanATK)
		{
			return;
		}

		if(!useBow)
		{
			EnterPrepare(currentType,GetPrepareDuration(currentType));
		}
		else
		{
			EnterBowFlow(currentType);
		}

	}

	#region 攻击状态机
	void OnPhaseEnd()
	{
		switch (currentStates)
		{
			case States.CanNotATK:
				CanNotATKAction(GetCanNotATKDuration());
				break;

			case States.Prepare:
				EnterATKing(currentType,GetATKDuration(currentType));
				break;
			case States.ATKing:
				EnterRecovery(currentType,GetRecoveryDuration(currentType));
				break;
			case States.Recovery:
				EnterCoolDown(currentType,GetCoolDownDuration(currentType));
				break;
			case States.CoolDown:
				ATKComplete();
				break;
		}
	}

    public virtual void CanNotATKAction(float Duration){}
    public virtual void EnterPrepare(ATKType type,float prepareDuration)
	{
		PrepareAction(type);
		timer.Start(prepareDuration);
		currentStates = States.Prepare;
	}
    public abstract void PrepareAction(ATKType type);

	public virtual void EnterATKing(ATKType type,float ATKDuration)
	{
		currentStates = States.ATKing;
		ATKingAction(type);
		timer.Start(ATKDuration);
	}
	public abstract void ATKingAction(ATKType type);

	public virtual void EnterRecovery(ATKType type,float recoveryDuration)
	{
		currentStates = States.Recovery;
		RecoveryAction(type);
		timer.Start(recoveryDuration);
	}
	public abstract void RecoveryAction(ATKType type);

	public virtual void EnterCoolDown(ATKType type,float coolDownDuration)
	{
		currentStates = States.CoolDown;
		CoolDownAction(type);
		timer.Start(coolDownDuration);
	}

	public abstract void CoolDownAction(ATKType type);
	protected abstract float GetPrepareDuration(ATKType type);
    protected abstract float GetATKDuration(ATKType type);
    protected abstract float GetRecoveryDuration(ATKType type);
	protected abstract float GetCoolDownDuration(ATKType type);
	protected abstract float GetCanNotATKDuration();

	public void ATKComplete()
	{
		currentStates = States.CanATK;
		currentType = ATKType.dft;
		canSwitchWeapon = true;
	}
	
	public virtual void EnterBowFlow(ATKType type){}
	#endregion

	#region 信号相关函数
	public void ItCanFlip()
	{
		EmitSignal(SignalName.CanFlip);
	}
	public void ItCanNotFlip()
	{
		EmitSignal(SignalName.CanNotFlip);
	}
	#endregion

}