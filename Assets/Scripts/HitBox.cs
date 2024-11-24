using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public EnemyBot bot;
    public void OnRaycastHit(MainGun gun, Vector3 direction) {
        bot.TakeDamage(gun.damage, direction);
    }
}
