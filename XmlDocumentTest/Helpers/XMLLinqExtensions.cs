using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using System.Data.Linq.SqlClient;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace XmlDocumentTest.Helpers
{
    public static class XMLLinqExtensions
    {
        /*public static IQueryable<TSource> WhereXPath<TSource>(this IQueryable<TSource> enumerable, string xpath, params object[] parameters)
        {
        }

        [DbFunction("xml.exist", "exits")]
        public static bool XPathExits*/

        public static DbSqlQuery<TSource> WhereXPath<TSource>(this DbSet<TSource> set, Expression<Func<TSource, bool>> selector) where TSource : class
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(set.ToString());
            builder.Append(" WHERE ");
            builder.Append(EvaluateExpression(selector));

            string query = builder.ToString();

            return set.SqlQuery(query);
        }

        public static IQueryable<TSource> WhereXPath<TSource>(this IQueryable<TSource> enumerable, Expression<Func<TSource, bool>> selector)
        {
            string sqlQuery = enumerable.ToString();
            string result = EvaluateExpression(selector);
            //Expression.Call()

            return enumerable;
        }

        private static string EvaluateExpression(Expression expression)
        {
            Type type = expression.GetType();

            if (typeof(ConstantExpression).IsAssignableFrom(type))
            {
                return EvaluateConstant((ConstantExpression)expression);
            }else if (typeof(BinaryExpression).IsAssignableFrom(type))
            {
                return EvaluateBinaryExpression((BinaryExpression)expression);
            }else if (typeof(LambdaExpression).IsAssignableFrom(type))
            {
                return EvaluateExpression(((LambdaExpression)expression).Body);
            }else if (typeof(MemberExpression).IsAssignableFrom(type)){
                return EvaluateMemberExpression((MemberExpression)expression);
            }else if (typeof(ParameterExpression).IsAssignableFrom(type))
            {
                return String.Format("[Extent1]", ((ParameterExpression)expression).Name); //TODO: get true name
            }

            return null;
        }

        private static string EvaluateConstant(ConstantExpression expression)
        {
            string constant = expression.Value != null ? expression.ToString() : "NULL";

            return constant;
        }

        private static string EvaluateBinaryExpression(BinaryExpression expression)
        {
            string method;

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    method = "=";
                    break;

                default:
                    method = "NOT_SUPPORTED";
                    break;
            }

            return String.Format("{0} {1} {2}]') = 1", EvaluateExpression(expression.Left), method, EvaluateExpression(expression.Right));
        }

        private static string EvaluateMemberExpression(MemberExpression expression)
        {
            bool isInXPath = true;
            string result = EvaluateMember(expression.Member, ref isInXPath);

            if (expression.Expression != null)
            {
                if (result.StartsWith("["))
                {
                    return String.Format("{0}{1}", EvaluateExpression(expression.Expression), result);
                }else if (isInXPath)
                {
                    return String.Format("{0}/{1}", EvaluateExpression(expression.Expression), result);
                }
                else
                {
                    return String.Format("{0}.{1}", EvaluateExpression(expression.Expression), result);
                }
            }
            
            return result;
        }

        private static string EvaluateMember(MemberInfo member, ref bool isInXPath)
        {
            StringBuilder builder = new StringBuilder();

            XmlObjectAttribute attr = (XmlObjectAttribute)member.GetCustomAttribute(typeof(XmlObjectAttribute));
            if (attr != null)
            {
                builder.Append(attr.XmlColumn); //TODO: Get Real Column Name
                builder.Append(".exist('/");

                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo info = (member as PropertyInfo);
                    builder.Append(info.PropertyType.Name);
                }
                else
                {
                    builder.Append(member.DeclaringType.Name);
                }

                isInXPath = false;
            }
            else if (member.GetCustomAttribute(typeof(XmlAttributeAttribute)) != null)
            {
                builder.Append("[");
                builder.Append(member.Name);
            }else if (member.GetCustomAttribute(typeof(XmlTextAttribute)) != null)
            {
                builder.Append("[text()");
            }
            else
            {
                builder.Append(member.Name);
            }

            return builder.ToString();
        }
    }
}
