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
        public List<string> Encodings { get; set; } = new List<string>();

        private int _customEncoding;
        public int CustomEncoding
        {
            get => _customEncoding;
            set => SetProperty(ref _customEncoding, value);
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

        public CultureInfo CustomLocaleCulture { get; set; }

        public string AppVersion
        {
            get => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public ICommand EncodingSettingsFromCodeCommand { get; set; }
        public ICommand LocaleSettingsCommand { get; set; }
        public ICommand ResetSettingsCommand { get; set; }

        public Settings()
        {
            EncodingSettingsFromCodeCommand = new DelegateCommand(new Action<object>(EncodingSettings_Action));
            LocaleSettingsCommand = new DelegateCommand(new Action<object>(LocaleSettings_Action));
            ResetSettingsCommand = new DelegateCommand(new Action<object>(ResetSettings_Action));

            Encodings = Encoding.GetEncodings().Select(encoding => encoding.Name).Distinct().ToList();
            Encodings.Insert(0, "");
            CustomEncoding = Encodings.IndexOf(Properties.Settings.Default.DefaultEncoding);

            Locales = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(culture => culture.Name).Distinct().ToList();
            CustomLocale = Locales.IndexOf(Properties.Settings.Default.DefaultLocale);
            CustomLocaleCulture = new CultureInfo(Properties.Settings.Default.DefaultLocale, false);
        }

        public void EncodingSettings_Action(object obj)
        {
            string encodingName = (string)obj;

            Properties.Settings.Default.DefaultEncoding = encodingName;
            Properties.Settings.Default.Save();

            CustomEncoding = Encodings.IndexOf(encodingName);
        }

        public void LocaleSettings_Action(object obj)
        {
            string locale = (string)obj;

            Properties.Settings.Default.DefaultLocale = locale;
            Properties.Settings.Default.Save();

            CustomLocale = Locales.IndexOf(locale);

            CustomLocaleCulture = new CultureInfo(Properties.Settings.Default.DefaultLocale, false);
        }

        public void ResetSettings_Action(object obj)
        {
            Properties.Settings.Default.Reset();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
