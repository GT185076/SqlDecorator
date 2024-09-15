    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

namespace CommonInfra
    {
        public class DictionaryWithDefault<TKey, TValue> : Dictionary<TKey, TValue>
        {
            TValue _default;
            public TValue DefaultValue
            {
                get { return _default; }
                set { _default = value; }
            }
            public DictionaryWithDefault(TValue defaultValue)
                : base()
            {
                _default = defaultValue;
            }
            public new TValue this[TKey key]
            {
                get
                {
                    TValue t;
                    return base.TryGetValue(key, out t) ? t : _default;
                }
                set { base[key] = value; }
            }
        }

    public static class dictionaryExtansion
    {
        public static string GetValue(this Dictionary<int, string> dic, int key)
        {
            string answer = "";
            dic.TryGetValue(key, out answer);
            return answer;
        }

        public static int GetKey(this Dictionary<int, string> dic, string value)
        {
            int k = -1;

            try
            {
                k = dic.First(x => x.Value == value).Key;
            }
            catch
            { };

            return k;
        }

        public static string GetValue(this Dictionary<string, string> dic, string key)
        {
            string answer = "";
            dic.TryGetValue(key, out answer);
            return answer;
        }

        public static string GetKey(this Dictionary<string, string> dic, string value)
        {
            string answer = "";
            answer = dic.First(x => x.Value == value).Key;
            return answer == null ? "" : answer;
        }
    }

}



