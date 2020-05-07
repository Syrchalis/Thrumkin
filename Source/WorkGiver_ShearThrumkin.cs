using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;

namespace SyrThrumkin
{
    public class WorkGiver_ShearThrumkin : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef
        {
            get
            {
                return ThrumkinDefOf.ShearThrumkin;
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompShearable>();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Pawn> pawns = pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i] != pawn)
                {
                    yield return pawns[i];
                }
            }
            yield break;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Pawn> list = pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].def == ThrumkinDefOf.Thrumkin)
                {
                    CompHasGatherableBodyResource comp = GetComp(list[i]);
                    if (comp != null && comp.ActiveAndFull)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || pawn2.def != ThrumkinDefOf.Thrumkin)
            {
                return false;
            }
            CompHasGatherableBodyResource comp = GetComp(pawn2);
            if (comp != null && comp.ActiveAndFull && !pawn2.Downed && pawn2.CanCasuallyInteractNow(false) && pawn.CanReserve(pawn2, 1, -1, null, forced))
            {
                return true;
            }
            if (pawn2 != null && comp.ActiveAndFull && pawn2.CanCasuallyInteractNow(false) && pawn.CanReserve(pawn2, 1, -1, null, forced) && pawn2.IsPrisonerOfColony && pawn2.guest.PrisonerIsSecure && pawn2.Spawned && !pawn2.InAggroMentalState && !t.IsForbidden(pawn) && !pawn2.IsFormingCaravan() && pawn.CanReserveAndReach(pawn2, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1, -1, null, false))
            {
                return true;
            }
            return false;
        }
    }
}
