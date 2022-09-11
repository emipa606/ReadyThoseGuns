using RimWorld;
using Verse;
using Verse.AI;

namespace ReadyThoseGuns;

public class WorkGiver_Ready : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override PathEndMode PathEndMode => PathEndMode.Touch;


    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!pawn.CanReserve(t, 1, -1, null, forced))
        {
            Main.LogMessage($"{pawn} can not reserve {t}");
            return false;
        }

        if (t is not Building building)
        {
            Main.LogMessage($"{t} is not a building");
            return false;
        }

        if (!building.def.building?.IsMortar == true)
        {
            Main.LogMessage($"{building} is not a mortar");
            return false;
        }

        if (building.Faction != Faction.OfPlayerSilentFail)
        {
            Main.LogMessage($"{building} is not ours");
            return false;
        }

        if (building.GetComp<CompMannable>() == null)
        {
            Main.LogMessage($"{building} is not mannable");
            return false;
        }

        if (building is not Building_TurretGun building_TurretGun)
        {
            Main.LogMessage($"{building} is not a Building_TurretGun");
            return false;
        }

        if (JobDriver_Ready.GunNeedsLoading(building_TurretGun))
        {
            var thereIsAmmo = JobDriver_ManTurret.FindAmmoForTurret(pawn, building_TurretGun) != null;
            if (!thereIsAmmo)
            {
                JobFailReason.Is("RDG.OutOfAmmo".Translate());
            }

            return thereIsAmmo;
        }

        if (JobDriver_Ready.GunNeedsRefueling(building_TurretGun))
        {
            var thereIsFuel = JobDriver_ManTurret.FindFuelForTurret(pawn, building_TurretGun) != null;
            if (!thereIsFuel)
            {
                JobFailReason.Is("RDG.OutOfFuel".Translate());
            }

            return thereIsFuel;
        }

        if ((int)Main.CoolDownFieldInfo.GetValue(building_TurretGun) > 0)
        {
            return true;
        }

        JobFailReason.Is("RDG.NotOnCooldown".Translate());
        Main.LogMessage($"{building_TurretGun} is not on cooldown");
        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(JobDefOf.RTG_Ready, t);
    }
}