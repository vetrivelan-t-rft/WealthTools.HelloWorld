using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WealthTools.Common.Models.Attribute;
using WealthTools.Common.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace WealthTools.WebAPI.Proposals
{

    public class ProtectedStringPropertyResolverX : CamelCasePropertyNamesContractResolver
    {
        IEncrtpyHelper encrtpyHelper;

        public ProtectedStringPropertyResolverX(IServiceProvider serviceProvider)
        {
            encrtpyHelper = (IEncrtpyHelper)serviceProvider.GetService(typeof(IEncrtpyHelper));
        }

        /// <summary>
        /// CreateProperties is override method of DefaultContractResolver
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

            // Find all string properties that have a [JsonProtectAttribute] attribute applied           
            foreach (JsonProperty prop in props.Where(p => p.PropertyType == typeof(string)))
            {
                PropertyInfo pi = type.GetProperty(prop.UnderlyingName);
                if (pi != null && pi.GetCustomAttribute(typeof(ProtectAttribute), true) != null)
                {
                    prop.ValueProvider =
                        new ProtectStringValueProviderX(pi, encrtpyHelper);
                }
            }

            return props;
        }


    }

    public class ProtectStringValueProviderX : Newtonsoft.Json.Serialization.IValueProvider
    {

        PropertyInfo targetProperty;
        IEncrtpyHelper encryptHelper;

        public ProtectStringValueProviderX(System.Reflection.PropertyInfo pi, IEncrtpyHelper encryptHelper)
        {
            this.targetProperty = pi;
            this.encryptHelper = encryptHelper;
        }

        public object GetValue(object target)
        {
            if (null != target)
            {
                var rtnValue = targetProperty.GetValue(target);
                if (null != rtnValue && encryptHelper != null)
                {
                    return encryptHelper.EncryptString(rtnValue.ToString());
                }
            }
            return target;
        }

        public void SetValue(object target, object value)
        {
            //Not sure what is the usecase when this will be needed. 
            //TODO:                     return encryptHelper.DecryptString(rtnValue.ToString());
        }
    }


    [AttributeUsage(AttributeTargets.Method,
                   AllowMultiple = false,
                   Inherited = true)]
    public class XUnProtectParamsAttribute : ActionFilterAttribute
    {
        public string[] paramstoDecrpt { get; set; }

        public bool IsReusable => throw new NotImplementedException();

        private IEncrtpyHelper encrtpyHelper;

        public XUnProtectParamsAttribute(string[] ParamstoDecrpt)
        {
            this.paramstoDecrpt = ParamstoDecrpt;
        }



        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            this.encrtpyHelper = context.HttpContext.RequestServices.GetService<IEncrtpyHelper>();
            //// execute any code before the action executes
            UnprotectProperties(context);

            var result = await next();
            // execute any code after the action executes

        }

        private void UnprotectProperties(ActionExecutingContext context)
        {

            string[] paramstoReplace = context.ActionArguments.Keys.Intersect(paramstoDecrpt).ToArray();



            foreach (var item in paramstoReplace)
            {
                context.ActionArguments[item] = UnProtect(context.ActionArguments[item]);
            }

        }

        private object UnProtect(object objToUnprotect)
        {
            if (objToUnprotect != null)
            {
                string uprotectvalue = objToUnprotect as string;
                if (!string.IsNullOrEmpty(uprotectvalue))
                {                 
                    string decodedValue = System.Net.WebUtility.UrlDecode(uprotectvalue).Replace(" ", "+");
                    return encrtpyHelper.DeCryptString(decodedValue);
                }
            }
            return objToUnprotect;
        }
    }



}
