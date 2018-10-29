using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Scientist.Feature.Redirects.Extensions;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Text;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Sitecore.Scientist.Feature.Redirects.Pipelines.HttpRequest
{
    public class RedirectMapResolver : HttpRequestProcessor
    {
        private string AllMappingsPrefix
        {
            get
            {
                return string.Format("{0}AllMappings-{1}-{2}-{3}", "Scientist-Redirect-", Context.Database.Name, Context.Site.Name,Context.Language.Name);
            }
        }


        public int CacheExpiration
        {
            get;
            set;
        }

        protected virtual List<RedirectMapping> MappingsMap
        {
            get
            {
                RedirectType redirectType;
                Item item = null;
                List<RedirectMapping> redirectMappings = HttpRuntime.Cache[this.AllMappingsPrefix] as List<RedirectMapping>;
                if (redirectMappings == null)
                {
                    redirectMappings = new List<RedirectMapping>();
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
                            where i.InheritsFrom(Sitecore.Scientist.Feature.Redirects.Templates.RedirectMap.ID)
                            select i).ToArray<Item>();
                        Array.Sort<Item>(array, new TreeComparer());
                        Item[] itemArray = array;
                        for (int num = 0; num < (int)itemArray.Length; num++)
                        {
                            Item item2 = itemArray[num];
                            if (Enum.TryParse<RedirectType>(item2[Templates.RedirectMap.Fields.RedirectType], out redirectType))
                            {
                                bool flag = MainUtil.GetBool(item2[Templates.RedirectMap.Fields.PreserveQueryString], false);
                                UrlString urlString = new UrlString()
                                {
                                    Query = item2[Templates.RedirectMap.Fields.UrlMapping]
                                };
                                foreach (string key in urlString.Parameters.Keys)
                                {
                                    if (string.IsNullOrEmpty(key))
                                    {
                                        continue;
                                    }
                                    string str = urlString.Parameters[key];
                                    if (string.IsNullOrEmpty(str))
                                    {
                                        continue;
                                    }
                                    string lower = HttpUtility.UrlDecode(key.ToLower(), System.Text.Encoding.UTF8);
                                    bool flag1 = (!lower.StartsWith("^") ? false : lower.EndsWith("$"));
                                    if (!flag1)
                                    {
                                        lower = this.EnsureSlashes(lower);
                                    }
                                    str = HttpUtility.UrlDecode(str.ToLower(), System.Text.Encoding.UTF8);
                                    str = HttpUtility.UrlDecode(str.ToLower(), System.Text.Encoding.UTF8);
                                    str = str.ToLower() ?? string.Empty;
                                    str = str.TrimStart(new char[] { '\u005E' }).TrimEnd(new char[] { '$' });
                                    redirectMappings.Add(new RedirectMapping()
                                    {
                                        RedirectType = redirectType,
                                        PreserveQueryString = flag,
                                        Pattern = lower,
                                        Target = str,
                                        IsRegex = flag1
                                    });
                                }
                            }
                            else
                            {
                                Log.Info(string.Format("Redirect map {0} does not specify redirect type.", item2.Paths.FullPath), this);
                            }
                        }
                    }
                    if (this.CacheExpiration > 0)
                    {
                        Cache cache = HttpRuntime.Cache;
                        string allMappingsPrefix = this.AllMappingsPrefix;
                        DateTime utcNow = DateTime.UtcNow;
                        cache.Add(allMappingsPrefix, redirectMappings, null, utcNow.AddMinutes((double)this.CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
                    }
                }
                return redirectMappings;
            }
        }

        private string ResolvedMappingsPrefix
        {
            get
            {
                return string.Format("{0}ResolvedMappings-{1}-{2}-{3}", "Scientist-Redirect-", Context.Database.Name, Context.Site.Name,Context.Language.Name);
            }
        }

        public RedirectMapResolver()
        {
        }

        private string EnsureSlashes(string text)
        {
            return StringUtil.EnsurePostfix('/', StringUtil.EnsurePrefix('/', text));
        }

        protected virtual RedirectMapping FindMapping(string filePath)
        {
            RedirectMapping redirectMapping = null;
            List<RedirectMapping>.Enumerator enumerator = this.MappingsMap.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    RedirectMapping current = enumerator.Current;
                    if ((current.IsRegex || !(current.Pattern == filePath)) && (!current.IsRegex || !current.Regex.IsMatch(filePath)))
                    {
                        continue;
                    }
                    redirectMapping = current;
                    return redirectMapping;
                }
                return null;
            }
            catch (Exception)
            {

            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            return redirectMapping;
        }

        protected virtual RedirectMapping GetResolvedMapping(string filePath)
        {
            Dictionary<string, RedirectMapping> item = HttpRuntime.Cache[this.ResolvedMappingsPrefix] as Dictionary<string, RedirectMapping>;
            if (item == null || !item.ContainsKey(filePath))
            {
                return null;
            }
            return item[filePath];
        }
        protected virtual string GetTargetUrl(RedirectMapping mapping, string input)
        {
            string target = mapping.Target;
            if (mapping.IsRegex)
            {
                target = mapping.Regex.Replace(input.TrimEnd(new char[] { '/' }), target);
            }
            if (mapping.PreserveQueryString)
            {
                target = string.Concat(target.TrimEnd(new char[] { '/' }), HttpContext.Current.Request.Url.Query);
            }
            if (!string.IsNullOrEmpty(Context.Site.VirtualFolder))
            {
                target = string.Concat(StringUtil.EnsurePostfix('/', Context.Site.VirtualFolder), target.TrimStart(new char[] { '/' }));
            }
            return target;
        }

        protected virtual bool IsFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || WebUtil.IsExternalUrl(filePath))
            {
                return true;
            }
            return File.Exists(HttpContext.Current.Server.MapPath(filePath));
        }


        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item != null || Context.Database == null || Context.Site == null || this.IsFile(Context.Request.FilePath))
            {
                return;
            }
            if (Context.PageMode.IsExperienceEditor || Context.PageMode.IsPreview)
            {
                return;
            }
            string str = this.EnsureSlashes(Context.Request.FilePath.ToLower());
            RedirectMapping resolvedMapping = this.GetResolvedMapping(str);
            bool flag = resolvedMapping != null;
            if (resolvedMapping == null)
            {
                resolvedMapping = this.FindMapping(str);
            }
            if (resolvedMapping != null && !flag)
            {
                Dictionary<string, RedirectMapping> item = HttpRuntime.Cache[this.ResolvedMappingsPrefix] as Dictionary<string, RedirectMapping> ?? new Dictionary<string, RedirectMapping>();
                item[str] = resolvedMapping;
                Cache cache = HttpRuntime.Cache;
                string resolvedMappingsPrefix = this.ResolvedMappingsPrefix;
                DateTime utcNow = DateTime.UtcNow;
                cache.Add(resolvedMappingsPrefix, item, null, utcNow.AddMinutes((double)this.CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
            }
            if (resolvedMapping != null && HttpContext.Current != null)
            {
                string targetUrl = this.GetTargetUrl(resolvedMapping, str);
                if (resolvedMapping.RedirectType == RedirectType.Redirect301)
                {
                    this.Redirect301(HttpContext.Current.Response, targetUrl);
                }
                if (resolvedMapping.RedirectType == RedirectType.Redirect302)
                {
                    HttpContext.Current.Response.Redirect(targetUrl, true);
                }
                if (resolvedMapping.RedirectType == RedirectType.ServerTransfer)
                {
                    HttpContext.Current.Server.TransferRequest(targetUrl);
                }
            }
        }

        protected virtual void Redirect301(HttpResponse response, string url)
        {
            HttpCookieCollection httpCookieCollection = new HttpCookieCollection();
            for (int i = 0; i < response.Cookies.Count; i++)
            {
                HttpCookie item = response.Cookies[i];
                if (item != null)
                {
                    httpCookieCollection.Add(item);
                }
            }
            response.Clear();
            for (int j = 0; j < httpCookieCollection.Count; j++)
            {
                HttpCookie httpCookie = httpCookieCollection[j];
                if (httpCookie != null)
                {
                    response.Cookies.Add(httpCookie);
                }
            }
            response.Status = "301 Moved Permanently";
            response.AddHeader("Location", url);
            response.End();
        }
    }
}