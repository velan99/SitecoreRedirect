using System;

namespace Sitecore.Scientist.Feature.Redirects
{
	internal struct Constants
	{
		public const string CachePrefix = "Scientist-Redirect-";

		public const string RedirectMapsQuery = "./*[@@templatename='RedirectMapGroupingTemplate']/*[@@templatename='RedirectMapTemplate']";
	}
}