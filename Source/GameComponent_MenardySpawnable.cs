using Harmony;
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
    public class GameComponent_MenardySpawnable : GameComponent
    {
        public GameComponent_MenardySpawnable(Game game)
        {
        }
        public bool MenardySpawnable = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref MenardySpawnable, "Thrumkin_MenardySpawnable", true, false);
        }
    }
}
