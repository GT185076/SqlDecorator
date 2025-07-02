﻿using System;
using System.Collections.Generic;

namespace SQLDecorator.Composer
{
    internal class Implementation
    {
        internal Type ClassType;
        internal bool SingleTon;
        internal object Instance;
    }

    static public class Resolver<T>
    {
        static Dictionary<KeyValuePair<Type, string>, Implementation> _catalog = new Dictionary<KeyValuePair<Type, string>, Implementation>();
        static public void Register<ImpT>(bool SingleTon = false)
        {
            var tkey = new KeyValuePair<Type, string>(typeof(T), "");
            var imp = new Implementation { ClassType = typeof(ImpT), SingleTon = SingleTon };
            _catalog.Add(tkey, imp);
        }
        static public void Register<ImpT>(string ImpKey, bool SingleTon = false)
        {
            var tkey = new KeyValuePair<Type, string>(typeof(T), ImpKey);
            var imp = new Implementation { ClassType = typeof(ImpT), SingleTon = SingleTon };
            _catalog.Add(tkey, imp);
        }
        static public T Resolve()
        {
            var emptyTypes = new Type[0];
            var tkey = new KeyValuePair<Type, string>(typeof(T), "");
            if (_catalog[tkey].SingleTon && _catalog[tkey].Instance != null)
                return (T)_catalog[tkey].Instance;
            else
            {
                _catalog[tkey].Instance = _catalog[tkey].ClassType.GetConstructor(emptyTypes).Invoke(null);
                return (T)_catalog[tkey].Instance;
            }
        }
        static public T Resolve(string Impkey)
        {
            var emptyTypes = new Type[0];
            var tkey = new KeyValuePair<Type, string>(typeof(T), Impkey);

            if (_catalog[tkey].SingleTon && _catalog[tkey].Instance != null)
                return (T)_catalog[tkey].Instance;
            else
            {
                _catalog[tkey].Instance = _catalog[tkey].ClassType.GetConstructor(emptyTypes).Invoke(null);
                return (T)_catalog[tkey].Instance;
            }
        }
    }
}
