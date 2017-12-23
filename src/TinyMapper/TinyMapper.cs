using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace TinyMapper
{
    /// <summary>
    /// A tiny object to object mapping and conversion utility, weighing less than 300 PLOC. Similar
    /// to Automapper, but exposes the mapping metadata making it useful for other things, such as dynamic
    /// expression tree translation of Linq queries. Thread safe.
    /// </summary>
    public class TinyMapper
    {
        private static Lazy<TinyMapper> _global = new Lazy<TinyMapper>(() => new TinyMapper());

        /// <summary>
        /// Singleton, usage:
        /// using static TinyMapper.TinyMapper;
        /// ModelMapper.Map<T>(foo);
        /// </summary>
        public static TinyMapper ModelMapper => _global.Value;

        private ConcurrentDictionary<string, Converter> _registered =
            new ConcurrentDictionary<string, Converter>();

        /// <summary>
        /// This constructor is hidden, use the singleton instance
        /// </summary>
        private TinyMapper()
        {
            // intentionally blank
        }

        /// <summary>
        /// Begins the creation of a type mapping definition.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ConversionMapping<T1, T2> CreateMap<T1, T2>(Action<ConversionMapping<T1, T2>> configuration = null)
        {
            var result = new ConversionMapping<T1, T2>();
            var converter = new Converter<T1, T2>(result);
            // full duplex conversion, so register both directions
            _registered.TryAdd(Key(typeof(T1), typeof(T2)), converter);
            _registered.TryAdd(Key(typeof(T2), typeof(T1)), converter);

            configuration?.Invoke(result);

            return result;
        }


        /// <summary>
        /// Converts value to T using the mapping definition.
        /// If no mapping definition is found, returns default(T)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Map<T>(object value)
        {

            if (value == null)
            {
                return default(T);
            }
            // run this thing through our conversion
            var valueType = value.GetType();
            var key = Key(valueType, typeof(T));
            // try to get the converter based on the constructed key
            if (_registered.TryGetValue(key, out var converter))
            {
                return (T)converter.Convert(value);
            }
            return default(T);
        }

        /// <summary>
        /// /// Converts value to T using the mapping definition.
        /// If no mapping definition is found, returns default(T). 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionCallback">a callback action to invoke on the converted item</param>
        /// <returns></returns>
        public T Map<T>(object value, Action<T> conversionCallback)
        {
            var result = Map<T>(value);
            conversionCallback?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Translates an expression from the T1 mapped type to a synonomous expression against T2 members
        /// </summary>
        /// <param name="source">The query expression wherein TFrom is a mapped type</param>
        /// <returns>An expression against the member mapped TTo properties</returns>
        public Expression<Func<TTo, TResult>> Translate<TFrom, TTo, TResult>(Expression<Func<TFrom, TResult>> source)
        {
            var param = Expression.Parameter(typeof(TTo), source.Parameters.First().Name);
            var visitor = new Visitor<TFrom, TTo>(this, param);
            var body = visitor.Visit(source.Body);
            var lambda = Expression.Lambda(body, param);
            return (Expression<Func<TTo, TResult>>)lambda;
        }

        /// <summary>
        /// Finds the embedded conversion mapping for T1 and T2
        /// </summary>
        /// <returns></returns>
        public ConversionMapping<T1, T2> Find<T1, T2>()
        {
            if (_registered.TryGetValue(Key(typeof(T1), typeof(T2)), out var result))
            {
                return ((Converter<T1, T2>)result).Mapping;
            }
            return null;
        }

        private string Key(Type t1, Type t2) => $"{t1.FullName}->{t2.FullName}";
    }
}