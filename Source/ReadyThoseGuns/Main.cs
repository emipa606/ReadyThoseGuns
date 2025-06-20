using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReadyThoseGuns;

[StaticConstructorOnStartup]
public static class Main
{
    public static readonly FieldInfo CoolDownFieldInfo;
    private static readonly FieldInfo progressBarEffecterFieldInfo;

    static Main()
    {
        CoolDownFieldInfo = AccessTools.Field(typeof(Building_TurretGun), "burstCooldownTicksLeft");
        progressBarEffecterFieldInfo = AccessTools.Field(typeof(Building_TurretGun), "progressBarEffecter");
    }

    public static bool ProgressComplete(Building_TurretGun turretGun)
    {
        var currentCooldown = (int)CoolDownFieldInfo.GetValue(turretGun);
        var progressBarEffecter = (Effecter)progressBarEffecterFieldInfo.GetValue(turretGun);
        currentCooldown--;
        CoolDownFieldInfo.SetValue(turretGun, currentCooldown);

        if (currentCooldown <= 0)
        {
            if (progressBarEffecter == null)
            {
                return true;
            }

            progressBarEffecter.Cleanup();
            progressBarEffecterFieldInfo.SetValue(turretGun, null);

            return true;
        }

        var totalTime = turretGun.def.building.turretBurstCooldownTime;
        if (totalTime == 0f)
        {
            totalTime = turretGun.AttackVerb.verbProps.defaultCooldownTime;
        }

        progressBarEffecter ??= EffecterDefOf.ProgressBar.Spawn();

        progressBarEffecter.EffectTick(turretGun, TargetInfo.Invalid);
        var mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
        mote.progress = 1f - (Math.Max(currentCooldown, 0) / (float)totalTime.SecondsToTicks());
        mote.offsetZ = -0.8f;
        progressBarEffecterFieldInfo.SetValue(turretGun, progressBarEffecter);
        return false;
    }

    public static void LogMessage(string message)
    {
#if DEBUG
        Log.Message($"[ReadyThoseGuns]: {message}");
#endif
    }
}