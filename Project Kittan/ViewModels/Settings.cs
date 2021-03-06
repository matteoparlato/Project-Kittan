﻿using Project_Kittan.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    /// <summary>
    /// Settings class
    /// </summary>
    public class Settings : Observable
    {
        public List<string> Encodings { get; set; }

        private int _customEncoding;
        public int CustomEncoding
        {
            get => _customEncoding;
            set => Set(ref _customEncoding, value);
        }

        public List<string> Locales { get; set; }

        public string CurrentLocale => Thread.CurrentThread.CurrentCulture.Name;

        public string CurrentLocaleDateFormat => Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;

        private int _customLocale;
        public int CustomLocale
        {
            get => _customLocale;
            set => Set(ref _customLocale, value);
        }

        public CultureInfo CustomLocaleCulture { get; set; }

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public ICommand EncodingSettingsCommand { get; set; }
        public ICommand LocaleSettingsCommand { get; set; }
        public ICommand ResetSettingsCommand { get; set; }

        public Settings()
        {
            EncodingSettingsCommand = new RelayCommand<object>(EncodingSettings_Action);
            LocaleSettingsCommand = new RelayCommand<object>(LocaleSettings_Action);
            ResetSettingsCommand = new RelayCommand<object>(ResetSettings_Action);

            Encodings = Encoding.GetEncodings().Select(encoding => encoding.Name).Distinct().ToList();
            CustomEncoding = Encodings.IndexOf(Properties.Settings.Default.DefaultEncoding);

            Locales = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(culture => culture.Name).Distinct().ToList();
            CustomLocale = Locales.IndexOf(Properties.Settings.Default.DefaultLocale);
            CustomLocaleCulture = new CultureInfo(Properties.Settings.Default.DefaultLocale, false);
        }

        private void EncodingSettings_Action(object obj)
        {
            string encodingName = (string)obj;

            Properties.Settings.Default.DefaultEncoding = encodingName;
            Properties.Settings.Default.Save();

            CustomEncoding = Encodings.IndexOf(encodingName);
        }

        private void LocaleSettings_Action(object obj)
        {
            string locale = (string)obj;

            Properties.Settings.Default.DefaultLocale = locale;
            Properties.Settings.Default.Save();

            CustomLocale = Locales.IndexOf(locale);

            CustomLocaleCulture = new CultureInfo(Properties.Settings.Default.DefaultLocale, false);
        }

        private void ResetSettings_Action(object obj)
        {
            Properties.Settings.Default.Reset();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
