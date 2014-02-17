using System;
using System.Collections;
using System.Collections.Generic;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Globalization;
using Sitecore.SharedSource.PartialLanguageFallback.Extensions;
using Sitecore.Pipelines.GetTranslation;

namespace Verndale.SharedSource.SitecoreProcessors
{
    public class TryGetFromFallbackLanguage : TryGetFromFallbackDomains
    {
        public static bool EnableFallback
        {
            get
            {
                return Sitecore.Context.Site != null &&
                       Sitecore.Context.Site.SiteInfo.Properties["enableFallback"] != null &&
                       Sitecore.Context.Site.SiteInfo.Properties["enableFallback"].Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Runs the processor.
        /// 
        /// </summary>
        /// <param name="args">The arguments.
        ///             </param>
        public void Process(GetTranslationArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            List<string> processedDomains = new List<string>();
            if (args.HasResult || Context.Site == null || string.IsNullOrEmpty(Context.Site.DictionaryDomain))
                return;
            this.Args = args;
            this.Database = args.Options.Database ?? args.ContentDatabase;
            DictionaryDomain domain;
            if (!DictionaryDomain.TryParse(Context.Site.DictionaryDomain, this.Database, out domain) || domain == null)
                return;

            string result;
            if (this.TryGetTranslation(domain, processedDomains, out result) && result != null)
            {
                args.Result = result;
            }
            else if (EnableFallback)
            {
                if (this.TryTranslateTextByFallbackLanguage(args, domain, out result) && result != null)
                {
                    args.Result = result;
                }
            }

        }

        protected virtual bool TryTranslateTextByFallbackLanguage(GetTranslationArgs args, DictionaryDomain domain, out string result)
        {
            result = null;
            List<string> processedDomains = new List<string>();

            // check if the the language passed in with the args has fallback assigned
            // if so, then get that fallback language
            // must try to get the translation based on that language
            var languageFallsBack = args.Language.HasFallbackAssigned(args.ContentDatabase);
            if (languageFallsBack)
            {
                Language fallbackLanguage = args.Language.GetFallbackLanguage(args.ContentDatabase);

                // the following cannot be called from here, because it is an internal method to the Sitecore.Kernel library
                //Translate.TryTranslateTextByLanguage(args.Key, domain, fallbackLanguage, out result, args.Parameters);

                // therefore, we set Args.Language to the fallbacklanguage
                // this.Args is the Args object in TryGetFromFallbackDomains processor
                // then we call this.TryGetTranslation, which is a method in the TryGetFromFallbackDomains processor, 
                // which IS in the Sitecore.Kernel library and therefore can make the call to TryTranslateTextByLanguage

                this.Args.Language = fallbackLanguage;

                if (this.TryGetTranslation(domain, processedDomains, out result) && result != null)
                {
                    return true;
                }
                else
                {
                    // if no results if found, try to see if this fallback language falls back itself to another language
                    // and then if so, try the translation with that
                    // pass into the recursive call this.Args (instead of args), since the language has been updated in this.Args
                    if (result == null)
                    {
                        var isSuccess = TryTranslateTextByFallbackLanguage(this.Args, domain, out result);
                        return isSuccess;
                    }
                    else
                        return false;
                }
            }
            else
            {
                return false;
            }

        }


    }
}
