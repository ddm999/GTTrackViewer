using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;

namespace GTTrackEditor
{
    public static class TrackEditorConfig
    {
        static Configuration _config;
        static KeyValueConfigurationCollection _settings;
        public static bool Loaded { get; private set; }
        public static bool Modified { get; private set; }

        public static void Init()
        {
            try
            {
                _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                _settings = _config.AppSettings.Settings;
                Loaded = true;

            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }

            InitializeDefaultValues();
        }

        private static void InitializeDefaultValues()
        {
            SetSettingIfNotExists("EnableGrid", "True");
            SetSettingIfNotExists("RotateMode", "False");

            Save();
        }

        public static void SetSettingAndSave(string key, string value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value);
            else
                settings[key].Value = value;

            Modified = true;
            Save();
        }

        public static void Save()
        {
            if (!Modified)
                return;

            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(_config.AppSettings.SectionInformation.Name);

            Modified = false;
        }

        public static bool TryGetString(string key, out string value, string defaultValue)
        {
            var confvalue = _settings[key];
            if (confvalue is null)
            {
                value = defaultValue;
                return false;
            }

            value = confvalue.Value;
            return true;
        }

        public static bool TryGetLong(string key, out long value, long defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !long.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetULong(string key, out ulong value, ulong defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !ulong.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetInt(string key, out int value, int defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !int.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetUInt(string key, out uint value, uint defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !uint.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetShort(string key, out short value, short defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !short.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetUShort(string key, out ushort value, ushort defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !ushort.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetByte(string key, out byte value, byte defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !byte.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetSByte(string key, out sbyte value, sbyte defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !sbyte.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetBool(string key, out bool value, bool defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !bool.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static bool TryGetFloat(string key, out float value, float defaultValue)
        {
            KeyValueConfigurationElement confValue = _settings[key];
            if (confValue is null || !float.TryParse(confValue.Value, out value))
            {
                value = defaultValue;
                return false;
            }

            return true;
        }

        public static void SetSettingIfNotExists(string key, string value)
        {
            if (_settings[key] != null)
                return;

            SetSetting(key, value);
        }

        public static void SetSetting(string key, string value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value);
            else
                settings[key].Value = value;

            Modified = true;
        }

        public static void SetSetting(string key, bool value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString());
            else
                settings[key].Value = value.ToString();

            Modified = true;
        }

        public static void SetSetting(string key, int value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, uint value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, short value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, ushort value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, byte value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, sbyte value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, long value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, ulong value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }

        public static void SetSetting(string key, float value)
        {
            KeyValueConfigurationCollection settings = _config.AppSettings.Settings;

            if (_settings[key] == null)
                settings.Add(key, value.ToString(CultureInfo.InvariantCulture));
            else
                settings[key].Value = value.ToString(CultureInfo.InvariantCulture);

            Modified = true;
        }
    }
}
