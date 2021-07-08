using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    // Non-combat stats
    public int Level {
        get {
            return m_Level;
        }
        set {
            m_Level = value;
            OnLevelChange?.Invoke(this, EventArgs.Empty);
        }
    }
    public float Exp {
        get {
            return m_Exp;
        }
        set {
            m_Exp = value;
            OnExpChange?.Invoke(this, EventArgs.Empty);
        }
    }
    public int Gold {
        get {
            return m_Gold;
        }
        set {
            m_Gold = value;
            OnGoldChange?.Invoke(this, EventArgs.Empty);
        }
    }
    public int Wsp {
        get {
            return m_Wsp;
        }
        set {
            m_Wsp = value;
            OnWspChange?.Invoke(this, EventArgs.Empty);
        }
    }

    // Base stats
    public int Str {
        get {
            return m_Str;
        }
        set {
            m_Str = value;
            OnStrChange?.Invoke(this, EventArgs.Empty);
        }
    }
    public int Dex {
        get {
            return m_Dex;
        }
        set {
            m_Dex = value;
            OnDexChange?.Invoke(this, EventArgs.Empty);
        }
    }
    public int Int {
        get {
            return m_Int;
        }
        set {
            m_Int = value;
            OnIntChange?.Invoke(this, EventArgs.Empty);
        }
    }
    public int Spr {
        get {
            return m_Spr;
        }
        set {
            m_Spr = value;
            OnSprChange?.Invoke(this, EventArgs.Empty);
        }
    }

    // Dynamic stats - these are determined by non-combat/base stats
    public int MaxHealth { get; private set; }
    public int MaxMana { get; private set; }
    public int PhyPow { get; private set; }
    public int PhyDef { get; private set; }
    public int MagPow { get; private set; }
    public int MagDef { get; private set; }

    // Events
    public event EventHandler OnLevelChange;
    public event EventHandler OnExpChange;
    public event EventHandler OnGoldChange;
    public event EventHandler OnWspChange;

    public event EventHandler OnStrChange;
    public event EventHandler OnDexChange;
    public event EventHandler OnIntChange;
    public event EventHandler OnSprChange;

    public event EventHandler OnMaxHealthChange;
    public event EventHandler OnMaxManaChange;
    public event EventHandler OnPhyPowChange;
    public event EventHandler OnPhyDefChange;
    public event EventHandler OnMagPowChange;
    public event EventHandler OnMagDefChange;

    // Backing fields
    int m_Level;
    float m_Exp;
    int m_Gold;
    int m_Wsp;

    int m_Str;
    int m_Dex;
    int m_Int;
    int m_Spr;

    void Start()
    {
        // Initialise non-combat/base stats
        Level = 1;
        Exp = 0f;
        Gold = 0;
        Wsp = 0;
        Str = 10;
        Dex = 10;
        Int = 10;
        Spr = 10;

        // Hook up dynamic stats to the events of stats they're based on
        OnLevelChange += (object sender, EventArgs e) => SetMaxHealth();
        OnSprChange += (object sender, EventArgs e) => SetMaxHealth();

        OnLevelChange += (object sender, EventArgs e) => SetMaxMana();
        OnIntChange += (object sender, EventArgs e) => SetMaxMana();

        OnStrChange += (object sender, EventArgs e) => SetPhyPow();
        OnSprChange += (object sender, EventArgs e) => SetPhyDef();
        OnIntChange += (object sender, EventArgs e) => SetMagPow();
        OnDexChange += (object sender, EventArgs e) => SetMagDef();

        // Initialise dynamic stats
        SetMaxHealth();
        SetMaxMana();
        SetPhyPow();
        SetPhyDef();
        SetMagPow();
        SetMagDef();
    }

    void SetMaxHealth()
    {
        MaxHealth = 100 + 5*Level + 2*Spr;
        OnMaxHealthChange?.Invoke(this, EventArgs.Empty);
    }

    void SetMaxMana()
    {
        MaxMana = 100 + 5*Level + 2*Int;
        OnMaxManaChange?.Invoke(this, EventArgs.Empty);
    }

    void SetPhyPow()
    {
        PhyPow = 3*Str;
        OnPhyPowChange?.Invoke(this, EventArgs.Empty);
    }

    void SetPhyDef()
    {
        PhyDef = 3*Spr;
        OnPhyDefChange?.Invoke(this, EventArgs.Empty);
    }

    void SetMagPow()
    {
        MagPow = 3*Int;
        OnMagPowChange?.Invoke(this, EventArgs.Empty);
    }

    void SetMagDef()
    {
        MagDef = 3*Dex; // Makes no sense
        OnMagDefChange?.Invoke(this, EventArgs.Empty);
    }
}
