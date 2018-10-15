using Sitecore.Data;
using System;

namespace Sitecore.Scientist.Feature.Redirects
{
	public class Templates
	{
		public Templates()
		{
		}

		public struct Redirect
		{
			public static ID ID;

			static Redirect()
			{
				Templates.Redirect.ID = ID.Parse("{232D252D-7E17-489A-B692-9BFEABB2689F}");
			}

            public struct Fields
            {
                public readonly static ID RedirectUrl;

                public readonly static ID TargetItem;

                static Fields()
                {
                    Templates.Redirect.Fields.RedirectUrl = new ID("{55C4EBDD-A0C6-42E3-9197-4180D3C664ED}");
                    Templates.Redirect.Fields.TargetItem = new ID("{627A5131-5F85-4037-A513-FAEE4C0FE169}");
                }
            }
        }

		public struct RedirectMap
		{
			public static ID ID;

			static RedirectMap()
			{
				Templates.RedirectMap.ID = ID.Parse("{4F554D94-F449-429C-9DA0-187F316BC95E}");
			}

			public struct Fields
			{
				public readonly static ID RedirectType;

				public readonly static ID PreserveQueryString;

				public readonly static ID UrlMapping;

				static Fields()
				{
					Templates.RedirectMap.Fields.RedirectType = new ID("{57A41BCA-DF6E-45CD-80B1-A840DF5CE724}");
					Templates.RedirectMap.Fields.PreserveQueryString = new ID("{A15D77B0-F075-4B6E-8D9F-D406C69F1A8D}");
					Templates.RedirectMap.Fields.UrlMapping = new ID("{3A32FF07-C588-4696-B512-3A553D1AD6A8}");
				}
			}
		}

		public struct RedirectMapGrouping
		{
			public static ID ID;

			static RedirectMapGrouping()
			{
				Templates.RedirectMapGrouping.ID = ID.Parse("{57C61BB1-9B1E-4FCC-8CA0-CC580AF337F1}");
			}
		}
	}
}