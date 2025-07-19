using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using UnityEngine.UIElements;
using Vasi;

namespace GalienIllusionist
{
    public class GalienIllusionist : Mod
    {
        public GalienIllusionist() : base("GalienIllusionist") { }

        public override string GetVersion() => "v1.0.0";

        public override void Initialize()
        {
            On.PlayMakerFSM.Start += PlayMakerFSM_Galien;
            On.HutongGames.PlayMaker.Actions.CreateObject.OnEnter += CreateObject_OnEnter;
        }
        private void CreateObject_OnEnter(On.HutongGames.PlayMaker.Actions.CreateObject.orig_OnEnter orig, HutongGames.PlayMaker.Actions.CreateObject self)
        {
            orig(self);

            if (self.Fsm.GameObject.name == "Ghost Warrior Galien" &&
            self.Fsm.Name == "Summon Minis" &&
            (self.State.Name == "Copy Summon" || self.State.Name == "Copy Summon 2"))
            {
                GameObject sphere = self.storeObject.Value;
                sphere.GetComponent<DamageHero>().damageDealt = 0;
            }
        }
        public void PlayMakerFSM_Galien(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Ghost Warrior Galien" && self.FsmName == "Summon Minis")
            {
                self.CopyState("Summon", "Copy Summon");
                self.ChangeTransition("Summon", "FINISHED", "Copy Summon");
                self.ChangeTransition("Copy Summon", "FINISHED", "Idle 2");

                self.CopyState("Summon 2", "Copy Summon 2");
                self.GetState("Summon 2").AddTransition("FINISHED", "Copy Summon 2");
                self.ChangeTransition("Copy Summon 2", "FINISHED", "Summon 2");
            }
        }
    }
}