using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace TinyMapper
{
    public abstract class Converter
    {
        public abstract object Convert(object value);
    }

    public class Converter<T1, T2> : Converter
    {
        private ConversionMapping<T1, T2> _mapping;
        public ConversionMapping<T1, T2> Mapping => _mapping;
        public Converter(ConversionMapping<T1, T2> mapping)
        {
            _mapping = mapping;
        }

        /// <summary>
        /// Converts an instance of T2 to an instance of T1
        /// </summary>
        /// <param name="t2"></param>
        /// <returns></returns>
        public T1 Convert(T2 t2)
        {
            var result = (T1)Activator.CreateInstance(typeof(T1));
            foreach (var item in _mapping.Mapping)
            {
                try
                {
                    var value = item.T2Property.Compile().DynamicInvoke(t2);
                    ((item.T1Property.Body as MemberExpression)?.Member as PropertyInfo)?.SetValue(result, item.RTL(value));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts an instance of T1 to an instance of T2
        /// </summary>
        /// <param name="t1"></param>
        /// <returns></returns>
        public T2 Convert(T1 t1)
        {
            var result = (T2)Activator.CreateInstance(typeof(T2));
            foreach (var item in _mapping.Mapping)
            {
                try
                {
                    var value = item.T1Property.Compile().DynamicInvoke(t1);
                    if (value != null)
                    {
                        var converted = item.LTR(value);
                        // Rearrange things into an assignment expression so things like:
                        // foo.MapProperty(x=>x.Foo, x=>x.Foo.Fee) will get properly assigned
                        // in an LTR conversion
                        var exp = Expression.Assign(item.T2Property.Body, Expression.Constant(converted));
                        var lambda = Expression.Lambda(exp, item.T2Property.Parameters);
                        var compiled = lambda.Compile();
                        compiled.DynamicInvoke(result);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.FormattedMessage());
                }
            }
            return result;
        }

        /// <summary>
        /// Converts the object to the corresponding type, if a mapping exists
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object Convert(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (typeof(T1).IsAssignableFrom(value.GetType()))
            {
                return Convert((T1)value);
            }

            else if (typeof(T2).IsAssignableFrom(value.GetType()))
            {
                return Convert((T2)value);
            }
            throw new InvalidOperationException("I don't understand this conversion");
        }
    }
}