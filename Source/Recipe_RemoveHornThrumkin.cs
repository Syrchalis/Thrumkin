using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace SyrThrumkin
{
    internal class Recipe_RemoveHornThrumkin : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
            using (IEnumerator<BodyPartRecord> enumerator = parts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BodyPartRecord part = enumerator.Current;
                    if (part.def == ThrumkinDefOf.HornThrumkin)
                    {
                        yield return part;
                    }
                }
            }
            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            bool flag = pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part);
            bool flag2 = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                GenSpawn.Spawn(part.def.spawnThingOnRemoved, billDoer.Position, billDoer.Map, WipeMode.Vanish);
            }
            DamagePart(pawn, part);
            if (flag)
            {
                ApplyThoughts(pawn, billDoer);
            }
            if (flag2)
            {
                ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
            }
        }

        public virtual void DamagePart(Pawn pawn, BodyPartRecord part)
        {
            pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
        }
        public virtual void ApplyThoughts(Pawn pawn, Pawn billDoer)
        {
            if (pawn.Dead)
            {
                ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.OrganHarvesting);
                return;
            }
            ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
        }

        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            if (pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part))
            {
                return recipe.label; 
            }
            else
            {
                return "SyrThrumkinStealHorn".Translate();
            }    
        }

        public static void GiveThoughtsForPawnHornHarvested(Pawn victim)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            ThoughtDef thoughtDef = null;
            if (victim.IsColonist)
            {
                thoughtDef = ThrumkinDefOf.KnowColonistHornHarvested;
            }
            else if (victim.HostFaction == Faction.OfPlayer)
            {
                thoughtDef = ThrumkinDefOf.KnowOtherHornHarvested;
            }
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
            {
                if (pawn == victim)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThrumkinDefOf.MyHornHarvested, null);
                }
                else if (thoughtDef != null)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, null);
                }
            }
        }
    }
}
