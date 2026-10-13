// Copyright (C) 2026 Sk
// SPDX-License-Identifier: GPL-3.0-or-later

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Drugs_on_Duty
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "DrugsonDuty.LoadingLabel", doAsynchronously: true, null);
        }

        private void Init()
        {
            // Triggers RimWorld's Scribe read of the previously saved settings into ModSettings'
            // static fields; nothing else needs the returned instance.
            GetSettings<ModSettings>();
            new Harmony("sk.drugsonduty").PatchAll();
        }

        public override string SettingsCategory()
        {
            return "DrugsonDuty.SettingsTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }
}
