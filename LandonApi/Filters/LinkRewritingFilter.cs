using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LandonApi.Infrastructure;
using LandonApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace LandonApi.Filters
{
    public class LinkRewritingFilter:IAsyncResultFilter
    {
        private readonly IUrlHelperFactory urlHelperFactory;

        public LinkRewritingFilter(IUrlHelperFactory urlHelperFactory)
        {
            this.urlHelperFactory = urlHelperFactory;
        }

        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var asObjectResult = context.Result as ObjectResult;
            var shouldSkip = asObjectResult?.StatusCode >= 400 || !(asObjectResult?.Value is Resource);

            if (shouldSkip) return next();

            var reWriter = new LinkRewriter(urlHelperFactory.GetUrlHelper(context));
            RewriteAllLinks(asObjectResult.Value, reWriter);

            return next();
        }

        private static void RewriteAllLinks(object model, LinkRewriter reWriter)
        {
            if(model ==null) return;

            var allProperties = model
                .GetType().GetTypeInfo()
                .GetAllProperties()
                .Where(p => p.CanRead)
                .ToArray();

            var linkProperties = allProperties
                .Where(p => p.CanWrite && p.PropertyType == typeof(Link)).ToArray();

            foreach (var linkProperty in linkProperties)
            {
                var rewritten = reWriter.Rewrite(linkProperty.GetValue(model) as Link);
                if (rewritten == null) continue;

                linkProperty.SetValue(model,rewritten);
                
            }

            var arrayProperties = allProperties.Where(p => p.PropertyType.IsArray).ToArray();

            RewriteLinksInArrays(arrayProperties, model, reWriter);

            var objectProperties = arrayProperties
                .Except(linkProperties)
                .Except(arrayProperties);

            RewriteLinksInNestedObjects(objectProperties, model, reWriter);


        }
        private static void RewriteLinksInNestedObjects(
            IEnumerable<PropertyInfo> objectProperties,
            object model,
            LinkRewriter rewriter)
        {
            foreach (var objectProperty in objectProperties)
            {
                if (objectProperty.PropertyType == typeof(string))
                {
                    continue;
                }

                var typeInfo = objectProperty.PropertyType.GetTypeInfo();
                if (typeInfo.IsClass)
                {
                    RewriteAllLinks(objectProperty.GetValue(model), rewriter);
                }
            }
        }

        private static void RewriteLinksInArrays(
            IEnumerable<PropertyInfo> arrayProperties,
            object model,
            LinkRewriter rewriter)
        {

            foreach (var arrayProperty in arrayProperties)
            {
                var array = arrayProperty.GetValue(model) as Array ?? new Array[0];

                foreach (var element in array)
                {
                    RewriteAllLinks(element, rewriter);
                }
            }
        }
    }
}
