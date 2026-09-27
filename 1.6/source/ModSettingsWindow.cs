// Copyright (C) 2026 Sk
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Drugs_on_Duty
{
    public static class ModSettingsWindow
    {
        private const float ButtonRowHeight = 30f;
        private const float ButtonRowSpacing = 8f;

        private static List<ThingDef> cachedDrugDefs;
        private static Vector2 scrollPosition;

        public static void Draw(Rect parent)
        {
            List<ThingDef> drugDefs = GetDrugDefs();

            Rect headerRect = parent.TopPartPixels(30f);
            Widgets.Label(headerRect, "DrugsonDuty.SettingsList.Header".Translate());

            Rect buttonRowRect = new Rect(parent.x, headerRect.yMax + 4f, parent.width, ButtonRowHeight);
            float buttonWidth = (buttonRowRect.width - ButtonRowSpacing) / 2f;
            Rect disableAllRect = new Rect(buttonRowRect.x, buttonRowRect.y, buttonWidth, buttonRowRect.height);
            Rect resetRect = new Rect(disableAllRect.xMax + ButtonRowSpacing, buttonRowRect.y, buttonWidth, buttonRowRect.height);

            if (Widgets.ButtonText(disableAllRect, "DrugsonDuty.SettingsList.DisableAll".Translate()))
            {
                ModSettings.DisableAll(drugDefs);
            }

            if (Widgets.ButtonText(resetRect, "DrugsonDuty.SettingsList.ResetToDefaults".Translate()))
            {
                ModSettings.ResetToDefaults();
            }

            Rect listOutRect = new Rect(parent.x, buttonRowRect.yMax + 4f, parent.width, parent.height - headerRect.height - buttonRowRect.height - 8f);
            float rowHeight = Text.LineHeight + 4f;
            Rect listViewRect = new Rect(0f, 0f, listOutRect.width - 16f, rowHeight * drugDefs.Count);

            Widgets.BeginScrollView(listOutRect, ref scrollPosition, listViewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(listViewRect);
            foreach (ThingDef drugDef in drugDefs)
            {
                bool enabled = ModSettings.IsEnabledForDrug(drugDef);
                bool newEnabled = enabled;
                listing.CheckboxLabeled(drugDef.LabelCap, ref newEnabled);
                if (newEnabled != enabled)
                {
                    ModSettings.SetEnabledForDrug(drugDef, newEnabled);
                }
            }
            listing.End();
            Widgets.EndScrollView();
        }

        private static List<ThingDef> GetDrugDefs()
        {
            if (cachedDrugDefs == null)
            {
                cachedDrugDefs = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(d => d.IsDrug)
                    .OrderBy(d => d.LabelCap.ToString())
                    .ToList();
            }

            return cachedDrugDefs;
        }
    }
}
