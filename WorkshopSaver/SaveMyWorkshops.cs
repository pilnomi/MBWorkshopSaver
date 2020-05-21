using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using System.Collections;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace WorkshopSaver
{
    public class SaveMyWorkshops : MBSubModuleBase
    {
        public class WorkshopOwners
        {
            public Workshop workshop;
            public Hero hero;
            public WorkshopType workshoptype;
            public int capital;

            public WorkshopOwners(Workshop workshop, Hero hero, WorkshopType t, int capital)
            {
                this.workshop = workshop;
                this.hero = hero;
                this.workshoptype = t;
                this.capital = capital;
            }
        }

        public static bool returnFalse()
        {
            return false;
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign)
            {
                workshopsSaved = new List<Workshop>();
                var harmony = new HarmonyLib.Harmony("MBWorkshopSaver");
                var original = typeof(ChangeOwnerOfWorkshopAction).GetMethod("ApplyByWarDeclaration");
                var newfunc = typeof(SaveMyWorkshops).GetMethod("ApplyByWarDeclaration");
                var returnFalse = typeof(SaveMyWorkshops).GetMethod("returnFalse");
                harmony.Patch(original, new HarmonyLib.HarmonyMethod(returnFalse), new HarmonyLib.HarmonyMethod(newfunc));

                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
                campaignStarter.AddBehavior(new workshopworker());
            }
            base.BeginGameStart(game);
        }

        static List<Workshop> workshopsSaved;
        public static void ApplyByWarDeclaration(
          Workshop workshop,
          Hero newOwner,
          WorkshopType workshopType,
          TextObject name,
          int capital,
          bool upgradable)
        {
            if (!workshopsSaved.Contains(workshop))
                workshopsSaved.Add(workshop);
        }

        public class workshopworker : CampaignBehaviorBase
        {
            //public List<WorkshopOwners> workshoplist;
            public override void SyncData(IDataStore dataStore)
            {
                //dataStore.SyncData("test", ref workshoplist);
            }
            /*

            public void refreshWorkshops()
            {
                workshoplist = new List<WorkshopOwners>();
                Hero myhero = Hero.MainHero;
                IReadOnlyList<Workshop> mylist = myhero.OwnedWorkshops;
                
                for (int i = 0; i < mylist.Count; i++)
                    workshoplist.Add(new WorkshopOwners(mylist[i], mylist[i].Owner, mylist[i].WorkshopType, mylist[i].Capital));
            }
            */
            public override void RegisterEvents()
            {
                //refreshWorkshops();
                /*
                CampaignEvents.HeroOrPartyTradedGold.AddNonSerializedListener(this, new Action<(Hero, PartyBase), (Hero, PartyBase), (int, string), bool>(
                    (s, t, a, b) => 
                    {
                        //needed a way to keep workshops up to date if they are bought and sold
                        //seems safe to assume if money changed hands, then any change in workshop ownership is intentional
                        refreshWorkshops();
                    }));
                */
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(
                    (faction1, faction2) =>
                    {
                        //string faction1name = faction1.Name.ToString();
                        //string faction2name = faction2.Name.ToString();
                        /*
                        int workshopsSaved = 0;
                        for (int i = 0; i < workshoplist.Count; i++)
                        {
                            if (workshoplist[i].workshop.Owner != workshoplist[i].hero)
                            {
                                
                                ChangeOwnerOfWorkshopAction.ApplyByWarDeclaration(
                                    workshoplist[i].workshop,
                                    workshoplist[i].hero,
                                    workshoplist[i].workshoptype,
                                    workshoplist[i].workshop.Name,
                                    workshoplist[i].capital,
                                    workshoplist[i].workshop.Upgradable);
                                
                                workshopsSaved++;
                            }
                        }
                        */
                        InformationManager.DisplayMessage(new InformationMessage("WorkshopSaver: Saved " + workshopsSaved.Count.ToString() + " workshop(s)!"));
                        workshopsSaved = new List<Workshop>();
                    }));
            }
        }

    }
}
