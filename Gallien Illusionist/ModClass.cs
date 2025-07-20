using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using Vasi;

namespace GalienIllusionist
{
    public class GalienIllusionist : Mod
    {
        private static List<SpriteRenderer> SpritesToFade = new List<SpriteRenderer>();

        public GalienIllusionist() : base("GalienIllusionist") { }

        public override string GetVersion() => "v1.0.1";

        public override void Initialize()
        {
            On.PlayMakerFSM.Start += PlayMakerFSM_Galien;
            On.HutongGames.PlayMaker.Actions.CreateObject.OnEnter += CreateObject_OnEnter;
            ModHooks.HeroUpdateHook += Update;
        }

        // Эта функция вообще полностью про прозрачность
        private void Update()
        {
            for (int i = SpritesToFade.Count - 1; i >= 0; i--)
            {
                SpriteRenderer sr = SpritesToFade[i];
                if (sr == null)
                {
                    SpritesToFade.RemoveAt(i);
                    continue;
                }

                Color c = sr.color;
                if (c.a < 1f)
                {
                    // Плавное появление за 5 секунд
                    c.a += Time.deltaTime / 5f;
                    sr.color = c;
                }
                else
                {
                    SpritesToFade.RemoveAt(i);
                }
            }
        }

        private void CreateObject_OnEnter(On.HutongGames.PlayMaker.Actions.CreateObject.orig_OnEnter orig, HutongGames.PlayMaker.Actions.CreateObject self)
        {
            orig(self);

            if (self.Fsm.GameObject.name == "Ghost Warrior Galien" &&
                self.Fsm.Name == "Summon Minis" &&
                (self.State.Name == "Copy Summon" || self.State.Name == "Copy Summon 2"))
            {
                // Копия не наносит урона
                GameObject sphere = self.storeObject.Value;
                if (sphere == null) return;

                DamageHero damageHero = sphere.GetComponent<DamageHero>();
                if (damageHero != null)
                {
                    damageHero.damageDealt = 0;
                }

                // Находим спрайт и делаем его прозрачным
                SpriteRenderer transparency = sphere.GetComponentInChildren<SpriteRenderer>();
                if (transparency != null)
                {
                    Color newColor = transparency.color;
                    newColor.a = 0.4f; // Начинаем с полной прозрачности
                    transparency.color = newColor;

                    SpritesToFade.Add(transparency);
                }
            }
        }

        public void PlayMakerFSM_Galien(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Ghost Warrior Galien" && self.FsmName == "Summon Minis")
            {
                // Первая сфера
                self.CopyState("Summon", "Copy Summon");

                FsmState copySummonState = self.GetState("Copy Summon");
                if (copySummonState != null)
                {
                    CreateObject createAction = copySummonState.GetAction<CreateObject>();
                    if (createAction != null)
                    {
                        createAction.position.Value = new Vector3(3f, 0f, 0.029f);
                    }

                    if (self.GetState("Summon") != null)
                    {
                        self.ChangeTransition("Summon", "FINISHED", "Copy Summon");
                    }

                    self.ChangeTransition("Copy Summon", "FINISHED", "Idle 2");
                }

                // Вторая сфера
                self.CopyState("Summon 2", "Copy Summon 2");

                FsmState copySummon2State = self.GetState("Copy Summon 2");
                if (copySummon2State != null)
                {
                    CreateObject createAction2 = copySummon2State.GetAction<CreateObject>();
                    if (createAction2 != null)
                    {
                        createAction2.position.Value = new Vector3(3f, 0f, 0.029f);
                    }

                    if (self.GetState("Summon 2") != null)
                    {
                        self.GetState("Summon 2").AddTransition("FINISHED", "Copy Summon 2");
                    }

                    self.ChangeTransition("Copy Summon 2", "FINISHED", "Summon 2");
                }
            }
        }
    }
}