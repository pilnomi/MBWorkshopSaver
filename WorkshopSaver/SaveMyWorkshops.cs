using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
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
          int capital,
          bool upgradable,
          TextObject customName
            )
        {
            if (!workshopsSaved.Contains(workshop))
                workshopsSaved.Add(workshop);
        }

        public class workshopworker : CampaignBehaviorBase
        {
            public override void SyncData(IDataStore dataStore)
            {
            }

            public override void RegisterEvents()
            {
                Configuration config = null;
                string showtextmessage = "true";
                string textmessage = "WorkshopSaver: Saved {0} workshop(s)!";

                string exeConfigPath = this.GetType().Assembly.Location;
                try
                {
                    config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
                }
                catch (Exception ex)
                {
                    //handle errror here.. means DLL has no sattelite configuration file.
                }

                if (config != null)
                {
                    showtextmessage = GetAppSetting(config, "ShowTextMessage");
                    textmessage = GetAppSetting(config, "TextMessageOnWarDeclared");
                }

                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(
                    (faction1, faction2) =>
                    {
                        if (Hero.MainHero.MapFaction == faction1 ||
                            Hero.MainHero.MapFaction == faction2
                        )
                        {
                            bool b = false;
                            bool.TryParse(showtextmessage, out b);
                            if (b)
                            {
                                InformationManager.DisplayMessage(new InformationMessage(
                                    string.Format(textmessage, workshopsSaved.Count)
                                )); 
                            }
                        }
                        workshopsSaved = new List<Workshop>();
                    }));
            }

            string GetAppSetting(Configuration config, string key)
            {
                KeyValueConfigurationElement element = config.AppSettings.Settings[key];
                if (element != null)
                {
                    string value = element.Value;
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }
                return string.Empty;
            }
        }

    }
}
