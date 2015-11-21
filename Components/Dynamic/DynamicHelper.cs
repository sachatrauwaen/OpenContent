using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;


namespace Satrabel.OpenContent.Components.Dynamic
{
    internal class DynamicUtils
    {
        public static bool TryGetMemberValue(object obj, string memberName, out object result)
        {
            try
            {
                result = GetMemberValue(obj, memberName);
                return true;
            }
            catch (RuntimeBinderException)
            {
            }
            catch (RuntimeBinderInternalCompilerException)
            {
            }

            result = null;
            return false;
        }

        //[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to swallow exceptions that happen during runtime binding")]
        public static bool TryGetMemberValue(object obj, GetMemberBinder binder, out object result)
        {
            try
            {
                if (typeof(Binder).Assembly.Equals(binder.GetType().Assembly))
                {
                    result = GetMemberValue(obj, binder);
                }
                else
                {
                    result = GetMemberValue(obj, binder.Name);
                }
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static object GetMemberValue(object obj, string memberName)
        {
            var callSite = GetMemberAccessCallSite(memberName);
            return callSite.Target(callSite, obj);
        }

        public static object GetMemberValue(object obj, GetMemberBinder binder)
        {
            var callSite = GetMemberAccessCallSite(binder);
            return callSite.Target(callSite, obj);
        }

        public static CallSite<Func<CallSite, object, object>> GetMemberAccessCallSite(string memberName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, typeof(DynamicUtils), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            return GetMemberAccessCallSite(binder);
        }

        public static CallSite<Func<CallSite, object, object>> GetMemberAccessCallSite(CallSiteBinder binder)
        {
            return CallSite<Func<CallSite, object, object>>.Create(binder);
        }

        public static IEnumerable<string> GetMemberNames(object obj)
        {
            var provider = obj as IDynamicMetaObjectProvider;
            Debug.Assert(provider != null, "obj doesn't implement IDynamicMetaObjectProvider");

            Expression parameter = Expression.Parameter(typeof(object));
            return provider.GetMetaObject(parameter).GetDynamicMemberNames();
        }
    }
}