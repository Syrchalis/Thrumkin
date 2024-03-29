﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;

namespace SyrThrumkin
{
    class SyrThrumkinSettings : ModSettings
    {
        public static bool useUnsupportedHair = false;
        public static bool useStandardAI = false;
        public static bool manualWoodConsumption = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref useUnsupportedHair, "SyrThrumkin_useUnsupportedHair", false, true);
            Scribe_Values.Look<bool>(ref useStandardAI, "SyrThrumkin_usestandardAI", false, true);
            Scribe_Values.Look<bool>(ref manualWoodConsumption, "SyrThrumkin_manualWoodConsumption", false, true);
        }
    }
}
