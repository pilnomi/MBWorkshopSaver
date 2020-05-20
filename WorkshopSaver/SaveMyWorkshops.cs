using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using System.Collections;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.CampaignSystem.Actions;
using Helpers;
using SandBox;

namespace WorkshopSaver
{
    public class SaveMyWorkshops : MBSubModuleBase
    {
        public class WorkshopOwners
        {
            public Workshop workshop;
            public Hero hero;

            public WorkshopOwners(Workshop workshop, Hero hero)
            {
                this.workshop = workshop;
                this.hero = hero;
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
                campaignStarter.AddBehavior(new workshopworker());
            }
            base.BeginGameStart(game);
        }


        public class workshopworker : CampaignBehaviorBase
        {
            public List<WorkshopOwners> workshoplist;
            public override void SyncData(IDataStore dataStore)
            {
                //dataStore.SyncData("test", ref workshoplist);
            }

            public void refreshWorkshops()
            {
                workshoplist = new List<WorkshopOwners>();
                Hero myhero = Hero.MainHero;
                IReadOnlyList<Workshop> mylist = myhero.OwnedWorkshops;
                for (int i = 0; i < mylist.Count; i++)
                    workshoplist.Add(new WorkshopOwners(mylist[i], mylist[i].Owner));
            }

            public override void RegisterEvents()
            {
                refreshWorkshops();

                CampaignEvents.HeroOrPartyTradedGold.AddNonSerializedListener(this, new Action<(Hero, PartyBase), (Hero, PartyBase), (int, string), bool>(
                    (s, t, a, b) => 
                    {
                        //needed a way to keep workshops up to date if they are bought and sold
                        //seems safe to assume if money changed hands, then any change in workshop ownership is intentional
                        refreshWorkshops();
                    }));
                
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(
                    (faction1, faction2) =>
                    {
                        //string faction1name = faction1.Name.ToString();
                        //string faction2name = faction2.Name.ToString();
                        int workshopsSaved = 0;
                        for (int i = 0; i < workshoplist.Count; i++)
                        {
                            if (workshoplist[i].workshop.Owner != workshoplist[i].hero)
                            {
                                ChangeOwnerOfWorkshopAction.ApplyByWarDeclaration(
                                    workshoplist[i].workshop,
                                    workshoplist[i].hero,
                                    workshoplist[i].workshop.WorkshopType,
                                    workshoplist[i].workshop.Name,
                                    workshoplist[i].workshop.Capital,
                                    workshoplist[i].workshop.Upgradable);
                                
                                workshopsSaved++;
                            }
                        }
                        InformationManager.DisplayMessage(new InformationMessage("WorkshopSaver: Saved " + workshopsSaved.ToString() + " workshop(s)!"));
                    }));
            }
        }

    }
}
