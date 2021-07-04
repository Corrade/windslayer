using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
/*
    primitives
        fix
            prevent all movement. at the conclusion of this status, if the player is airborne, they'll drop to the ground. that is, the status cancels out all velocity beforehand. (note: knockback may still take effect if this status is removed before knocking back)
        root
            prevent all player-generated movement (including from abilities)
        silence
            prevent all abilities
        disarm
            prevent all basic attacks
        invincibility
            the player cannot be damaged (even by dot effects applied before the invincibility), but may be healed and targeted; that is, knocked back, frozen, etc.
        intangible
            the player cannot be targeted (the player may still take damage with this status if they were suffering from dot before becoming intangible)
        invisibility
            the player is invisible, but their attacks and dirt skids etc. are still visible
        slow
            the player's ground and air movement speed is altered. this may or may not affect ability speed (design choice).
        confuse
            the player's left and right movement keybinds are swapped

    composites
        stun
            root + silence + disarm
        suspend
            fix + silence + disarm
        freeze
            fix + silence + disarm + intangible
*/
}
