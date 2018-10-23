using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Scientist.Feature.Redirects.Extensions;
using Sitecore.Links;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;
using Sitecore.StringExtensions;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Sitecore.Scientist.Feature.Redirects.Pipelines.HttpRequestBegin
{
    public class RedirectItem : HttpRequestProcessor
    {
        public RedirectItem()
        {
        }
        public int CacheExpiration
        {
            get;
            set;
        }


        private string AllMappingsPrefix
        {
            get
            {
                return string.Format("{0}AllRedirectMappings-{1}-{2}", "Scientist-Redirect-", Context.Database.Name, Context.Site.Name);
            }
        }
        protected virtual string GetRedirectUrl(Item redirectItem)
        {
            LinkField item = redirectItem.Fields[Templates.Redirect.Fields.RedirectUrl];
            string str = null;
            if (item != null)
            {
                if (!item.IsInternal || item.TargetItem == null)
                {
                    str = (!item.IsMediaLink || item.TargetItem == null ? item.Url : ((MediaItem)item.TargetItem).GetMediaUrl(null));
                }
                else
                {
                    SiteInfo siteInfo = Context.Site.SiteInfo;
                    UrlOptions defaultOptions = UrlOptions.DefaultOptions;
                    defaultOptions.Site = SiteContextFactory.GetSiteContext(siteInfo.Name);
                    defaultOptions.AlwaysIncludeServerUrl = true;
                    str = string.Concat(LinkManager.GetItemUrl(item.TargetItem, defaultOptions), (string.IsNullOrEmpty(item.QueryString) ? "" : string.Concat("?", item.QueryString)));
                }
            }
            return str;
        }
        protected virtual string GetTargetItemId(Item targetItem)
        {
            LinkField item = targetItem.Fields[Templates.Redirect.Fields.TargetItem];
            string str = string.Empty;
            if (item != null)
            {
                if (item.IsInternal && !string.IsNullOrEmpty(item.Value))
                {
                    str = GetItemIdByPath(item.Value);
                }
            }
            return str;
        }
        public static string GetItemIdByPath(string path)
        {
            var Item = Sitecore.Context.Database.SelectSingleItem(path);
            if (Item != null)
            {
                return Item.ID.ToString();
            }

            return string.Empty;
        }

        private string ResolvedRedirectPrefix
        {
            get
            {
                return string.Format("{0}ResolvedRedirect-{1}-{2}", "Scientist-Redirect-", Context.Database.Name, Context.Site.Name);
            }
        }
        protected virtual Redirect GetResolvedMapping(string ItemId)
        {
            Dictionary<string, Redirect> item = HttpRuntime.Cache[this.ResolvedRedirectPrefix] as Dictionary<string, Redirect>;
            if (item == null || !item.ContainsKey(ItemId))
            {
                return null;
            }
            return item[ItemId];
        }

        protected virtual List<Redirect> MappingsMap
        {
            get
            {
                Item item = null;
                List<Redirect> redirectItems = HttpRuntime.Cache[this.AllMappingsPrefix] as List<Redirect>;
                if (redirectItems == null)
                {
                    redirectItems = new List<Redirect>();
                    var redirectSettingsId = Context.Site.Properties["redirectSettingsId"];
                    if (!string.IsNullOrEmpty(redirectSettingsId))
                    {
                        item = Context.Database.GetItem(redirectSettingsId);
                    }
                    else
                    {
                        item = null;
                    }
                    Item item1 = item;
                    if (item1 != null)
                    {
                        Item[] array = (
                            from i in (IEnumerable<Item>)item1.Axes.GetDescendants()
                            where i.InheritsFrom(Templates.Redirect.ID)
                            select i).ToArray<Item>();
                        Array.Sort<Item>(array, new TreeComparer());
                        Item[] itemArray = array;
                        for (int num = 0; num < (int)itemArray.Length; num++)
                        {
                            Item item2 = itemArray[num];
                            var targetId = this.GetTargetItemId(item2);
                            var redirectUrl = this.GetRedirectUrl(item2);
                            if (!string.IsNullOrEmpty(targetId) && !string.IsNullOrEmpty(redirectUrl))
                            {
                                redirectItems.Add(new Redirect()
                                {
                                    RedirectUrl = redirectUrl,
                                    TargetItemId = targetId
                                });
                            }

                        }
                    }
                    if (this.CacheExpiration > 0)
                    {
                        Cache cache = HttpRuntime.Cache;
                        string allMappingsPrefix = this.AllMappingsPrefix;
                        DateTime utcNow = DateTime.UtcNow;
                        cache.Add(allMappingsPrefix, redirectItems, null, utcNow.AddMinutes((double)this.CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
                    }
                }
                return redirectItems;
            }
        }
        protected virtual Redirect FindMapping(string itemId)
        {
            Redirect redirectMapping = null;
            List<Redirect>.Enumerator enumerator = this.MappingsMap.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {

                    Redirect current = enumerator.Current;
                    if (itemId == current.TargetItemId)
                    {
                        redirectMapping = current;
                        return redirectMapping;
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            return redirectMapping;
        }

        public override void Process(HttpRequestArgs args)
        {
            Item item = Context.Item;
            if (Context.Database == null || item == null)
            {
                return;
            }
            if (Context.PageMode.IsExperienceEditor || Context.PageMode.IsPreview)
            {
                return;
            }
            string itemId = item.ID.ToString();
            Redirect resolvedMapping = this.GetResolvedMapping(itemId);
            bool flag = resolvedMapping != null;
            if (resolvedMapping == null)
            {
                resolvedMapping = this.FindMapping(itemId);
            }
            if (resolvedMapping != null && !flag)
            {
                Dictionary<string, Redirect> dictionaryitem = HttpRuntime.Cache[this.ResolvedRedirectPrefix] as Dictionary<string, Redirect> ?? new Dictionary<string, Redirect>();
                dictionaryitem[itemId] = resolvedMapping;
                Cache cache = HttpRuntime.Cache;
                string resolvedMappingsPrefix = this.ResolvedRedirectPrefix;
                DateTime utcNow = DateTime.UtcNow;
                cache.Add(resolvedMappingsPrefix, item, null, utcNow.AddMinutes((double)this.CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
            }
            if (resolvedMapping != null && HttpContext.Current != null)
            {
                if (!resolvedMapping.RedirectUrl.IsNullOrEmpty())
                {
                    HttpContext.Current.Response.Redirect(resolvedMapping.RedirectUrl, true);
                    args.AbortPipeline();
                }
            }
        }
    }
}