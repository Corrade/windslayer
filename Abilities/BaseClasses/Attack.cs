using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An ability with hitboxes
public abstract class Attack : Ability
{
    [Tooltip("The maximum number of targets that this ability will hit")]
    public int MaxPierce;

    public int Pierced { get; set; }

    protected List<Hitbox> m_HitboxesToDestroyOnInterrupt;

    public override void Initialise(GameObject player)
    {
        Pierced = 0;
        m_HitboxesToDestroyOnInterrupt = new List<Hitbox>();
        base.Initialise(player);
    }

    public override void Interrupt()
    {
        base.Interrupt();

        foreach (Hitbox h in m_HitboxesToDestroyOnInterrupt) {
            if (!h.Destroyed) {
                h.Interrupt();
            }
        }
    }
}
