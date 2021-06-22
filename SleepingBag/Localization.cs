using Jotunn.Configs;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleepingBag
{
    class Localization
    {
        public static void CreateLocalization()
        {
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations = {
                { "item_sleepingbag", "Sleeping bag (packed)"},
                { "item_sleepingbag_description", "Allows you to set your spawn point with less requirements than a bed, and to sleep under the stars if you are lucky with the weather."},
                { "piece_sleepingbag", "Sleeping bag"}
            }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("French")
            {
                Translations = {
                { "item_sleepingbag", "Sac de couchage (replié)"},
                { "item_sleepingbag_description", "Permet de définir le point de spawn avec moins de contraintes qu'un lit, et même de dormir à la belle étoile, si le temps est clément !"},
                { "piece_sleepingbag", "Sac de couchage"}
            }
            });
        }
    }
}