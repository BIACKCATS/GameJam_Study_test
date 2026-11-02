using UnityEngine;

namespace JSH.Core
{
    /// <summary>
    /// Thin PlayerPrefs wrapper. All keys are prefixed with "JSH." to avoid clashing with teammates.
    /// Complex data: Save/Load with a [System.Serializable] class (JsonUtility).
    /// </summary>
    public static class SaveSystem
    {
        private const string Prefix = "JSH.";

        public static void SetInt(string key, int value) => PlayerPrefs.SetInt(Prefix + key, value);
        public static int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(Prefix + key, defaultValue);

        public static void SetFloat(string key, float value) => PlayerPrefs.SetFloat(Prefix + key, value);
        public static float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(Prefix + key, defaultValue);

        public static void SetString(string key, string value) => PlayerPrefs.SetString(Prefix + key, value);
        public static string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(Prefix + key, defaultValue);

        public static void Save<T>(string key, T data)
        {
            PlayerPrefs.SetString(Prefix + key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static T Load<T>(string key, T defaultValue = default)
        {
            string json = PlayerPrefs.GetString(Prefix + key, string.Empty);
            return string.IsNullOrEmpty(json) ? defaultValue : JsonUtility.FromJson<T>(json);
        }

        public static bool HasKey(string key) => PlayerPrefs.HasKey(Prefix + key);
        public static void Delete(string key) => PlayerPrefs.DeleteKey(Prefix + key);
        public static void Flush() => PlayerPrefs.Save();

        /// <summary>Stores value only if it beats the saved best. Returns true on new record.</summary>
        public static bool TrySetHighScore(string key, int score)
        {
            if (score <= GetInt(key, int.MinValue))
            {
                return false;
            }

            SetInt(key, score);
            PlayerPrefs.Save();
            return true;
        }
    }
}
