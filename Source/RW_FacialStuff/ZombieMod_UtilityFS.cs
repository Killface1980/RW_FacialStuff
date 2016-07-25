using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Verse
{
    [StaticConstructorOnStartup]
    public static class ZombieMod_UtilityFS
    {
        // Verse.ZombieMod_Utility
        public static bool Zombify(Pawn pawn)
        {
            bool flag = pawn.Drawer == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = pawn.Drawer.renderer == null;
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    bool flag3 = pawn.Drawer.renderer.graphics == null;
                    if (flag3)
                    {
                        result = false;
                    }
                    else
                    {
                        bool flag4 = !pawn.Drawer.renderer.graphics.AllResolved;
                        if (flag4)
                        {
                            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                        }
                        bool flag5 = pawn.Drawer.renderer.graphics.headGraphic == null;
                        if (flag5)
                        {
                            result = false;
                        }
                        else
                        {
                            bool flag6 = pawn.Drawer.renderer.graphics.nakedGraphic == null;
                            if (flag6)
                            {
                                result = false;
                            }
                            else
                            {
                                bool flag7 = pawn.Drawer.renderer.graphics.headGraphic.path == null;
                                if (flag7)
                                {
                                    result = false;
                                }
                                else
                                {
                                    bool flag8 = pawn.Drawer.renderer.graphics.nakedGraphic.path == null;
                                    if (flag8)
                                    {
                                        result = false;
                                    }
                                    else
                                    {
                                        Graphic nakedBodyGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, ZombieMod_Utility.zombieSkinColor);
                                        Graphic headGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.HeadGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, ZombieMod_Utility.zombieSkinColor);
                                        pawn.Drawer.renderer.graphics.headGraphic = headGraphic;
                                        pawn.Drawer.renderer.graphics.nakedGraphic = nakedBodyGraphic;
                                        bool flag9 = ((ZombiePawn)pawn).isRaiding && UnityEngine.Random.value < 0.4f;
                                        if (flag9)
                                        {
                                            ((ZombiePawn)pawn).isRaiding = false;
                                            ((ZombiePawn)pawn).wasRaiding = true;
                                        }
                                        PawnDownedWiggler wiggler = pawn.Drawer.renderer.wiggler;
                                        bool flag10 = ((ZombiePawn)pawn).CanReanimateZ();
                                        if (flag10)
                                        {
                                            float num = 0f;
                                            wiggler.SetToCustomRotation(Mathf.SmoothDampAngle(wiggler.downedAngle, 0f, ref num, 1.35f));
                                            bool flag11 = wiggler.downedAngle < 1f || wiggler.downedAngle > 359f;
                                            if (flag11)
                                            {
                                                wiggler.SetToCustomRotation(0f);
                                                result = true;
                                                return result;
                                            }
                                        }
                                        result = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

    }

    internal class ZombiePawn : Pawn
    {
        private bool setZombie;

        public bool isRaiding = true;

        public bool wasRaiding;

        public bool wasColonist;

        public string originalNick = "";

        public float notRaidingAttackRange = 15f;

        public ZombiePawn()
        {
            this.Init();
        }

        private void Init()
        {
            this.setZombie = false;
            this.pather = new Pawn_PathFollower(this);
            this.stances = new Pawn_StanceTracker(this);
            this.health = new Pawn_HealthTracker(this);
            this.jobs = new Pawn_JobTracker(this);
            this.filth = new Pawn_FilthTracker(this);
            this.notRaidingAttackRange = UnityEngine.Random.Range(15f, 250f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref this.isRaiding, "isRaiding", true, false);
            Scribe_Values.LookValue<bool>(ref this.wasColonist, "wasColonist", false, false);
            Scribe_Values.LookValue<string>(ref this.originalNick, "originalNick", "Zombie", false);
            Scribe_Values.LookValue<float>(ref this.notRaidingAttackRange, "notRaidingAttackRange", 100f, false);
        }

        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            this.health.PreApplyDamage(dinfo, out absorbed);
            bool flag = !base.Destroyed && (dinfo.Def == DamageDefOf.Cut || dinfo.Def == DamageDefOf.Stab);
            if (flag)
            {
                float num = 0f;
                float num2 = 0f;
                bool flag2 = dinfo.Instigator != null && dinfo.Instigator is Pawn && ((Pawn)dinfo.Instigator).IsColonist;
                if (flag2)
                {
                    Pawn pawn = dinfo.Instigator as Pawn;
                    bool flag3 = pawn.skills != null;
                    if (flag3)
                    {
                        SkillRecord skill = pawn.skills.GetSkill(SkillDefOf.Melee);
                        num = (float)(skill.level * 2);
                        num2 = (float)skill.level / 20f * 3f;
                    }
                    bool flag4 = UnityEngine.Random.Range(0f, 100f) < 20f + num;
                    if (flag4)
                    {
                        dinfo.SetAmount(999);
                        dinfo.SetPart(new BodyPartDamageInfo(health.hediffSet.GetBrain(), false, HediffDefOf.Cut));
                        dinfo.Def.Worker.Apply(dinfo, this);
                    }
                    else
                    {
                        dinfo.SetAmount((int)((float)dinfo.Amount * (1f + num2)));
                    }
                }
            }
        }

        public bool CanReanimateZ()
        {
            bool flag = this.health.hediffSet.GetBrain() == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                List<BodyPartRecord> list = new List<BodyPartRecord>(this.health.hediffSet.GetInjuredParts());
                bool flag2 = list == null;
                if (flag2)
                {
                    result = true;
                }
                else
                {
                    foreach (BodyPartRecord current in list)
                    {
                        bool flag3 = current.def == this.health.hediffSet.GetBrain().def;
                        if (flag3)
                        {
                            result = false;
                            return result;
                        }
                    }
                    result = true;
                }
            }
            return result;
        }

        public override void Tick()
        {
            try
            {
                base.Tick();
                bool flag = this.needs != null && this.needs.food != null && this.needs.food.CurLevel <= 0.95f;
                if (flag)
                {
                    this.needs.food.CurLevel = (1f);
                }
                bool flag2 = this.needs != null && this.needs.joy != null && this.needs.joy.CurLevel <= 0.95f;
                if (flag2)
                {
                    this.needs.joy.CurLevel = (1f);
                }
                bool flag3 = this.needs != null && this.needs.beauty != null && this.needs.beauty.CurLevel <= 0.95f;
                if (flag3)
                {
                    this.needs.beauty.CurLevel = (1f);
                }
                bool flag4 = this.needs != null && this.needs.comfort != null && this.needs.comfort.CurLevel <= 0.95f;
                if (flag4)
                {
                    this.needs.comfort.CurLevel = (1f);
                }
                bool flag5 = this.needs != null && this.needs.rest != null && this.needs.rest.CurLevel <= 0.95f;
                if (flag5)
                {
                    this.needs.rest.CurLevel = (1f);
                }
                bool flag6 = this.needs != null && this.needs.mood != null && this.needs.mood.CurLevel <= 0.45f;
                if (flag6)
                {
                    this.needs.mood.CurLevel = (0.5f);
                }
                bool flag7 = !this.setZombie;
                if (flag7)
                {
                    this.mindState.mentalStateHandler.neverFleeIndividual = true;
                    ZombieMod_Utility.SetZombieName(this);
                    this.setZombie = ZombieMod_Utility.Zombify(this);
                }
                bool flag8 = base.Downed || this.health.Downed || this.health.InPainShock;
                if (flag8)
                {
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, 9999, this, null, null);
                    damageInfo.SetPart(new BodyPartDamageInfo(health.hediffSet.GetBrain(), false, HediffDefOf.Cut));
                    base.TakeDamage(damageInfo);
                }
                bool flag9 = !this.stances.FullBodyBusy;
                if (flag9)
                {
                    try
                    {
                        this.pather.PatherTick();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(string.Concat(new object[]
                        {
                            "ZA: Error met with pather of ",
                            this,
                            ": \n",
                            ex
                        }));
                    }
                }
                base.Drawer.DrawTrackerTick();
                this.health.HealthTick();
                this.stances.StanceTrackerTick();
                bool flag10 = this.equipment != null;
                if (flag10)
                {
                    this.equipment.EquipmentTrackerTick();
                }
                bool flag11 = this.apparel != null;
                if (flag11)
                {
                    this.apparel.ApparelTrackerTick();
                }
                bool flag12 = this.jobs != null;
                if (flag12)
                {
                    bool flag13 = this.jobs.curJob != null && this.jobs.curJob.def == JobDefOf.AttackMelee && this.jobs.curJob.targetA == null && this.mindState.enemyTarget == null;
                    if (flag13)
                    {
                        Log.Error("Target is null");
                    }
                    this.jobs.JobTrackerTick();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}


