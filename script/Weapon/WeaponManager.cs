using Godot;
using System;
using Godot.Collections;


public partial class WeaponManager : Node2D
{
    #region 武器相关声明
    public Timer hitStopTimer;
    private bool _lastFrameBlockSuccess = false;
    private bool hasTriggerStopTimer = false;
    [Export] public Array<WeaponBase> weapons = [];
    private int _currentWeaponIndex;
    public WeaponBase currentWeapon;

    private Sword sword;
    private Shield shield;
    private Bow bow;
    #endregion

    #region 动画相关声明
    [ExportGroup("动画接入")]
    [Export] public AnimatedSprite2D blockSuc;
    [Export] public AnimatedSprite2D swordSlash;
    [Export] public AnimatedSprite2D shieldKick;
    public bool isFacingRight;
    private bool _lastFrame;
    private bool _canFlip;
    private Vector2 originalposition;
    #endregion

    public override void _Ready()
    {
        Visible = true;
        //初始化名称绑定
        sword = weapons[0] as Sword;
        shield = weapons[1] as Shield;
        bow = weapons[2] as Bow;

        //基础初始化
        InitWeaponsInHand();
        originalposition = Position;
        _canFlip = true;
        blockSuc.Visible = false;
        swordSlash.Visible = false;
        shieldKick.Visible = false;

        //初始化信号连接
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].CanFlip += OnCanFlip;
            weapons[i].CanNotFlip += OnCanNotFlip;
        }

        blockSuc.AnimationFinished += () =>
        {
            blockSuc.Visible = false;
        };
        swordSlash.AnimationFinished += () =>
        {
            swordSlash.Visible = false;
        };
        shieldKick.AnimationFinished += () =>
        {
            shieldKick.Visible = false;
        };
        //计时器初始化
        hitStopTimer = new Timer
        {
            OneShot = true,
        };
        AddChild(hitStopTimer);
        hitStopTimer.Timeout += () =>
        {
            Engine.TimeScale = 1;
            hasTriggerStopTimer = false;
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        FlipFunc();
    }
    private void InitWeaponsInHand()
    {
        isFacingRight = true;
        _lastFrame = isFacingRight;

        _currentWeaponIndex = 0;
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].weaponTexture.Visible = false;
        }
        weapons[_currentWeaponIndex].weaponTexture.Visible = true;
        currentWeapon = weapons[_currentWeaponIndex];
    }

    private void FlipFunc()
    {
        if (_canFlip)
        {
            isFacingRight = (GetParent() as Player).isRight;

            if (_lastFrame != isFacingRight)
            {
                if (isFacingRight == false)
                {
                    Scale = new Vector2(-1, 1);
                    Position = new Vector2(-1 * originalposition.X, originalposition.Y);
                }
                else
                {
                    Scale = new Vector2(1, 1);
                    Position = new Vector2(originalposition.X, originalposition.Y);
                }

                //更新上一帧状态
                _lastFrame = isFacingRight;
            }
        }

    }

    public void TrySwitchWeapon()
    {
        if (!currentWeapon.canSwitchWeapon)
        {
            return;
        }

        if (_currentWeaponIndex == weapons.Count - 1)
        {
            _currentWeaponIndex = 0;
        }
        else
        {
            _currentWeaponIndex++;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].weaponTexture.Visible = false;
        }
        weapons[_currentWeaponIndex].weaponTexture.Visible = true;
        currentWeapon = weapons[_currentWeaponIndex];

        IsShieldRaised();
        IsBowInHand();
        WeaponInHandEffection();
    }
    public void TrySwitchWeapon(int code)
    {
        if (!currentWeapon.canSwitchWeapon)
        {
            return;
        }
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].weaponTexture.Visible = false;
        }
        _currentWeaponIndex = code;
        weapons[_currentWeaponIndex].weaponTexture.Visible = true;
        currentWeapon = weapons[_currentWeaponIndex];
        IsShieldRaised();
        IsBowInHand();
        WeaponInHandEffection();
    }
    private void IsShieldRaised()
    {
        if (currentWeapon is Shield)
        {
            shield.isRaisingShield = true;
            shield.DEFArea.Monitorable = true;
        }
        else
        {
            shield.isRaisingShield = false;
            shield.DEFArea.Monitorable = false;
        }
    }
    private void IsBowInHand()
    {
        if (currentWeapon == bow)
        {
            //使用bow自定义状态机
            bow.useBow = true;
            //初始化bow状态
            bow.ItCanFlip();
            bow.canSwitchWeapon = true;
            bow.bowTexture.Animation = "default";
            bow.isArrowOnBow = false;
            bow.isNeedAim = false;
            bow.hasCharged = false;
            bow.hasShot = false;
            bow.aimArea.Monitoring = false;
            bow.blockingArea.Monitoring = false;
            bow.blockingArea.Monitorable = false;
            bow.RotationDegrees = 45;
            bow.canBlock = true;
            bow.autoAim = false;
            bow.currentType = WeaponBase.ATKType.dft;
            bow.aimArea.Monitoring = false;
            bow.aimArea.Monitorable = false;
        }
        else
        {
            bow.useBow = false;
            bow.reloadArrow.Visible = false;

        }
    }
    private void WeaponInHandEffection()
    {
        switch (currentWeapon)
        {
            case Sword:
                (GetParent() as Player).moveSpeed = 50;
                (GetParent() as Player).maxMoveSpeed = 500;
                (GetParent() as Player).jumpForce = 500;
                break;
            case Shield:
                (GetParent() as Player).moveSpeed = 30;
                (GetParent() as Player).maxMoveSpeed = 520;
                (GetParent() as Player).jumpForce = 410;
                break;
            case Bow:
                (GetParent() as Player).moveSpeed = 55;
                (GetParent() as Player).maxMoveSpeed = 550;
                (GetParent() as Player).jumpForce = 511;
                break;
        }
    }
    public void HitStop(float time, bool isblockSuc = false)
    {
        if (isblockSuc)
        {
            blockSuc.Visible = true;
            blockSuc.GlobalPosition = (blockSuc.GetParent() as Node2D).GlobalPosition;
            blockSuc.Play();
            time += 0.005f;
        }
        if (!hasTriggerStopTimer && hitStopTimer.IsStopped())
        {
            hasTriggerStopTimer = true;
            hitStopTimer.Start(time);
            Engine.TimeScale = 0.1;
        }

        return;
    }

    public void SwordSlash(bool isFacingRight)
    {
        swordSlash.Visible = true;
        if (isFacingRight == true && !this.isFacingRight)
        {
            swordSlash.RotationDegrees = -135;
        }
        if (isFacingRight == false && !this.isFacingRight)
        {
            swordSlash.RotationDegrees = 45;
        }
        if (isFacingRight == true && this.isFacingRight)
        {
            swordSlash.RotationDegrees = 45;
        }
        if (isFacingRight == false && this.isFacingRight)
        {
            swordSlash.RotationDegrees = -135;
        }
        swordSlash.Play();
    }
    public void ShieldKick(bool isFacingRight)
    {
        shieldKick.Visible = true;
        if (isFacingRight == true && !this.isFacingRight)
        {
            shieldKick.RotationDegrees = -135;
        }
        if (isFacingRight == false && !this.isFacingRight)
        {
            shieldKick.RotationDegrees = 45;
        }
        if (isFacingRight == true && this.isFacingRight)
        {
            shieldKick.RotationDegrees = 45;
        }
        if (isFacingRight == false && this.isFacingRight)
        {
            shieldKick.RotationDegrees = -135;
        }
        shieldKick.Play();
    }

    #region 武器攻击调用
    public void Try1()
    {
        currentWeapon.OnT1();
    }
    public void Try2()
    {
        currentWeapon.OnT2();
    }
    public void TrySPC()
    {
        currentWeapon.OnSPC();
    }
    #endregion
    #region 信号相关函数
    void OnCanFlip()
    {
        _canFlip = true;
    }
    void OnCanNotFlip()
    {
        _canFlip = false;
    }
    #endregion

}