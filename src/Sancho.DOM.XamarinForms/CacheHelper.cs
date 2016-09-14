using System;
using System.Collections.Generic;

namespace Sancho.DOM.XamarinForms
{
    public static class CacheHelper
    {
        public static Func<T1, TReturn> Cache<T1, TReturn>(Func<T1, TReturn> func)
        {
            var d = new Dictionary<Tuple<T1>, TReturn>();
            return (arg1) =>
            {
                TReturn result;
                var arg = Tuple.Create(arg1);
                if (d.TryGetValue(arg, out result))
                {
                    return result;
                }

                result = func(arg1);
                d.Add(arg, result);
                return result;
            };
        }

        public static Func<T1, T2, TReturn> Cache<T1, T2, TReturn>(Func<T1, T2, TReturn> func)
        {
            var d = new Dictionary<Tuple<T1, T2>, TReturn>();
            return (arg1, arg2) =>
            {
                TReturn result;
                var arg = Tuple.Create(arg1, arg2);
                if (d.TryGetValue(arg, out result))
                {
                    return result;
                }

                result = func(arg1, arg2);
                d.Add(arg, result);
                return result;
            };
        }
    }
}
