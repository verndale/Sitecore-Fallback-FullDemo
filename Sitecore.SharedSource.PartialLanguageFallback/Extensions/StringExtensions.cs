using System.Globalization;

namespace Sitecore.SharedSource.PartialLanguageFallback.Extensions
{
   public static class StringExtensions
   {
      public static string[] Split(this string source)
      {
         return StringUtil.Split(source, '|', true);
      }

      public static bool StartsWith(this string source, string part)
      {
         return (part.StartsWith(source, true, CultureInfo.InvariantCulture));
      }
   }
}
