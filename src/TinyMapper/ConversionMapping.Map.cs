using System;
using System.Linq.Expressions;

namespace TinyMapper
{
    public partial class ConversionMapping<T1, T2>
    {
        public class Map
        {
            /// <summary>
            /// Gets or sets a transformation function for T1.Property to T2.Property
            /// </summary>
            /// <returns></returns>
            public virtual Func<object, object> LTR { get; set; } = x => x;

            /// <summary>
            /// Gets or sets a transformation function for T2.Property to T1.Property
            /// </summary>
            /// <returns></returns>
            public virtual Func<object, object> RTL { get; set; } = x => x;

            /// <summary>
            /// Gets or sets the selector for a property in the T1 type
            /// </summary>
            /// <returns></returns>
            public LambdaExpression T1Property { get; set; }

            /// <summary>
            /// Gets or sets the selctor for a property in T2 type
            /// </summary>
            /// <returns></returns>
            public LambdaExpression T2Property { get; set; }
            
            /// <summary>
            /// Gets a value indicating whether or not this type supports specializations
            /// </summary>
            public virtual bool HasSpecialization => false;
        }

        public class Map<TP1, TP2> : Map
        {
            /// <summary>
            /// Gets or sets the specialized form of LTR.
            /// </summary>
            /// <returns></returns>
            public Func<TP1, TP2> SpecializedLTR
            {
                get; set;
            }

            /// <summary>
            /// Gets or sets the specialized form of RTL
            /// </summary>
            /// <returns></returns>
            public Func<TP2, TP1> SpecializedRTL
            {
                get; set;
            }

            /// <summary>
            /// Gets or sets the normalized form of LTR, translating to
            /// the specialized form for type validation
            /// </summary>
            /// <returns></returns>
            public override Func<object, object> LTR
            {
                get => x => SpecializedLTR((TP1)x);
                set => SpecializedLTR = x => (TP2)value(x);
            }
            /// <summary>
            /// Gets or sets the normalized form of RTL, translating to
            /// the specialized form for type validation
            /// </summary>
            /// <returns></returns>
            public override Func<object, object> RTL
            {
                get => x => SpecializedRTL((TP2)x);
                set => SpecializedRTL = x => (TP1)value(x);
            }

            /// <summary>
            /// Gets a value indicating whether or not this type 
            /// supports specialized conversions
            /// </summary>
            public override bool HasSpecialization => true;
        }
    }
}