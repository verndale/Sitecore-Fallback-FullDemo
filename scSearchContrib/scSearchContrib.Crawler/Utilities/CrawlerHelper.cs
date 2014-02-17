using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.PartialLanguageFallback.Extensions;

namespace scSearchContrib.Crawler.Utilities
{
    //<!--ADDED BY VERNDALE-->
    public class CrawlerHelper
    {
        //<!--ADDED FOR FALLBACK DEMO-->
        //Added this function to recursively loop through the fallback items and get the item whose field has value and return that item
        public static Item GetSitecoreFallbackItem(Item item, Field fld)
        {
            var checkValue = true;
            Item currentItem = item;
            try
            {

                if (item.Fields[fld.ID] != null)
                    checkValue = item.Fields[fld.ID].HasValue;
                if (checkValue)
                {
                    return item;
                }

                var fallbackItem = item.GetFallbackItem();
                if (fallbackItem != null)
                {
                    //Recursive call to get the item that has value for the particular field.
                    currentItem = GetSitecoreFallbackItem(fallbackItem, fld);
                }

            }
            catch (Exception ex)
            {

            }
            return currentItem;
        }
    }
}
