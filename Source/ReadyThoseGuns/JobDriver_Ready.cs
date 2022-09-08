using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ReadyThoseGuns;

public class JobDriver_Ready : JobDriver
{
    public static bool GunNeedsRefueling(Building b)
    {
        if (b is not Building_TurretGun building_TurretGun)
        {
            return false;
        }

        var compRefuelable = building_TurretGun.TryGetComp<CompRefuelable>();
        return compRefuelable != null && !compRefuelable.HasFuel && compRefuelable.Props.fuelIsMortarBarrel &&
               !Find.Storyteller.difficulty.classicMortars;
    }

    public static bool GunNeedsLoading(Building b)
    {
        if (b is not Building_TurretGun building_TurretGun)
        {
            return false;
        }

        var compChangeableProjectile = building_TurretGun.gun.TryGetComp<CompChangeableProjectile>();
        return compChangeableProjectile != null && !compChangeableProjectile.Loaded;
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }


    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        var gotoTurret = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        var refuelIfNeeded = new Toil();
        refuelIfNeeded.initAction = delegate
        {
            var actor = refuelIfNeeded.actor;
            var building = (Building)actor.CurJob.targetA.Thing;
            var building_TurretGun = building as Building_TurretGun;
            if (!GunNeedsRefueling(building))
            {
                JumpToToil(gotoTurret);
                return;
            }

            var thing = JobDriver_ManTurret.FindFuelForTurret(pawn, building_TurretGun);
            if (thing == null)
            {
                var compRefuelable = building.TryGetComp<CompRefuelable>();
                if (actor.Faction == Faction.OfPlayer && compRefuelable != null)
                {
                    Messages.Message(
                        "MessageOutOfNearbyFuelFor".Translate(actor.LabelShort, building_TurretGun?.Label,
                            actor.Named("PAWN"), building_TurretGun.Named("GUN"),
                            compRefuelable.Props.fuelFilter.Summary.Named("FUEL")).CapitalizeFirst(),
                        building_TurretGun, MessageTypeDefOf.NegativeEvent);
                }

                actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            }

            actor.CurJob.targetB = thing;
            actor.CurJob.count = 1;
        };
        yield return refuelIfNeeded;
        yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
            .WithProgressBarToilDelay(TargetIndex.A);
        yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.B);
        var loadIfNeeded = new Toil();
        loadIfNeeded.initAction = delegate
        {
            var actor = loadIfNeeded.actor;
            var building = (Building)actor.CurJob.targetA.Thing;
            var building_TurretGun = building as Building_TurretGun;
            if (!GunNeedsLoading(building))
            {
                JumpToToil(gotoTurret);
                return;
            }

            var thing = JobDriver_ManTurret.FindAmmoForTurret(pawn, building_TurretGun);
            if (thing == null)
            {
                if (actor.Faction == Faction.OfPlayer)
                {
                    Messages.Message(
                        "MessageOutOfNearbyShellsFor".Translate(actor.LabelShort, building_TurretGun?.Label,
                            actor.Named("PAWN"), building_TurretGun.Named("GUN")).CapitalizeFirst(), building_TurretGun,
                        MessageTypeDefOf.NegativeEvent);
                }

                actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            }

            actor.CurJob.targetB = thing;
            actor.CurJob.count = 1;
        };
        yield return loadIfNeeded;
        yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        var loadShell = new Toil();
        loadShell.initAction = delegate
        {
            var actor = loadShell.actor;
            if ((Building)actor.CurJob.targetA.Thing is Building_TurretGun building_TurretGun)
            {
                SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(building_TurretGun.Position,
                    building_TurretGun.Map));
                building_TurretGun.gun.TryGetComp<CompChangeableProjectile>()
                    .LoadShell(actor.CurJob.targetB.Thing.def, 1);
            }

            actor.carryTracker.innerContainer.ClearAndDestroyContents();
        };
        yield return loadShell;
        yield return gotoTurret;
        var ready = new Toil();
        ready.tickAction = delegate
        {
            var actor = ready.actor;
            var building = (Building)actor.CurJob.targetA.Thing;
            if (GunNeedsLoading(building))
            {
                JumpToToil(loadIfNeeded);
                return;
            }

            if (GunNeedsRefueling(building))
            {
                JumpToToil(refuelIfNeeded);
                return;
            }

            ready.actor.rotationTracker.FaceCell(building.Position);
            var building_TurretGun = (Building_TurretGun)building;
            if (!Main.ProgressComplete(building_TurretGun))
            {
                return;
            }

            EndJobWith(JobCondition.Succeeded);
        };
        ready.handlingFacing = true;
        ready.defaultCompleteMode = ToilCompleteMode.Never;
        ready.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        yield return ready;
    }
}