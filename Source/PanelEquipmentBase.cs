﻿using RimWorld;
using System;
using UnityEngine;
using Verse;
namespace EdB.PrepareCarefully {
    public abstract class PanelEquipmentBase : PanelBase {
        /*
        protected void DrawEquipmentIcon(Rect rect, EquipmentDatabaseEntry entry) {
            rect.x = rect.x + 10;
            rect.y = rect.y + 2;
            rect = new Rect(rect.x, rect.y, 38, 38);
            GUI.color = entry.color;
            if (entry.thing == null) {
                // EdB: Inline copy of static Widgets.ThingIcon(Rect, ThingDef) with the selected
                // color based on the stuff.
                GUI.color = entry.color;
                // EdB: Inline copy of static private method with modifications to keep scaled icons within the
                // bounds of the specified Rect and to draw them using the stuff color.
                //Widgets.ThingIconWorker(rect, thing.def, thingDef.uiIcon);
                float num = GenUI.IconDrawScale(entry.def);
                Rect resizedRect = rect;
                if (num != 1f) {
                    // For items that are going to scale out of the bounds of the icon rect, we need to shrink
                    // the bounds a little.
                    if (num > 1) {
                        resizedRect = rect.ContractedBy(4);
                    }
                    resizedRect.width *= num;
                    resizedRect.height *= num;
                    resizedRect.center = rect.center;
                }
                GUI.DrawTexture(resizedRect, entry.def.uiIcon);
                GUI.color = Color.white;
            }
            else {
                // EdB: Inline copy of static Widgets.ThingIcon(Rect, Thing) with graphics switched to show a side view
                // instead of a front view.
                Thing thing = entry.thing;
                GUI.color = thing.DrawColor;
                Texture resolvedIcon;
                if (!thing.def.uiIconPath.NullOrEmpty()) {
                    resolvedIcon = thing.def.uiIcon;
                }
                else if (thing is Pawn) {
                    Pawn pawn = (Pawn)thing;
                    if (!pawn.Drawer.renderer.graphics.AllResolved) {
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }
                    Material matSingle = pawn.Drawer.renderer.graphics.nakedGraphic.MatSide;
                    resolvedIcon = matSingle.mainTexture;
                    GUI.color = matSingle.color;
                }
                else {
                    resolvedIcon = thing.Graphic.ExtractInnerGraphicFor(thing).MatSide.mainTexture;
                }
                // EdB: Inline copy of static private method.
                //Widgets.ThingIconWorker(rect, thing.def, resolvedIcon);
                float num = GenUI.IconDrawScale(thing.def);
                if (num != 1f) {
                    Vector2 center = rect.center;
                    rect.width *= num;
                    rect.height *= num;
                    rect.center = center;
                }
                GUI.DrawTexture(rect, resolvedIcon);
            }
            GUI.color = Color.white;
        }*/
    }
}
