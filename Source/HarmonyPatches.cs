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
using RimWorld.SketchGen;
using Verse.Grammar;
using RimWorld.Planet;

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
            if (c.GetPlant(map)?.def != null && c.GetPlant(map)?.def == ThrumkinDefOf.Plant_Frostleaf)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(SketchGenUtility), nameof(SketchGenUtility.IsStuffAllowed))]
    public static class IsStuffAllowedPatch
    {
        [HarmonyPostfix]
        public static void IsStuffAllowed_Postfix(ref bool __result, ThingDef stuff, bool allowWood, Map useOnlyStonesAvailableOnMap, bool allowFlammableWalls, ThingDef stuffFor)
        {
            if (stuff == ThrumkinDefOf.WoodLog_GhostAsh)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(SketchGenUtility), nameof(SketchGenUtility.IsFloorAllowed_NewTmp))]
    public static class IsFloorAllowedPatch
    {
        [HarmonyPostfix]
        public static void IsFloorAllowed_Postfix(ref bool __result, TerrainDef floor, bool allowWoodenFloor)
        {
            if (!allowWoodenFloor && floor == ThrumkinDefOf.GhostAshFloor)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.HasPartsToWear))]
    public static class HasPartsToWearPatch
    {
        [HarmonyPostfix]
        public static void HasPartsToWear_Postfix(ref bool __result, Pawn p, ThingDef apparel)
        {
            if (apparel?.apparel?.bodyPartGroups != null && p?.def != null)
            {
                if (p.def == ThrumkinDefOf.Thrumkin && apparel.apparel.bodyPartGroups.Contains(ThrumkinDefOf.Feet) 
                    && (!apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) || !apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)))
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Need_Food), nameof(Need_Food.MaxLevel), MethodType.Getter)]
    public static class Need_FoodPatch
    {
        [HarmonyPostfix]
        public static void Need_Food_Postfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn?.def != null && ___pawn.def == ThrumkinDefOf.Thrumkin)
            {
                __result *= 1.5f;
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
                if (__result != QualityCategory.Awful && Rand.Value < 0.2f)
                {
                    __result -= 1;
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
                if (__instance.def == ThingDefOf.MealSurvivalPack || __instance.def == ThingDefOf.Pemmican || __instance.def == ThrumkinDefOf.MealLavish)
                {
                    nutritionIngested *= 1.2f;
                    return;
                }
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
                        nutritionIngested *= 0.35f;
                    }
                    else if (animalP && nonAnimalP)
                    {
                        nutritionIngested *= 1.5f;
                    }
                    else if (meat && nonMeat)
                    {
                        nutritionIngested *= 0.7f;
                    }
                }
                else
                {
                    if ((__instance.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 0.35f;
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
                if (__instance.def == ThingDefOf.WoodLog && ingester.def == ThrumkinDefOf.Thrumkin)
                {
                    ingester.health.AddHediff(ThrumkinDefOf.AteWoodHediff);
                }
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    public static class FoodOptimalityPatch
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        public static void FoodOptimality_Postfix(ref float __result, Pawn eater, Thing foodSource)
        {
            if (SyrThrumkinSettings.useStandardAI)
            {
                return;
            }
            else
            {
                __result = FoodOptimality_Method(__result, eater, foodSource);
            }
        }
        public static float FoodOptimality_Method(float __result, Pawn eater, Thing foodSource)
        {
            float foodValue = __result;
            if (eater?.def != null && foodSource?.def?.ingestible != null && eater.def == ThrumkinDefOf.Thrumkin)
            {
                if (foodSource.def == ThingDefOf.MealSurvivalPack || foodSource.def == ThingDefOf.Pemmican || foodSource.def == ThrumkinDefOf.MealLavish)
                {
                    return foodValue;
                }
                if (foodSource.def == ThingDefOf.WoodLog || foodSource.def == ThingDefOf.Hay)
                {
                    foodValue -= 50;
                }
                CompIngredients compIngr = foodSource.TryGetComp<CompIngredients>();
                if (compIngr?.ingredients != null)
                {
                    bool meat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) != FoodTypeFlags.None);
                    bool animalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None);
                    bool nonMeat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) == FoodTypeFlags.None);
                    bool nonAnimalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) == FoodTypeFlags.None);
                    if (meat && animalP)
                    {
                        foodValue += 0;
                    }
                    else if (animalP && !nonAnimalP)
                    {
                        foodValue += 40;
                    }
                    else if (meat && !nonMeat)
                    {
                        foodValue -= 50;
                    }
                    else if (animalP && nonAnimalP)
                    {
                        foodValue += 20;
                    }
                    else if (meat && nonMeat)
                    {
                        foodValue -= 10;
                    }
                }
                else
                {
                    if ((foodSource.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        foodValue -= 70;
                    }
                    else if ((foodSource.def.ingestible.foodType & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None)
                    {
                        foodValue += 30;
                    }
                }
            }
            return foodValue;
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
                Predicate<Thing> validator = t => !t.IsForbidden(eater.Faction) && (t.def == ThingDefOf.WoodLog || t.def == ThingDefOf.Hay);
                Thing bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.Touch, TraverseParms.For(getter), 75f, validator);
                if (bestThing == null)
                {
                    __result = false;
                    return;
                }
                else
                {
                    foodSource = bestThing;
                    foodDef = FoodUtility.GetFinalIngestibleDef(bestThing);
                    __result = true;
                    return;
                }
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

    [HarmonyPatch(typeof(Room), nameof(Room.Owners), MethodType.Getter)]
    public class RoomOwnersPatch
    {
        [HarmonyPostfix]
        public static IEnumerable<Pawn> RoomOwners_Postfix(IEnumerable<Pawn> __result, Room __instance)
        {
            if (__instance.TouchesMapEdge)
            {
                yield break;
            }
            if (__instance.IsHuge)
            {
                yield break;
            }
            if (__instance.Role != RoomRoleDefOf.Bedroom && __instance.Role != RoomRoleDefOf.PrisonCell && __instance.Role != RoomRoleDefOf.Barracks && __instance.Role != RoomRoleDefOf.PrisonBarracks)
            {
                yield break;
            }
            Pawn pawn = null;
            Pawn secondOwner = null;
            foreach (Building_Bed building_Bed in __instance.ContainedBeds)
            {
                if (building_Bed.def.building.bed_humanlike)
                {
                    for (int i = 0; i < building_Bed.OwnersForReading.Count; i++)
                    {
                        if (pawn == null)
                        {
                            pawn = building_Bed.OwnersForReading[i];
                        }
                        else
                        {
                            if (secondOwner != null)
                            {
                                yield break;
                            }
                            secondOwner = building_Bed.OwnersForReading[i];
                        }
                    }
                }
            }
            if (pawn != null)
            {
                yield return pawn;
                if (secondOwner != null)
                {
                    yield return secondOwner;
                }
            }
            yield break;
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
                        if ((pawn.def == ThrumkinDefOf.Thrumkin || pawn2.def == ThrumkinDefOf.Thrumkin) && pawn.ageTracker.AgeBiologicalYearsFloat >= 16f && pawn2.ageTracker.AgeBiologicalYearsFloat >= 16f)
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
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void RandomSelectionWeight_Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (initiator?.def != null && recipient?.def != null)
            {
                if (initiator.def == ThrumkinDefOf.Thrumkin)
                {
                    __result *= 0.5f;
                }
                else if (recipient.def == ThrumkinDefOf.Thrumkin)
                {
                    __result *= 0.5f;
                }
            }
        }
    }

    /*[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
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
    }*/

    [HarmonyPatch(typeof(Faction), nameof(Faction.TryGenerateNewLeader))]
    public static class TryGenerateNewLeaderPatch
    {
        public static bool PrefixRunning = false;

        [HarmonyPrefix]
        public static bool TryGenerateNewLeader_Prefix(Faction __instance)
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
                pawn.relations.everSeenByPlayer = true;
                if (!Find.WorldPawns.Contains(pawn))
                {
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
                }
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
            __result = TryGenerateNewPawnInternal_Method(__result, request);
        }
        public static Pawn TryGenerateNewPawnInternal_Method(Pawn __result, PawnGenerationRequest request)
        {
            if (__result?.story?.childhood != null && __result?.story?.adulthood != null)
            {
                if (!TryGenerateNewLeaderPatch.PrefixRunning && (__result.story.childhood == ThrumkinDefOf.Thrumkin_CBio_Menardy.backstory || __result.story.adulthood == ThrumkinDefOf.Thrumkin_ABio_Menardy.backstory))
                {
                    return null;
                }
            }
            return __result;
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

    [HarmonyPatch(typeof(GrammarUtility), nameof(GrammarUtility.RulesForPawn), new[] {
        typeof(string),
        typeof(Name),
        typeof(string),
        typeof(PawnKindDef),
        typeof(Gender),
        typeof(Faction),
        typeof(int),
        typeof(int),
        typeof(string),
        typeof(bool),
        typeof(bool),
        typeof(bool),
        typeof(List<RoyalTitle>),
        typeof(Dictionary<string, string>),
        typeof(bool)})]
    public static class RulesForPawnPatch
    {
        [HarmonyPostfix]
        public static IEnumerable<Rule> RulesForPawn_Postfix(IEnumerable<Rule> __result, string pawnSymbol, string title, Gender gender, PawnKindDef kind)
        {
            List<Rule> ruleList = __result.ToList();
            string prefix = "";
            if (!pawnSymbol.NullOrEmpty())
            {
                prefix = prefix + pawnSymbol + "_";
            }
            for (int i = 0; i < ruleList.Count; i++)
            {
                Rule_String r = ruleList[i] as Rule_String;
                if (r != null && r.keyword == (prefix + "titleIndef"))
                {
                    ruleList[i] = new Rule_String(prefix + "titleIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.race.label + " " + title, gender, false, false));
                }
                else if (r != null && r.keyword == (prefix + "titleDef"))
                {
                    ruleList[i] = new Rule_String(prefix + "titleDef", Find.ActiveLanguageWorker.WithDefiniteArticle(kind.race.label + " " + title, gender, false, false));
                }
                else if (r != null && r.keyword == (prefix + "title"))
                {
                    ruleList[i] = new Rule_String(prefix + "title", kind.race.label + " " + title);
                }
            }
            __result = ruleList;
            foreach (Rule rule in __result)
            {
                yield return rule;
            }
        }
    }
}
