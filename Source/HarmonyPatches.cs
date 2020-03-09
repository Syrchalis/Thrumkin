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
using AlienRace;

namespace SyrThrumkin
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            SyrThrumkinCore.ApplySettings();
        }
    }

    [HarmonyPatch(typeof(PlantUtility), nameof(PlantUtility.GrowthSeasonNow))]
    public static class GrowthSeasonNowPatch
    {
        [HarmonyPostfix]
        public static void GrowthSeasonNow_Postfix(ref bool __result, IntVec3 c, Map map, bool forSowing = false)
        {
            IPlantToGrowSettable plantToGrowSettable = c.GetPlantToGrowSettable(map);
            if (plantToGrowSettable == null)
            {
                __result = false;
            }
            else
            {
                ThingDef plant = plantToGrowSettable.GetPlantDefToGrow();
                if (plant == ThrumkinDefOf.Plant_Frostleaf)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class PreApplyDamagePatch
    {
        [HarmonyPostfix]
        public static void PreApplyDamage_Postfix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (__instance.def == ThrumkinDefOf.Thrumkin)
            {
                dinfo.SetAmount(dinfo.Amount * 0.8f);
            }
        }
    }

    [HarmonyPatch(typeof(Hediff_Injury), nameof(Hediff_Injury.Heal))]
    public static class HealPatch
    {
        [HarmonyPrefix]
        public static void Heal_Prefix(ref float amount, Hediff_Injury __instance)
        {
            if (__instance?.pawn?.def != null)
            {
                if (__instance.pawn.def == ThrumkinDefOf.Thrumkin)
                {
                    amount *= 2;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RaceProperties), nameof(RaceProperties.CanEverEat), new[] { typeof(ThingDef) })]
    public static class CanEverEatPatch
    {
        [HarmonyPrefix]
        public static bool CanEverEat_Prefix(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (__instance.body == ThrumkinDefOf.Thrumkin_Body && (t == ThingDefOf.WoodLog || t == ThingDefOf.Hay))
            {
                __result = true;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef) })]
    public class GenerateQualityCreatedByPawnPatch
    {
        [HarmonyPostfix]
        public static void GenerateQualityCreatedByPawn_Postfix(ref QualityCategory __result, Pawn pawn, SkillDef relevantSkill)
        {
            if (pawn.def == ThrumkinDefOf.Thrumkin)
            {
                float roll = Rand.Value;
                if (roll < 0.5f)
                {
                    __result -= 1;
                }
                else if (roll > 0.99f)
                {
                    __result += 1;
                }
                if (__result < QualityCategory.Awful)
                {
                    __result = QualityCategory.Awful;
                }
                else if (__result > QualityCategory.Legendary)
                {
                    __result = QualityCategory.Legendary;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), "IngestedCalculateAmounts")]
    public class IngestedCalculateAmountsPatch
    {
        [HarmonyPostfix]
        public static void IngestedCalculateAmounts_Postfix(Thing __instance, Pawn ingester, float nutritionWanted, ref float nutritionIngested)
        {
            if (ingester?.def != null && __instance?.def?.ingestible != null && ingester.def == ThrumkinDefOf.Thrumkin)
            {
                CompIngredients compIngr = __instance.TryGetComp<CompIngredients>();
                if (compIngr?.ingredients != null)
                {
                    bool meat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) != FoodTypeFlags.None);
                    bool animalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None);
                    bool nonMeat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) == FoodTypeFlags.None);
                    bool nonAnimalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) == FoodTypeFlags.None);
                    if (meat && animalP)
                    {
                        return;
                    }
                    else if (animalP && !nonAnimalP)
                    {
                        nutritionIngested *= 2.0f;
                    }
                    else if (meat && !nonMeat)
                    {
                        nutritionIngested *= 0.2f;
                    }
                    else if (animalP && nonAnimalP)
                    {
                        nutritionIngested *= 1.5f;
                    }
                    else if (meat && nonMeat)
                    {
                        nutritionIngested *= 0.6f;
                    }
                }
                else
                {
                    if ((__instance.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 0.2f;
                    }
                    else if ((__instance.def.ingestible.foodType & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 2.0f;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    public class IngestedPatch
    {
        [HarmonyPostfix]
        public static void Ingested_Postfix(Thing __instance, Pawn ingester, float nutritionWanted)
        {
            if (ingester?.def != null)
            {
                CompIngredients compIngr = __instance.TryGetComp<CompIngredients>();
                if (compIngr?.ingredients != null)
                {
                    if (compIngr.ingredients.Contains(ThrumkinDefOf.RawFrostleaves))
                    {
                        ingester.health.AddHediff(ThrumkinDefOf.AteFrostLeaves);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    public static class FoodOptimalityPatch
    {
        [HarmonyPostfix]
        public static void FoodOptimality_Postfix(ref float __result, Pawn eater, Thing foodSource)
        {
            if (eater?.def != null && foodSource?.def?.ingestible != null && eater.def == ThrumkinDefOf.Thrumkin)
            {
                CompIngredients compIngr = foodSource.TryGetComp<CompIngredients>();
                if (compIngr?.ingredients != null)
                {
                    bool meat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) != FoodTypeFlags.None);
                    bool animalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None);
                    bool nonMeat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) == FoodTypeFlags.None);
                    bool nonAnimalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) == FoodTypeFlags.None);
                    if (meat && animalP)
                    {
                        return;
                    }
                    else if (animalP && !nonAnimalP)
                    {
                        __result += 40;
                    }
                    else if (meat && !nonMeat)
                    {
                        __result -= 50;
                    }
                    else if (animalP && nonAnimalP)
                    {
                        __result += 20;
                    }
                    else if (meat && nonMeat)
                    {
                        __result -= 10;
                    }
                }
                else
                {
                    if ((foodSource.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        __result -= 100;
                    }
                    else if ((foodSource.def.ingestible.foodType & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None)
                    {
                        __result += 30;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.TryFindBestFoodSourceFor))]
    public class TryFindBestFoodSourceForPatch
    {
        [HarmonyPostfix]
        public static void TryFindBestFoodSourceFor_Postfix(ref bool __result, Pawn getter, Pawn eater, bool desperate, ref Thing foodSource, ref ThingDef foodDef)
        {
            if (foodSource == null && foodDef == null && eater?.def != null && eater.def == ThrumkinDefOf.Thrumkin)
            {
                ThingRequest thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
                Predicate<Thing> validator = t => t.def == ThingDefOf.WoodLog || t.def == ThingDefOf.Hay;
                Thing bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.Touch, TraverseParms.For(getter), 9999f, validator);
                foodSource = bestThing;
                foodDef = FoodUtility.GetFinalIngestibleDef(bestThing);
                __result = true;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_SharedBed), "CurrentStateInternal")]
    public class ThoughtWorker_SharedBedPatch
    {
        [HarmonyPostfix]
        public static void ThoughtWorker_SharedBed_Postfix(ref ThoughtState __result, Pawn p)
        {
            if (p != null && LovePartnerRelationUtility.GetMostDislikedNonPartnerBedOwner(p)?.def != null)
            {
                if (p.def == ThrumkinDefOf.Thrumkin)
                {
                    __result = false;
                }
                if (LovePartnerRelationUtility.GetMostDislikedNonPartnerBedOwner(p).def == ThrumkinDefOf.Thrumkin)
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetPartnerInMyBed))]
    public class GetPartnerInMyBedPatch
    {
        [HarmonyPostfix]
        public static void GetPartnerInMyBed_Postfix(ref Pawn __result, Pawn pawn)
        {
            Building_Bed building_Bed = pawn.CurrentBed();
            if (pawn != null && building_Bed != null)
            {
                foreach (Pawn pawn2 in building_Bed.CurOccupants)
                {
                    if (pawn2 != pawn)
                    {
                        if (pawn.def == ThrumkinDefOf.Thrumkin || pawn2.def == ThrumkinDefOf.Thrumkin)
                        {
                            __result = pawn2;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
    public class GetLovinMtbHoursPatch
    {
        [HarmonyPostfix]
        public static void GetLovinMtbHours_Postfix(ref float __result, Pawn pawn, Pawn partner)
        {
            if (pawn?.def != null && partner?.def != null)
            {
                if (pawn.def == ThrumkinDefOf.Thrumkin && partner.def == ThrumkinDefOf.Thrumkin)
                {
                    __result = 12f;
                }
                else if (pawn.def == ThrumkinDefOf.Thrumkin || partner.def == ThrumkinDefOf.Thrumkin)
                {
                    __result = 24f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
    public static class InteractionWorker_RomanceAttemptPatch
    {
        [HarmonyPrefix]
        public static bool RandomSelectionWeight_Prefix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (initiator.def == ThrumkinDefOf.Thrumkin)
            {
                __result = 0f;
                return false;
            }
            else if (recipient.def == ThrumkinDefOf.Thrumkin)
            {
                __result = 0.1f;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    public static class RenderPawnInternalPatch
    {
        [HarmonyPrefix]
        public static void RenderPawnInternal_Prefix(PawnRenderer __instance, Vector3 rootLoc, float angle, ref bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            ThingDef thingDef = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>().def;
            if (thingDef != null && thingDef == ThrumkinDefOf.Thrumkin && !renderBody)
            {
                renderBody = true;
            }
        }
    }

    [HarmonyPatch(typeof(Faction), nameof(Faction.GenerateNewLeader))]
    public static class GenerateNewLeaderPatch
    {
        public static bool PrefixRunning = false;

        [HarmonyPrefix]
        public static bool GenerateNewLeader_Prefix(Faction __instance)
        {
            bool spawnable = Current.Game.GetComponent<GameComponent_MenardySpawnable>().MenardySpawnable;
            if (spawnable && __instance?.def != null && __instance.def == ThrumkinDefOf.ThrumkinTribe && Rand.Value > 0.5f)
            {
                __instance.leader = null;
                PrefixRunning = true;
                PawnGenerationRequest request = new PawnGenerationRequest(ThrumkinDefOf.Thrumkin_ElderMelee, __instance, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, true, 1f,
                    false, true, true, false, false, false, false, false, 0, null, 1, null, null, null, null, null, null, null, Gender.Female, null, null, null, null);
                Pawn pawn;
                int i = 0;
                do
                {
                    pawn = PawnGenerator.GeneratePawn(request);
                    i++;
                    if (pawn.story.childhood == ThrumkinDefOf.Thrumkin_CBio_Menardy.backstory && pawn.story.adulthood == ThrumkinDefOf.Thrumkin_ABio_Menardy.backstory)
                    {
                        break;
                    }
                } while (i < 1000);

                if (i >= 1000)
                {
                    Log.Warning("Thrumkin - Generated too many pawns when attempting to get specific backstory");
                }
                pawn.Name = ThrumkinDefOf.Thrumkin_Bio_Menardy.name;
                pawn.story.hairDef = ThrumkinDefOf.Thrumkin_Hair_3;
                __instance.leader = pawn;
                Current.Game.GetComponent<GameComponent_MenardySpawnable>().MenardySpawnable = false;
                PrefixRunning = false;
                return false;
            }
            else { return true; }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static class TryGenerateNewPawnInternalPatch
    {
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void TryGenerateNewPawnInternal_Postfix(ref Pawn __result, ref PawnGenerationRequest request)
        {
            if (__result?.story?.childhood != null && __result?.story?.adulthood != null)
            {
                if (!GenerateNewLeaderPatch.PrefixRunning && (__result.story.childhood == ThrumkinDefOf.Thrumkin_CBio_Menardy.backstory || __result.story.adulthood == ThrumkinDefOf.Thrumkin_ABio_Menardy.backstory))
                {
                    __result = null;
                }
            }
            if (__result?.def != null && __result?.story?.hairDef != null)
            {
                if (__result.def != ThrumkinDefOf.Thrumkin)
                {
                    if (__result.story.hairDef == ThrumkinDefOf.Thrumkin_Hair_0)
                    {
                        __result = null;
                    }
                    else if (__result.story.hairDef == ThrumkinDefOf.Thrumkin_Hair_1)
                    {
                        __result = null;
                    }
                    else if (__result.story.hairDef == ThrumkinDefOf.Thrumkin_Hair_2)
                    {
                        __result = null;
                    }
                    else if (__result.story.hairDef == ThrumkinDefOf.Thrumkin_Hair_3)
                    {
                        __result = null;
                    }
                    else if (__result.story.hairDef == ThrumkinDefOf.Thrumkin_Hair_4)
                    {
                        __result = null;
                    }
                    else if (__result.story.hairDef == ThrumkinDefOf.Thrumkin_Hair_5)
                    {
                        __result = null;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.PostProcessGeneratedGear))]
    public static class PostProcessGeneratedGearPatch
    {
        [HarmonyPostfix]
        public static void PostProcessGeneratedGear_Postfix(Thing gear, Pawn pawn)
        {
            if (pawn?.story?.childhood != null && pawn?.story?.adulthood != null && gear.def != null)
            {
                if ((gear.def == ThrumkinDefOf.Apparel_PlateArmor || gear.def == ThrumkinDefOf.Apparel_TribalA || gear.def.IsMeleeWeapon) && 
                    (pawn.story.childhood == ThrumkinDefOf.Thrumkin_CBio_Menardy.backstory || pawn.story.adulthood == ThrumkinDefOf.Thrumkin_ABio_Menardy.backstory) &&
                    pawn.Faction != Faction.OfPlayerSilentFail)
                {
                    if (gear.def == ThrumkinDefOf.Apparel_TribalA)
                    {
                        gear.SetStuffDirect(ThrumkinDefOf.DevilstrandCloth);
                        gear.HitPoints = 130;
                        gear.SetColor(new Color(0.7f, 0.23f, 0.23f));
                    }
                    else
                    {
                        gear.SetStuffDirect(ThingDefOf.Plasteel);
                        gear.HitPoints = (gear.def.IsMeleeWeapon) ? 280 : 810;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.Interacted))]
    public static class InteractionWorker_RecruitAttemptPatch
    {
        [HarmonyPostfix]
        public static void InteractionWorker_RecruitAttempt_Postfix(Pawn initiator, Pawn recipient)
        {
            if (initiator?.def != null && recipient?.def != null && initiator.def == ThrumkinDefOf.Thrumkin && recipient.def == ThingDefOf.Thrumbo && recipient.Faction == null)
            {
                if (Rand.Chance(0.25f))
                {
                    InteractionWorker_RecruitAttempt.DoRecruit(initiator, recipient, 0.25f, false);
                    string text = recipient.KindLabelIndefinite();
                    if (recipient.Name != null)
                    {
                        Messages.Message("ThrumkinThrumboTameNameSuccess".Translate(
                            initiator.LabelShort,
                            text,
                            recipient.Name.ToStringFull
                        ).AdjustedFor(recipient, "PAWN"), recipient, MessageTypeDefOf.PositiveEvent, true);
                    }
                    else
                    {
                        Messages.Message("ThrumkinThrumboTameSuccess".Translate(
                            initiator.LabelShort,
                            text
                        ), recipient, MessageTypeDefOf.PositiveEvent, true);
                    }
                    if (initiator.Spawned && recipient.Spawned)
                    {
                        MoteMaker.ThrowText(((initiator.DrawPos + recipient.DrawPos) / 2f) + new Vector3(0f, 0f, 1f), initiator.Map, "TextMote_TameSuccess".Translate("25%"), 8f);
                    }
                }
            }
        }
    }
}
