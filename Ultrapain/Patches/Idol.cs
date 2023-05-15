using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class Idol_Death_Patch
    {
        static void Postfix(Idol __instance)
        {
            if(ConfigManager.idolExplodionType.value == ConfigManager.IdolExplosionType.Sand)
            {
                GameObject.Instantiate(Plugin.sandExplosion, __instance.transform.position, Quaternion.identity);
                return;
            }

            GameObject tempExplosion = null;
            switch(ConfigManager.idolExplodionType.value)
            {
                case ConfigManager.IdolExplosionType.Normal:
                    tempExplosion = Plugin.explosion;
                    break;

                case ConfigManager.IdolExplosionType.Big:
                    tempExplosion = Plugin.bigExplosion;
                    break;

                case ConfigManager.IdolExplosionType.Ligthning:
                    tempExplosion = Plugin.lightningStrikeExplosive;
                    break;
            }

            GameObject explosion = GameObject.Instantiate(tempExplosion, __instance.gameObject.transform.position, Quaternion.identity);
            foreach (Explosion exp in explosion.GetComponentsInChildren<Explosion>())
            {
                exp.enemy = true;
                exp.canHit = AffectedSubjects.All;
                exp.hitterWeapon = "";
                exp.maxSize *= ConfigManager.idolExplosionSizeMultiplier.value;
                exp.speed *= ConfigManager.idolExplosionSizeMultiplier.value;
                exp.damage = (int)(exp.damage * ConfigManager.idolExplosionDamageMultiplier.value);
            }
        }
    }
}
