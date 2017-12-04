using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TinyMapper
{
    public partial class ConversionMapping<T1, T2>
    {
        /// <summary>
        /// Indicates whether or not the given type has been mapped
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CanMap(Type type) => typeof(T1).IsAssignableFrom(type) || typeof(T2).IsAssignableFrom(type);


        /// <summary>
        /// Gets expressed property mapping metadata
        /// </summary>
        /// <returns></returns>
        public ICollection<Map> Mapping { get; } = new List<Map>();

        /// <summary>
        /// Maps the properties reference by the selector expressions
        /// </summary>
        /// <param name="t1Selector"></param>
        /// <param name="t2Selector"></param>
        /// <returns></returns>
        public ConversionMapping<T1, T2> MapProperty<TProperty>(Expression<Func<T1, TProperty>> t1Selector, Expression<Func<T2, TProperty>> t2Selector)
        {
            Mapping.Add(new Map()
            {
                T1Property = t1Selector,
                T2Property = t2Selector
            });
            return this;
        }

        /// <summary>
        /// Maps the properteis referenced by the selector expressions 
        /// with explicit conversion behaviors
        /// </summary>
        /// <param name="t1Selector">T1 property selector</param>
        /// <param name="t2Selector">T2 property selector</param>
        /// <param name="ltrConverter">Conversion behavior when converting from T1.Foo to T2.Foo</param>
        /// <param name="rtlConverter">Conversion behavior when converting from T2.Foo to T1.Foo</param>
        /// <returns></returns>
        public ConversionMapping<T1, T2> MapProperty<TP1, TP2>(
            Expression<Func<T1, TP1>> t1Selector,
            Expression<Func<T2, TP2>> t2Selector,
            Func<TP1, TP2> ltrConverter,
            Func<TP2, TP1> rtlConverter)
        {
            Mapping.Add(new Map<TP1, TP2>()
            {
                SpecializedLTR = ltrConverter,
                SpecializedRTL = rtlConverter,
                T1Property = t1Selector,
                T2Property = t2Selector
            });
            return this;
        }
    }
}