Verndale Fallback Demo project

Compatibility
The codebase is compatible with Sitecore 6.6.x releases.
This project is built on Sitecore 6.6 update 7 (revision 131211).
The various elements of this solution have been verified to work with Sitecore 7.2 update 2
The functionality of the ADC element has been done for Sitecore 7 as well and can be found in another git repository here
The solution is Visual Studio 2010

How to build code and deploy the solution
1. You will need to add your Sitecore.Kernel dll to the following locations:
 - FallbackDemo\scSearchContrib\references
 - FallbackDemo\Verndale.SharedSource\references
 - FallbackDemo\Sitecore.SharedSource.PartialLanguageFallback\references\6.6

2. Add your sitecore license file to FallbackDemo\Data

3. Open the project -> Build Solution.

4. Restore the backups of the databases located FallbackDemo\Database\backups and update ConnectionString.config

5. Setup the site in IIS, .NET 4, Host Name FallbackDemo


6. Review the blog series about Partial Language Fallback on Sitecore, http://www.sitecore.net/en-gb/Learn/Blogs/Technical-Blogs/Elizabeth-Spranzani.aspx