using System;
using System.Collections.Generic;
using System.Text;

namespace MN.L10n.Javascript.Core
{
    public class L10nJavascriptMiddlewareOptions
    {
        public string Url { get; set; }

        internal L10nJavascriptMiddlewareOptions()
        {
            Url = "/L10nLanguage.js";
        }
    }
}
