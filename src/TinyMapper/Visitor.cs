using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TinyMapper
{
    /// <summary>
    /// Internal expression visitor used to translate a member expression body 
    /// of one type to the equivalen member expression body of a different, mapped type
    /// </summary>
    public class Visitor<T1, T2> : ExpressionVisitor
    {
        private TinyMapper _mapper;
        private ParameterExpression _changed;
        private ConversionMapping<T1, T2> _mapping;

        public Visitor(TinyMapper mapper) : this(mapper, Expression.Parameter(typeof(T2)))
        {

        }
        public Visitor(TinyMapper mapper, ParameterExpression changed)
        {
            _mapper = mapper;
            _changed = changed;
            _mapping = _mapper.Find<T1, T2>();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var name = node.Member.Name;
            var mp1 = _mapping.Mapping.FirstOrDefault(x => x.T1Property.ToPropertyInfo()?.Name == name);
            var other = mp1.T2Property.ToPropertyInfo() ?? typeof(T2).GetProperty(name);

            if (node.Member.MemberType != MemberTypes.Property || other == null || other.PropertyType != (((PropertyInfo)node.Member).PropertyType))
            {
                Debug.WriteLine($"Using default behavior for {node.Member.Name} due to {(other == null ? "other is null" : node.Member.MemberType != MemberTypes.Property ? "node member isn't a property" : "no matching inner property")}");
                return base.VisitMember(node);
            }

            return Expression.Property(Visit(node.Expression), other);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _changed;
        }
    }
}