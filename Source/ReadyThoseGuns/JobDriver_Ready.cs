using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ReadyThoseGuns;

public class JobDriver_Ready : JobDriver
{
    public static bool GunNeedsRefueling(Building building)
    {
        if (building is not Building_TurretGun buildingTurretGun)
        {
            return false;
        }

        var compRefuelable = buildingTurretGun.TryGetComp<CompRefuelable>();
        return compRefuelable is { HasFuel: false } && compRefuelable.Props.fuelIsMortarBarrel &&
               !Find.Storyteller.difficulty.classicMortars;
    }

    public static bool GunNeedsLoading(Building building)
    {
        if (building is not Building_TurretGun buildingTurretGun)
        {
            return false;
        }

        var compChangeableProjectile = buildingTurretGun.gun.TryGetComp<CompChangeableProjectile>();
        return compChangeableProjectile is { Loaded: false };
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
            var buildingTurretGun = building as Building_TurretGun;
            if (!GunNeedsRefueling(building))
            {
                JumpToToil(gotoTurret);
                return;
            }

            var thing = JobDriver_ManTurret.FindFuelForTurret(pawn, buildingTurretGun);
            if (thing == null)
            {
                var compRefuelable = building.TryGetComp<CompRefuelable>();
                if (actor.Faction == Faction.OfPlayer && compRefuelable != null)
                {
                    Messages.Message(
                        "MessageOutOfNearbyFuelFor".Translate(actor.LabelShort, buildingTurretGun?.Label,
                            actor.Named("PAWN"), buildingTurretGun.Named("GUN"),
                            compRefuelable.Props.fuelFilter.Summary.Named("FUEL")).CapitalizeFirst(),
                        buildingTurretGun, MessageTypeDefOf.NegativeEvent);
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
            var buildingTurretGun = building as Building_TurretGun;
            if (!GunNeedsLoading(building))
            {
                JumpToToil(gotoTurret);
                return;
            }

            var thing = JobDriver_ManTurret.FindAmmoForTurret(pawn, buildingTurretGun);
            if (thing == null)
            {
                if (actor.Faction == Faction.OfPlayer)
                {
                    Messages.Message(
                        "MessageOutOfNearbyShellsFor".Translate(actor.LabelShort, buildingTurretGun?.Label,
                            actor.Named("PAWN"), buildingTurretGun.Named("GUN")).CapitalizeFirst(), buildingTurretGun,
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
            if ((Building)actor.CurJob.targetA.Thing is Building_TurretGun buildingTurretGun)
            {
                SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(buildingTurretGun.Position,
                    buildingTurretGun.Map));
                buildingTurretGun.gun.TryGetComp<CompChangeableProjectile>()
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
            var buildingTurretGun = (Building_TurretGun)building;
            if (!Main.ProgressComplete(buildingTurretGun))
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