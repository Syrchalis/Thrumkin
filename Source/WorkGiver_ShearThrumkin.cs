using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace SyrThrumkin
{
    public class WorkGiver_ShearThrumkin : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef
        {
            get
            {
                return JobDefOf.Shear;
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompShearable>();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Pawn> pawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i] != pawn)
                {
                    yield return pawns[i];
                }
            }
            yield break;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null && pawn != pawn2)
            {
                return false;
            }
            CompHasGatherableBodyResource comp = this.GetComp(pawn2);
            if (comp != null && comp.ActiveAndFull && !pawn2.Downed && pawn2.CanCasuallyInteractNow(false))
            {
                LocalTargetInfo target = pawn2;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
