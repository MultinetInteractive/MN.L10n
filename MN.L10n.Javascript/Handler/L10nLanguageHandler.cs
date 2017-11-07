using System;
using System.Web;

namespace MN.L10n.Handler
{
    public class  L10nLanguageHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: https://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; } 
        }

        public void ProcessRequest(HttpContext context)
        {
			var cacheDuration = new TimeSpan(1, 0, 0);
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Cache.SetExpires(DateTime.Now.Add(cacheDuration));
			context.Response.Cache.SetMaxAge(cacheDuration);
			context.Response.Cache.SetValidUntilExpires(true);
            context.Response.Write(Javascript.RuleEvaluatorFactory.CreateJavascriptRuleEvaluator(context.Cache));
        }

        #endregion
    }
}
