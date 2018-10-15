using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Scientist.Feature.Redirects.Repositories;
using System;

namespace Sitecore.Scientist.Feature.Redirects.EventHandlers
{
    public class RedirectMapCacheClearer
	{
		public RedirectMapCacheClearer()
		{
		}

		public void ClearCache(object sender, EventArgs args)
		{
			Assert.ArgumentNotNull(sender, "sender");
			Assert.ArgumentNotNull(args, "args");
			Log.Info("RedirectMapCacheClearer clearing redirect map cache.", this);
			RedirectsRepository.Reset();
			Log.Info("RedirectMapCacheClearer done.", this);
		}

		public void OnItemSaved(object sender, EventArgs args)
		{
			Assert.ArgumentNotNull(sender, "sender");
			Assert.ArgumentNotNull(args, "args");
			Item item = Event.ExtractParameter(args, 0) as Item;
			if (item == null || !item.TemplateID.Equals(Sitecore.Scientist.Feature.Redirects.Templates.RedirectMap.ID))
			{
				return;
			}
			this.ClearCache(sender, args);
		}

		public void OnItemSavedRemote(object sender, EventArgs args)
		{
			Assert.ArgumentNotNull(sender, "sender");
			Assert.ArgumentNotNull(args, "args");
			ItemSavedRemoteEventArgs itemSavedRemoteEventArg = args as ItemSavedRemoteEventArgs;
			if (itemSavedRemoteEventArg == null || itemSavedRemoteEventArg.Item == null || !itemSavedRemoteEventArg.Item.TemplateID.Equals(Sitecore.Scientist.Feature.Redirects.Templates.RedirectMap.ID))
			{
				return;
			}
			this.ClearCache(sender, args);
		}
	}
}