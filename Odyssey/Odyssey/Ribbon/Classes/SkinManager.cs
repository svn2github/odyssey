using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#region Copyright
// Odyssey.Controls.Ribbonbar
// (c) copyright 2009 Thomas Gerber
// This source code and files, is licensed under The Microsoft Public License (Ms-PL)
#endregion
namespace Odyssey.Controls.Classes
{
    public static class SkinManager
    {
        private static SkinId skinId = SkinId.OfficeBlue;

        public static SkinId SkinId
        {
            get { return skinId; }
            set
            {
                if (skinId != value)
                {
                    skinId = value;
                    ApplySkin(value);
                }
            }
        }

        private static void ApplySkin(SkinId skin)
        {
            var dict = Application.Current.Resources.MergedDictionaries;
           dict.Remove(OfficeBlack);
           dict.Remove(OfficeSilver);
           dict.Remove(WindowsSeven);
           switch (skin)
           {
               case SkinId.OfficeBlack:
                   dict.Add(OfficeBlack);
                   break;

               case SkinId.OfficeSilver:
                   dict.Add(OfficeSilver);
                   break;

               case SkinId.Windows7:
                   dict.Add(WindowsSeven);
                   break;
           }           
        }

        private static ResourceDictionary officeSilver;
        public  static ResourceDictionary OfficeSilver
        {
            get
            {
                if (officeSilver == null)
                {
                    ResourceDictionary dictionary = new ResourceDictionary();
                    dictionary.Source = new Uri("/Odyssey;Component/Skins/Ribbon/OfficeSilverSkin.xaml", UriKind.Relative);
                    officeSilver = dictionary;
                }
                return officeSilver;
            }
        }

        private static ResourceDictionary officeBlack;
        public static ResourceDictionary OfficeBlack
        {
            get
            {
                if (officeBlack == null)
                {
                    ResourceDictionary dictionary = new ResourceDictionary();
                    dictionary.Source = new Uri("/Odyssey;Component/Skins/Ribbon/OfficeBlackSkin.xaml", UriKind.Relative);
                    officeBlack = dictionary;
                }
                return officeBlack;
            }
        }

        private static ResourceDictionary windowSeven;
        public static ResourceDictionary WindowsSeven
        {
            get
            {
                if (windowSeven == null)
                {
                    ResourceDictionary dictionary = new ResourceDictionary();
                    dictionary.Source = new Uri("/Odyssey;Component/Skins/Ribbon/Window7Skin.xaml", UriKind.Relative);
                    windowSeven = dictionary;
                }
                return windowSeven;
            }
        }
        
    }
}
