using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raft.Persistance.Journaler.Extensions
{
    public static class DictionaryExtensions
    {
        public static string Stringify(this IDictionary<string, string> dict)
        {
            if (dict == null)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var kvp in dict)
                sb.Append(string.Format("{0}={1};", kvp.Key, kvp.Value));

            return sb.ToString();
        }

        public static void PopulateFrom(this IDictionary<string, string> dict, string keyValueString)
        {
            if (dict == null)
                throw new ArgumentException("Dictionary must not be null.");

            if (string.IsNullOrWhiteSpace(keyValueString))
                throw new ArgumentException("Key/Value string must not be null.");

            keyValueString
                .TrimEnd(';').Split(';')
                .Select(item =>
                {
                    var split = item.Split('=');
                    if (split.Length != 2)
                        throw new InvalidOperationException("Key/Value string was not in the correct format.");

                    return new
                    {
                        Key = split[0],
                        Value = split[1]
                    };
                }).ToList().ForEach(x =>
                    dict.Add(x.Key, x.Value));
        }
    }
}
