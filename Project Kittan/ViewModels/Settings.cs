using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    public class Settings : BindableBase
    {
        private int _customEncoding;
        public int CustomEncoding
        {
            get => _customEncoding;
            set
            {
                SetProperty(ref _customEncoding, value);
                SetProperty(ref _customEncodingName, Encoding.GetEncoding(CustomEncoding).EncodingName);
            }
        }

        private string _customEncodingName;
        public string CustomEncodingName
        {
            get => _customEncodingName;
            set => SetProperty(ref _customEncodingName, value);
        }

        public List<string> Locales { get; set; } = new List<string>();

        public string CurrentLocale
        {
            get => Thread.CurrentThread.CurrentCulture.Name;
        }

        public string CurrentLocaleDateFormat
        {
            get => Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
        }

        private int _customLocale;
        public int CustomLocale
        {
            get => _customLocale;
            set => SetProperty(ref _customLocale, value);
        }

        private string _customLocaleName;
        public string CustomLocaleName
        {
            get => _customLocaleName;
            set => SetProperty(ref _customLocaleName, value);
        }

        private string _customLocaleDateFormat;
        public string CustomLocaleDateFormat
        {
            get => _customLocaleDateFormat;
            set => SetProperty(ref _customLocaleDateFormat, value);
        }

        public string AppVersion
        {
            get => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public ICommand EncodingSettingsFromCodeCommand { get; set; }
        public ICommand LocaleSettingsCommand { get; set; }
        public ICommand ResetSettingsCommand { get; set; }

        public Settings()
        {
            EncodingSettingsFromCodeCommand = new DelegateCommand(new Action<object>(EncodingSettingsFromCode_Action));
            LocaleSettingsCommand = new DelegateCommand(new Action<object>(LocaleSettings_Action));
            ResetSettingsCommand = new DelegateCommand(new Action<object>(ResetSettings_Action));


            CustomEncoding = Properties.Settings.Default.DefaultEncoding;

            Locales = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(culture => culture.Name).Distinct().ToList();
            CustomLocale = Locales.IndexOf(Properties.Settings.Default.DefaultLocale);
            CultureInfo cultureInfo = new CultureInfo(Properties.Settings.Default.DefaultLocale, false);
            CustomLocaleName = cultureInfo.Name;
            CustomLocaleDateFormat = cultureInfo.DateTimeFormat.ShortDatePattern;
        }

        public void EncodingSettingsFromCode_Action(object obj)
        {
            int.TryParse((string)obj, out int encodingCode);
            try
            {
                Encoding encoding = Encoding.GetEncoding(encodingCode);

                Properties.Settings.Default.DefaultEncoding = encodingCode;
                Properties.Settings.Default.Save();

                CustomEncoding = encodingCode;
                CustomEncodingName = encoding.EncodingName;
            }
            catch (Exception)
            {
                return;
            }
        }

        public void LocaleSettings_Action(object obj)
        {
            string locale = (string)obj;

            Properties.Settings.Default.DefaultLocale = locale;
            Properties.Settings.Default.Save();

            CustomLocale = Locales.IndexOf(locale);

            CultureInfo cultureInfo = new CultureInfo(Properties.Settings.Default.DefaultLocale, false);
            CustomLocaleName = cultureInfo.Name;
            CustomLocaleDateFormat = cultureInfo.DateTimeFormat.ShortDatePattern;
        }

        public void ResetSettings_Action(object obj)
        {
            Properties.Settings.Default.Reset();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
