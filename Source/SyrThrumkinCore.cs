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
    class SyrThrumkinCore : Mod
    {
        public static SyrThrumkinSettings settings;

        public SyrThrumkinCore(ModContentPack content) : base(content)
        {
            settings = GetSettings<SyrThrumkinSettings>();
            var harmony = new Harmony("Syrchalis.Rimworld.Thrumkin");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory() => "SyrThrumkinSettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            checked
            {
                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(inRect);
                listing_Standard.CheckboxLabeled("SyrThrumkin_useUnsupportedHair".Translate(), ref SyrThrumkinSettings.useUnsupportedHair, "SyrThrumkin_useUnsupportedHairTooltip".Translate());
                listing_Standard.CheckboxLabeled("SyrThrumkin_useStandardAI".Translate(), ref SyrThrumkinSettings.useStandardAI, "SyrThrumkin_useStandardAITooltip".Translate());
                listing_Standard.CheckboxLabeled("SyrThrumkin_manualWoodConsumption".Translate(), ref SyrThrumkinSettings.manualWoodConsumption, "SyrThrumkin_manualWoodConsumptionTooltip".Translate());
                listing_Standard.Gap(24f);
                if (listing_Standard.ButtonText("SyrThrumkin_defaultSettings".Translate(), "SyrThrumkin_defaultSettingsTooltip".Translate()))
                {
                    SyrThrumkinSettings.useUnsupportedHair = false;
                    SyrThrumkinSettings.useStandardAI = false;
                }
                listing_Standard.End();
                settings.Write();
            }
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
            ApplySettings();
        }

        public static void ApplySettings()
        {
            if (SyrThrumkinSettings.useUnsupportedHair)
            {
                ThingDef_AlienRace thrumkin = ThrumkinDefOf.Thrumkin as ThingDef_AlienRace;
                thrumkin.alienRace.hairSettings.hairTags.Add("Tribal");
                thrumkin.alienRace.hairSettings.hairTags.Add("Rural");
                thrumkin.alienRace.hairSettings.hairTags.Add("Urban");
                thrumkin.alienRace.hairSettings.hairTags.Add("Punk");
            }
            else
            {
                ThingDef_AlienRace thrumkin = ThrumkinDefOf.Thrumkin as ThingDef_AlienRace;
                thrumkin.alienRace.hairSettings.hairTags.Clear();
                thrumkin.alienRace.hairSettings.hairTags.Add("Thrumkin");
            }
        }
    }
}
