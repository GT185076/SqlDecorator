using System;
using System.Collections.Generic;

namespace Composer
{
    static public class Resolver<T> 
    {
        static Resolver()
        {
            Installer.Init();
        }

        static Dictionary<KeyValuePair<Type,string>,Type> _catalog   =  new Dictionary<KeyValuePair<Type, string>, Type> ();      
        static public void Register<ImpT>()
        {
            var tkey =  new KeyValuePair<Type, string>(typeof(T),"");
            _catalog.Add(tkey, typeof(ImpT));
        }
        static public void Register<ImpT>(string ImpKey)
        {
            var tkey = new KeyValuePair<Type, string>(typeof(T), ImpKey);
            _catalog.Add(tkey, typeof(ImpT));
        }
        static public T Resolve()
        {
            var emptyTypes = new Type[0];
            var tkey = new KeyValuePair<Type, string>(typeof(T),"");
            return  (T)(_catalog[tkey]).GetConstructor(emptyTypes).Invoke(null);
        }
        static public T Resolve(string Impkey)
        {
            var emptyTypes = new Type[0];
            var tkey = new KeyValuePair<Type, string>(typeof(T), Impkey);
            return (T)_catalog[tkey].GetConstructor(emptyTypes).Invoke(null);
        }
    }
}
