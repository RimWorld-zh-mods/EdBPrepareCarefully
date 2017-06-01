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
    public class UniqueBodyPart {
        public BodyPartRecord Record;
        public bool Replaceable;
        public int Index;
        public bool SkinCovered;
        public bool Solid;
        public List<UniqueBodyPart> Ancestors;
    }
    public class OptionsHealth {
        protected List<UniqueBodyPart> bodyPartList = new List<UniqueBodyPart>();
        protected Dictionary<BodyPartDef, List<UniqueBodyPart>> bodyPartDefLookup = new Dictionary<BodyPartDef, List<UniqueBodyPart>>();
        protected Dictionary<RecipeDef, List<UniqueBodyPart>> implantRecipeLookup = new Dictionary<RecipeDef, List<UniqueBodyPart>>();
        protected Dictionary<BodyPartRecord, UniqueBodyPart> bodyPartRecordLookup = new Dictionary<BodyPartRecord, UniqueBodyPart>();

        protected List<RecipeDef> implantRecipes = new List<RecipeDef>();
        protected Dictionary<HediffDef, InjuryOption> injuryOptionsByHediff = new Dictionary<HediffDef, InjuryOption>();
        protected List<InjuryOption> injuryOptions = new List<InjuryOption>();
        public OptionsHealth() {

        }
        public BodyDef BodyDef {
            get; set;
        }
        public void AddBodyPart(UniqueBodyPart part) {
            bodyPartList.Add(part);
            List<UniqueBodyPart> partsForRecord;
            if (!bodyPartDefLookup.TryGetValue(part.Record.def, out partsForRecord)) {
                partsForRecord = new List<UniqueBodyPart>();
                bodyPartDefLookup.Add(part.Record.def, partsForRecord);
            }
            partsForRecord.Add(part);
            bodyPartRecordLookup.Add(part.Record, part);
        }
        public int CountOfMatchingBodyParts(BodyPartDef def) {
            List<UniqueBodyPart> result;
            if (bodyPartDefLookup.TryGetValue(def, out result)) {
                return result.Count;
            }
            else {
                return 0;
            }
        }
        public IEnumerable<UniqueBodyPart> Ancestors(BodyPartRecord record) {
            UniqueBodyPart part;
            if (bodyPartRecordLookup.TryGetValue(record, out part)) {
                return part.Ancestors;
            }
            return null;
        }
        public int FindIndexForBodyPart(BodyPartRecord record) {
            List<UniqueBodyPart> result;
            if (bodyPartDefLookup.TryGetValue(record.def, out result)) {
                int index = result.FirstIndexOf((UniqueBodyPart p) => { return p.Record == record; });
                if (index >= result.Count) {
                    return -1;
                }
                else {
                    return index;
                }
            }
            return -1;
        }
        public UniqueBodyPart FindBodyPart(BodyPartDef def, int index) {
            List<UniqueBodyPart> result;
            if (bodyPartDefLookup.TryGetValue(def, out result)) {
                if (index < result.Count) {
                    return result[index];
                }
            }
            return null;
        }
        public UniqueBodyPart FindBodyPartByName(string name, int index) {
            BodyPartDef def = DefDatabase<BodyPartDef>.GetNamedSilentFail(name);
            if (def != null) {
                return FindBodyPart(def, index);
            }
            List<UniqueBodyPart> result;
            if (bodyPartDefLookup.TryGetValue(def, out result)) {
                if (index < result.Count) {
                    return result[index];
                }
            }
            return null;
        }
        public UniqueBodyPart FindBodyPartsForRecord(BodyPartRecord record) {
            UniqueBodyPart result;
            if (bodyPartRecordLookup.TryGetValue(record, out result)) {
                return result;
            }
            else {
                return null;
            }
        }
        public List<UniqueBodyPart> FindBodyPartsForDef(BodyPartDef def) {
            List<UniqueBodyPart> result;
            if (bodyPartDefLookup.TryGetValue(def, out result)) {
                return result;
            }
            else {
                return null;
            }
        }
        public IEnumerable<UniqueBodyPart> SkinCoveredBodyParts {
            get {
                return bodyPartList.Where((UniqueBodyPart p) => { return p.SkinCovered; });
            }
        }
        public IEnumerable<UniqueBodyPart> SoftBodyParts {
            get {
                return bodyPartList.Where((UniqueBodyPart p) => { return !p.Solid; });
            }
        }
        public IEnumerable<UniqueBodyPart> SolidBodyParts {
            get {
                return bodyPartList.Where((UniqueBodyPart p) => { return p.Solid; });
            }
        }
        public void AddImplantRecipe(RecipeDef recipe, List<UniqueBodyPart> parts) {
            if (parts != null && parts.Count > 0) {
                List<UniqueBodyPart> partList;
                if (implantRecipeLookup.TryGetValue(recipe, out partList)) {
                    partList.AddRange(parts);
                }
                else {
                    implantRecipes.Add(recipe);
                    implantRecipeLookup.Add(recipe, parts.ToList());
                }
            }
        }
        public List<UniqueBodyPart> FindBodyPartsForImplantRecipe(RecipeDef recipeDef) {
            List<UniqueBodyPart> partList;
            if (implantRecipeLookup.TryGetValue(recipeDef, out partList)) {
                return partList;
            }
            else {
                return null;
            }
        }
        public List<RecipeDef> ImplantRecipes {
            get {
                return implantRecipes;
            }
        }
        public void Sort() {
            implantRecipes.Sort((RecipeDef a, RecipeDef b) => {
                return a.LabelCap.CompareTo(b.LabelCap);
            });
            injuryOptions.Sort((InjuryOption a, InjuryOption b) => {
                return a.Label.CompareTo(b.Label);
            });
        }
        public void AddInjury(InjuryOption option) {
            if (!injuryOptionsByHediff.ContainsKey(option.HediffDef)) {
                this.injuryOptionsByHediff.Add(option.HediffDef, option);
                this.injuryOptions.Add(option);
            }
        }
        public InjuryOption FindInjuryOptionByHediffDef(HediffDef def) {
            InjuryOption option;
            if (injuryOptionsByHediff.TryGetValue(def, out option)) {
                return option;
            }
            else {
                return null;
            }
        }
        public List<InjuryOption> InjuryOptions {
            get {
                return injuryOptions;
            }
        }
        public IEnumerable<BodyPartRecord> BodyPartsForInjury(InjuryOption option) {
            return SkinCoveredBodyParts.Select((UniqueBodyPart p) => { return p.Record; });
        }
    }
}
