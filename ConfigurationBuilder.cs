using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VocabT
{
    public static class ConfigurationBuilder
    {
        private static readonly JObject _configuration;
        public static Dictionary<string, JToken> Configuration =>
            _configuration.Properties().ToDictionary(x => x.Name, x => x.Value);

        static ConfigurationBuilder()
        {
            var configText = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "config.json"));
            _configuration = JObject.Parse(configText);
        }
        
        public static T ToObject<T>(this string name) => Configuration[name].ToObject<T>();
    }
}
