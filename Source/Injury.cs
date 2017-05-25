﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully {
    public class Injury : CustomBodyPart {
        protected BodyPartRecord bodyPartRecord;

        protected InjuryOption option;

        protected Hediff hediff;

        protected string tooltip;

        public InjuryOption Option {
            get {
                return option;
            }
            set {
                option = value;
                tooltip = null;
                stageLabel = ComputeStageLabel();
            }
        }

        protected float severity = 0;

        protected string stageLabel = null;

        protected float? painFactor = null;

        public float Severity {
            get {
                return severity;
            }
            set {
                tooltip = null;
                severity = value;
                stageLabel = ComputeStageLabel();
            }
        }

        public float? PainFactor {
            get {
                return painFactor;
            }
            set {
                painFactor = value;
            }
        }

        public Injury() {
        }

        override public BodyPartRecord BodyPartRecord {
            get {
                return bodyPartRecord;
            }
            set {
                bodyPartRecord = value;
            }
        }

        override public string ChangeName {
            get {
                if (stageLabel != null) {
                    return stageLabel;
                }
                else if (Option != null) {
                    return Option.Label;
                }
                else {
                    return "?";
                }
            }
        }

        override public Color LabelColor {
            get {
                if (Option != null && Option.HediffDef != null) {
                    return Option.HediffDef.defaultLabelColor;
                }
                else {
                    return Style.ColorText;
                }
            }
        }

        public override void AddToPawn(CustomPawn customPawn, Pawn pawn) {
            if (Option.Giver != null) {
                Hediff hediff = HediffMaker.MakeHediff(Option.HediffDef, pawn, BodyPartRecord);
                hediff.Severity = this.Severity;
                pawn.health.AddHediff(hediff, BodyPartRecord, null);
                this.hediff = hediff;
            }
            else if (Option.IsOldInjury) {
                Hediff hediff = HediffMaker.MakeHediff(Option.HediffDef, pawn, null);
                hediff.Severity = this.Severity;

                HediffComp_GetsOld getsOld = hediff.TryGetComp<HediffComp_GetsOld>();
                if (getsOld != null) {
                    hediff.TryGetComp<HediffComp_GetsOld>().IsOld = true;
                    hediff.TryGetComp<HediffComp_GetsOld>().painFactor = painFactor == null ? OldInjuryUtility.GetRandomPainFactor() : painFactor.Value;
                }

                pawn.health.AddHediff(hediff, BodyPartRecord, null);
                this.hediff = hediff;
            }
            else {
                Hediff hediff = HediffMaker.MakeHediff(Option.HediffDef, pawn, null);
                hediff.Severity = this.Severity;
                pawn.health.AddHediff(hediff, null, null);
                this.hediff = hediff;
            }
            pawn.health.capacities.Clear();
        }

        protected string ComputeStageLabel() {
            if (Option.HasStageLabel) {
                return "EdB.PC.Panel.Health.InjuryLabel.Stage".Translate(new object[] {
                    this.option.Label, CurStage.label
                });
            }
            else if (Option.IsOldInjury) {
                return "EdB.PC.Panel.Health.InjuryLabel.Severity".Translate(new object[] {
                    this.option.Label, (int)severity
                });
            }
            else {
                return null;
            }
        }

        protected HediffStage CurStage {
            get {
                return (!this.option.HediffDef.stages.NullOrEmpty<HediffStage>()) ? this.option.HediffDef.stages[this.CurStageIndex] : null;
            }
        }

        protected int CurStageIndex {
            get {
                if (this.option.HediffDef.stages == null) {
                    return 0;
                }
                List<HediffStage> stages = this.option.HediffDef.stages;
                for (int i = stages.Count - 1; i >= 0; i--) {
                    if (this.Severity >= stages[i].minSeverity) {
                        return i;
                    }
                }
                return 0;
            }
        }

        public override bool HasTooltip {
            get {
                return hediff != null;
            }
        }

        public override string Tooltip {
            get {
                if (tooltip == null) {
                    InitializeTooltip();
                }
                return tooltip;
            }
        }

        protected void InitializeTooltip() {
            StringBuilder stringBuilder = new StringBuilder();
            if (hediff.Part != null) {
                stringBuilder.Append(hediff.Part.def.LabelCap + ": ");
                stringBuilder.Append(" " + hediff.pawn.health.hediffSet.GetPartHealth(hediff.Part).ToString() + " / " + hediff.Part.def.GetMaxHealth(hediff.pawn).ToString());
            }
            else {
                stringBuilder.Append("WholeBody".Translate());
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("------------------");
            Hediff_Injury hediff_Injury = hediff as Hediff_Injury;
            string damageLabel = hediff.SeverityLabel;
            if (!hediff.Label.NullOrEmpty() || !damageLabel.NullOrEmpty() || !hediff.CapMods.NullOrEmpty<PawnCapacityModifier>()) {
                stringBuilder.Append(hediff.LabelCap);
                if (!damageLabel.NullOrEmpty()) {
                    stringBuilder.Append(": " + damageLabel);
                }
                stringBuilder.AppendLine();
                string tipStringExtra = hediff.TipStringExtra;
                if (!tipStringExtra.NullOrEmpty()) {
                    stringBuilder.AppendLine(tipStringExtra.TrimEndNewlines().Indented());
                }
            }
            tooltip = stringBuilder.ToString();
        }
    }
}

