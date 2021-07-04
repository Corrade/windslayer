using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatManager : MonoBehaviour
{
    // Non-combat stats
    public int Level {
        get {
            return m_Level;
        }
        set {
            m_Level = value;
            OnLevelChange.Invoke();
        }
    }
    public float Exp {
        get {
            return m_Exp;
        }
        set {
            m_Exp = value;
            OnExpChange.Invoke();
        }
    }
    public int Gold {
        get {
            return m_Gold;
        }
        set {
            m_Gold = value;
            OnGoldChange.Invoke();
        }
    }
    public int Wsp {
        get {
            return m_Wsp;
        }
        set {
            m_Wsp = value;
            OnWspChange.Invoke();
        }
    }

    // Base stats
    public int Str {
        get {
            return m_Str;
        }
        set {
            m_Str = value;
            OnStrChange.Invoke();
        }
    }
    public int Dex {
        get {
            return m_Dex;
        }
        set {
            m_Dex = value;
            OnDexChange.Invoke();
        }
    }
    public int Int {
        get {
            return m_Int;
        }
        set {
            m_Int = value;
            OnIntChange.Invoke();
        }
    }
    public int Spr {
        get {
            return m_Spr;
        }
        set {
            m_Spr = value;
            OnSprChange.Invoke();
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
    public UnityEvent OnLevelChange;
    public UnityEvent OnExpChange;
    public UnityEvent OnGoldChange;
    public UnityEvent OnWspChange;

    public UnityEvent OnStrChange;
    public UnityEvent OnDexChange;
    public UnityEvent OnIntChange;
    public UnityEvent OnSprChange;

    public UnityEvent OnMaxHealthChange;
    public UnityEvent OnMaxManaChange;
    public UnityEvent OnPhyPowChange;
    public UnityEvent OnPhyDefChange;
    public UnityEvent OnMagPowChange;
    public UnityEvent OnMagDefChange;

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
        OnLevelChange.AddListener(SetMaxHealth);
        OnSprChange.AddListener(SetMaxHealth);

        OnLevelChange.AddListener(SetMaxMana);
        OnIntChange.AddListener(SetMaxMana);

        OnStrChange.AddListener(SetPhyPow);

        OnSprChange.AddListener(SetPhyDef);

        OnIntChange.AddListener(SetMagPow);

        OnDexChange.AddListener(SetMagDef);

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
        OnMaxHealthChange.Invoke();
    }

    void SetMaxMana()
    {
        MaxMana = 100 + 5*Level + 2*Int;
        OnMaxManaChange.Invoke();
    }

    void SetPhyPow()
    {
        PhyPow = 3*Str;
        OnPhyPowChange.Invoke();
    }

    void SetPhyDef()
    {
        PhyDef = 3*Spr;
        OnPhyDefChange.Invoke();
    }

    void SetMagPow()
    {
        MagPow = 3*Int;
        OnMagPowChange.Invoke();
    }

    void SetMagDef()
    {
        MagDef = 3*Dex; // Makes no sense
        OnMagDefChange.Invoke();
    }
}
