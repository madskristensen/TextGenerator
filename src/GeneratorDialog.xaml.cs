﻿using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using NLipsum.Core;

namespace MadsKristensen.TextGenerator
{
    public partial class GeneratorDialog : Window
    {
        private SettingsManager _settings;

        public GeneratorDialog(IServiceProvider serviceProvider, int length)
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                _settings = new ShellSettingsManager(serviceProvider);
                Icon = BitmapFrame.Create(new Uri("pack://application:,,,/TextGenerator;component/Resources/images.png", UriKind.RelativeOrAbsolute));

                Title = VSPackage.Name;

                SetLipsumTypes();
                SetSelection();

                txLength.Text = (length == 0 ? 30 : length).ToString();
                txLength.Focus();
                txLength.SelectAll();
            };

            PreviewKeyDown += (a, b) =>
            {
                if (b.Key == Key.Escape)
                    Close();
            };
        }

        public string Text { get; private set; }

        private void SetSelection()
        {
            try
            {
                SettingsStore store = _settings.GetReadOnlySettingsStore(SettingsScope.UserSettings);
                int index = store.GetInt32(VSPackage.Name, "type", 0);
                cbType.SelectedIndex = index;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void SaveSelection()
        {
            try
            {
                WritableSettingsStore wstore = _settings.GetWritableSettingsStore(SettingsScope.UserSettings);

                if (!wstore.CollectionExists(VSPackage.Name))
                    wstore.CreateCollection(VSPackage.Name);

                wstore.SetInt32(VSPackage.Name, "type", cbType.SelectedIndex);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void SetLipsumTypes()
        {
            Type type = typeof(Lipsums);

            foreach (var p in type.GetProperties())
            {
                string name = Prettify(p.Name);
                cbType.Items.Add(name);
            }
        }

        private string Prettify(string text)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in text)
            {
                if (char.IsUpper(c))
                    sb.Append(" ");

                sb.Append(c);
            }

            return sb.ToString().Trim();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            SaveSelection();
            int length;

            if (int.TryParse(txLength.Text, out length))
            {
                Type type = typeof(Lipsums);
                string propName = ((string)cbType.SelectedItem).Replace(" ", string.Empty);
                var prop = type.GetProperty(propName);
                string vocab = (string)prop.GetValue(null, null);

                var generator = new LipsumGenerator(vocab, false);
                string[] words = generator.GenerateWords(length);

                Telemetry.TrackEvent(propName);

                Text = string.Join(" ", words);

                DialogResult = true;
            }
        }
    }
}