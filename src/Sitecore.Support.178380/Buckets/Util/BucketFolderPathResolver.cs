namespace Sitecore.Support.Buckets.Util
{
  using Sitecore;
  using Sitecore.Buckets.Rules.Bucketing;
  using Sitecore.Buckets.Util;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Rules;
  using Sitecore.StringExtensions;
  using System;

  public class BucketFolderPathResolver : IDynamicBucketFolderPath
  {
    public BucketFolderPathResolver()
    {
    }

    public string GetFolderPath(Database database, string itemName, ID templateId, ID itemId, ID parentItemId, DateTime creationDateOfNewItem)
    {
      Assert.ArgumentNotNull(database, "database");
      BucketingRuleContext ruleContext = new BucketingRuleContext(database, parentItemId, itemId, itemName, templateId, creationDateOfNewItem)
      {
        NewItemId = itemId,
        CreationDate = creationDateOfNewItem
      };
      Item item = database.GetItem(Sitecore.Buckets.Util.Constants.SettingsItemId);
      Assert.IsNotNull(item, "Setting Item");
      RuleList<BucketingRuleContext> rules = RuleFactory.GetRules<BucketingRuleContext>(new Item[] { item }, Sitecore.Buckets.Util.Constants.BucketRulesFieldId);
      try
      {
        if (rules != null)
        {
          rules.Run(ruleContext);
        }
      }
      catch (Exception exception)
      {
        Log.Error($"BucketFolderPathResolver: Cannot resolve bucket path for item {itemId}. Parent = {parentItemId}", exception);
      }
      string resolvedPath = ruleContext.ResolvedPath.Replace(".", "/").Replace("-","/");
      if (resolvedPath.IsNullOrEmpty())
      {
        resolvedPath = creationDateOfNewItem.ToString(BucketConfigurationSettings.BucketFolderPath, Context.Culture);
      }
      return resolvedPath;
    }
  }
}