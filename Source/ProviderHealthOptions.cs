﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully {
    public class ProviderHealthOptions {
        protected Dictionary<ThingDef, OptionsHealth> optionsLookup = new Dictionary<ThingDef, OptionsHealth>();
        public OptionsHealth GetOptions(CustomPawn pawn) {
            OptionsHealth result = null;
            if (!optionsLookup.TryGetValue(pawn.Pawn.def, out result)) {
                result = InitializeHealthOptions(pawn.Pawn.def);
                optionsLookup.Add(pawn.Pawn.def, result);
            }
            return result;
        }
        protected OptionsHealth InitializeHealthOptions(ThingDef pawnThingDef) {
            OptionsHealth result = new OptionsHealth();
            BodyDef bodyDef = pawnThingDef.race.body;
            result.BodyDef = bodyDef;
            
            HashSet<UniqueBodyPart> ancestors = new HashSet<UniqueBodyPart>();
            ProcessBodyPart(result, bodyDef.corePart, 1, ancestors);

            InitializeImplantRecipes(result, pawnThingDef);
            InitializeInjuryOptions(result, pawnThingDef);

            result.Sort();
            return result;
        }
        protected int ProcessBodyPart(OptionsHealth options, BodyPartRecord record, int index, HashSet<UniqueBodyPart> ancestors) {
            int partIndex = options.CountOfMatchingBodyParts(record.def);
            FieldInfo skinCoveredField = typeof(BodyPartDef).GetField("skinCovered", BindingFlags.Instance | BindingFlags.NonPublic);
            bool skinCoveredValue = (bool)skinCoveredField.GetValue(record.def);
            FieldInfo isSolidField = typeof(BodyPartDef).GetField("isSolid", BindingFlags.Instance | BindingFlags.NonPublic);
            bool isSolidValue = (bool)isSolidField.GetValue(record.def);
            UniqueBodyPart part = new UniqueBodyPart() {
                Index = partIndex,
                Record = record,
                SkinCovered = skinCoveredValue,
                Solid = isSolidValue,
                Ancestors = ancestors.ToList()
            };
            options.AddBodyPart(part);
            ancestors.Add(part);
            foreach (var c in record.parts) {
                index = ProcessBodyPart(options, c, index + 1, ancestors);
            }
            ancestors.Remove(part);
            return index;
        }
        protected void InitializeImplantRecipes(OptionsHealth options, ThingDef pawnThingDef) {
            // Find all recipes that replace a body part.
            List<RecipeDef> recipes = new List<RecipeDef>();
            recipes.AddRange(DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef def) => {
                if (def.addsHediff != null && def.appliedOnFixedBodyParts != null && def.appliedOnFixedBodyParts.Count > 0
                        && (def.recipeUsers.NullOrEmpty() || def.recipeUsers.Contains(pawnThingDef))) {
                    return true;
                }
                else {
                    return false;
                }
            }));
            // De-dupe the recipe list.
            HashSet<RecipeDef> recipeSet = new HashSet<RecipeDef>();
            foreach (var r in recipes) {
                recipeSet.Add(r);
            }
            recipes = new List<RecipeDef>(recipeSet);

            // Iterate the recipes. Populate a list of all of the body parts that apply to a given recipe.
            foreach (var r in recipes) {
                // Add all of the body parts for that recipe to the list.
                foreach (var bodyPartDef in r.appliedOnFixedBodyParts) {
                    List<UniqueBodyPart> validBodyParts = options.FindBodyPartsForDef(bodyPartDef);
                    if (validBodyParts != null && validBodyParts.Count > 0) {
                        options.AddImplantRecipe(r, validBodyParts);
                        foreach (var part in validBodyParts) {
                            part.Replaceable = true;
                        }
                    }
                }
            }
        }
        protected void InitializeHediffGiverInjuries(OptionsHealth options, HediffGiver giver) {
            InjuryOption option = new InjuryOption();
            option.HediffDef = giver.hediff;
            option.Label = giver.hediff.LabelCap;
            option.Giver = giver;
            if (giver.partsToAffect == null) {
                option.WholeBody = true;
            }
            if (giver.canAffectAnyLivePart) {
                option.WholeBody = false;
            }
            if (giver.partsToAffect != null && !giver.canAffectAnyLivePart) {
                List<BodyPartDef> validParts = new List<BodyPartDef>();
                foreach (var def in giver.partsToAffect) {
                    if (options.FindBodyPartsForDef(def) != null) {
                        validParts.Add(def);
                    }
                }
                if (validParts.Count == 0) {
                    return;
                }
                else {
                    option.ValidParts = validParts;
                }
            }
            options.AddInjury(option);
        }
        protected void InitializeInjuryOptions(OptionsHealth options, ThingDef pawnThingDef) {
            HashSet<HediffDef> addedDefs = new HashSet<HediffDef>();
            // Go through all of the hediff giver sets for the pawn's race and intialize injuries from
            // each giver.
            if (pawnThingDef.race.hediffGiverSets != null) {
                foreach (var giverSetDef in pawnThingDef.race.hediffGiverSets) {
                    foreach (var giver in giverSetDef.hediffGivers) {
                        InitializeHediffGiverInjuries(options, giver);
                    }
                }
            }
            // Go through all hediff stages, looking for hediff givers.
            foreach (var hd in DefDatabase<HediffDef>.AllDefs) {
                if (hd.stages != null) {
                    foreach (var stage in hd.stages) {
                        if (stage.hediffGivers != null) {
                            foreach (var giver in stage.hediffGivers) {
                                InitializeHediffGiverInjuries(options, giver);
                            }
                        }
                    }
                }
            }
            // Go though all of the chemical defs, looking for hediff givers.
            foreach (var chemicalDef in DefDatabase<ChemicalDef>.AllDefs) {
                if (chemicalDef.onGeneratedAddictedEvents != null) {
                    foreach (var giver in chemicalDef.onGeneratedAddictedEvents) {
                        InitializeHediffGiverInjuries(options, giver);
                    }
                }
            }

            // Get all of the hediffs that can be added via the "forced hediff" scenario part and
            // add them to a hash set so that we can quickly look them up.
            ScenPart_ForcedHediff scenPart = new ScenPart_ForcedHediff();
            IEnumerable<HediffDef> scenPartDefs = (IEnumerable<HediffDef>)typeof(ScenPart_ForcedHediff)
                .GetMethod("PossibleHediffs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(scenPart, null);
            HashSet<HediffDef> scenPartDefSet = new HashSet<HediffDef>(scenPartDefs);
            
            // Add injury options.
            foreach (var hd in DefDatabase<HediffDef>.AllDefs) {
                // TODO: Missing body part seems to be a special case.  The hediff giver doesn't itself remove
                // limbs, so disable it until we can add special-case handling.
                if (hd.defName == "MissingBodyPart") {
                    continue;
                }
                // Filter out defs that were already added via the hediff giver sets.
                if (addedDefs.Contains(hd)) {
                    continue;
                }
                // Filter out implants.
                if (hd.hediffClass == typeof(Hediff_AddedPart)) {
                    continue;
                }

                // If it's an old injury, use the old injury properties to get the label.
                HediffCompProperties p = hd.CompPropsFor(typeof(HediffComp_GetsOld));
                HediffCompProperties_GetsOld getsOldProperties = p as HediffCompProperties_GetsOld;
                String label;
                if (getsOldProperties != null) {
                    if (getsOldProperties.oldLabel != null) {
                        label = getsOldProperties.oldLabel.CapitalizeFirst();
                    }
                    else {
                        Log.Warning("Prepare Carefully could not find label for old injury: " + hd.defName);
                        continue;
                    }
                }
                // If it's not an old injury, make sure it's one of the available hediffs that can
                // be added via ScenPart_ForcedHediff.  If it's not, filter it out.
                else {
                    if (!scenPartDefSet.Contains(hd)) {
                        continue;
                    }
                    label = hd.LabelCap;
                }

                // Add the injury option..
                InjuryOption option = new InjuryOption();
                option.HediffDef = hd;
                option.Label = label;
                if (getsOldProperties != null) {
                    option.IsOldInjury = true;
                }
                else {
                    option.ValidParts = new List<BodyPartDef>();
                }
                options.AddInjury(option);
            }
            
            // Disambiguate duplicate injury labels.
            HashSet<string> labels = new HashSet<string>();
            HashSet<string> duplicateLabels = new HashSet<string>();
            foreach (var option in options.InjuryOptions) {
                if (labels.Contains(option.Label)) {
                    duplicateLabels.Add(option.Label);
                }
                else {
                    labels.Add(option.Label);
                }
            }
            foreach (var option in options.InjuryOptions) {
                HediffCompProperties p = option.HediffDef.CompPropsFor(typeof(HediffComp_GetsOld));
                HediffCompProperties_GetsOld props = p as HediffCompProperties_GetsOld;
                if (props != null) {
                    if (duplicateLabels.Contains(option.Label)) {
                        string label = "EdB.PC.Dialog.Injury.OldInjury.Label".Translate(new string[] {
                                    props.oldLabel.CapitalizeFirst(), option.HediffDef.LabelCap
                                });
                        option.Label = label;
                    }
                }
            }
        }
    }
}
