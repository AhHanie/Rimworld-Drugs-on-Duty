// Copyright (C) 2026 Sk
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using Verse;

namespace Drugs_on_Duty
{
    public class ModSettings : Verse.ModSettings
    {
        // Only exceptions are stored, so every drug - including ones added later by this mod or by
        // other mods - defaults to enabled without needing a migration step. Static because RimWorld
        // only ever creates one instance of this class for the mod's lifetime; the instance itself
        // exists solely so Verse.Mod.GetSettings<T>() has something to call ExposeData() on.
        public static HashSet<string> DisabledDrugDefNames = new HashSet<string>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref DisabledDrugDefNames, "disabledDrugDefNames", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && DisabledDrugDefNames == null)
            {
                DisabledDrugDefNames = new HashSet<string>();
            }
        }

        public static bool IsEnabledForDrug(ThingDef drugDef)
        {
            return drugDef != null && !DisabledDrugDefNames.Contains(drugDef.defName);
        }

        public static void SetEnabledForDrug(ThingDef drugDef, bool enabled)
        {
            if (drugDef == null)
            {
                return;
            }

            if (enabled)
            {
                DisabledDrugDefNames.Remove(drugDef.defName);
            }
            else
            {
                DisabledDrugDefNames.Add(drugDef.defName);
            }
        }

        public static void DisableAll(IEnumerable<ThingDef> drugDefs)
        {
            foreach (ThingDef drugDef in drugDefs)
            {
                if (drugDef != null)
                {
                    DisabledDrugDefNames.Add(drugDef.defName);
                }
            }
        }

        public static void ResetToDefaults()
        {
            DisabledDrugDefNames.Clear();
        }
    }
}
