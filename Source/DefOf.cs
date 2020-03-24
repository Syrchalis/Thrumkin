using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using AlienRace;

namespace SyrThrumkin
{
    [DefOf]
    public static class ThrumkinDefOf
    {
        static ThrumkinDefOf()
        {
        }
        public static ThingDef Thrumkin;
        public static ThingDef Apparel_PlateArmor;
        public static ThingDef Apparel_TribalA;
        public static ThingDef DevilstrandCloth;
        public static ThingDef WoodLog_GhostAsh;
        public static ThingDef RawFrostleaves;
        public static ThingDef Plant_Frostleaf;
        public static ThingDef MealLavish;
        public static TerrainDef GhostAshFloor;
        public static BodyDef Thrumkin_Body;
        public static BodyPartDef HornThrumkin;
        public static BodyPartGroupDef Feet;
        public static ThoughtDef MyHornHarvested;
        public static ThoughtDef KnowColonistHornHarvested;
        public static ThoughtDef KnowOtherHornHarvested;
        public static DamageDef SurgicalCutSpecial;
        public static FactionDef ThrumkinTribe;
        public static BackstoryDef Thrumkin_CBio_Menardy;
        public static BackstoryDef Thrumkin_ABio_Menardy;
        public static PawnBioDef Thrumkin_Bio_Menardy;
        public static PawnKindDef Thrumkin_ElderMelee;
        public static HairDef Thrumkin_Hair_0;
        public static HairDef Thrumkin_Hair_1;
        public static HairDef Thrumkin_Hair_2;
        public static HairDef Thrumkin_Hair_3;
        public static HairDef Thrumkin_Hair_4;
        public static HairDef Thrumkin_Hair_5;
        public static HediffDef AteFrostLeaves;
        public static HediffDef AteWoodHediff;
        public static JobDef ShearThrumkin;
    }
}
