using CMS.ContentEngine;
using CMS.Core;
using Microsoft.Extensions.Logging;
using Migration.Tool.KXP.Api.Services.CmsClass;
using Newtonsoft.Json.Linq;

// migrate a property using a custom selector form component, e.g. Cloudinary selector to  Kentico combined content selector
// This migration code is not complete, it just serves as a template, using the cloudinary selector as an example
// It needs to be adapted to the actual custom selector and its data structure
public class WidgetCustomSelectorMigration(
    ILogger<WidgetCustomSelectorMigration> logger) : IWidgetPropertyMigration
{
    private const string MigratedComponent = "CloudinarySelectorComponent";

    public int Rank => 100_002;

    public bool ShallMigrate(WidgetPropertyMigrationContext context, string propertyName)
        => MigratedComponent.Equals(context.EditingFormControlModel?.FormComponentIdentifier, StringComparison.InvariantCultureIgnoreCase);

    // version A - migrate the property to combined content selector with content item references
    // *******************************************************************************************
    public Task<WidgetPropertyMigrationResult> MigrateWidgetProperty(
        string key, JToken? value, WidgetPropertyMigrationContext context)
    {
        (int siteId, _) = context;

        var refsToMedia = new List<object>();
        if (value != null && !string.IsNullOrEmpty(value.ToString()))
        {
            // port the assets into the target solution before this migration runs
            // map the value stored in widget property to th corresponding Asset GUID
            var assetGuid = ...; // extract the asset GUID from the value, depending on how it is stored in the custom selector
            // add the GUID as ContentItemReference to the result list
            refsToMedia.Add(new ContentItemReference { Identifier = assetGuid });
        }
        var resultAsJToken = JToken.FromObject(refsToMedia);
        return Task.FromResult(new WidgetPropertyMigrationResult(resultAsJToken));
    }

    // version B - migrate a property to text and use a custom component (e.g. ported Cloudinary selector) to display it in the target instance
    // *******************************************************************************************
    // public Task<WidgetPropertyMigrationResult> MigrateWidgetProperty(
    //     string key, JToken? value, WidgetPropertyMigrationContext context)
    // {
    //     (int siteId, _) = context;

    //     const string CUSTOM_PREFIX = "cloudinary_"; // prefix to identify the value as a custom selector value in the target instance

    //     if (value != null)
    //     {
    //         var newValue = $"{CUSTOM_PREFIX}{value}"; // adapt the value to the format expected by the custom selector in the target instance
    //         var resultAsJToken = JToken.FromObject(newValue);
    //         return Task.FromResult(new WidgetPropertyMigrationResult(resultAsJToken));
    //     }
    //     else
    //     {
    //         logger.LogError("Failed to parse '{ComponentName}' json {Json}", MigratedComponent, value?.ToString() ?? "<null>");

    //         // leave value as it is
    //         return Task.FromResult(new WidgetPropertyMigrationResult(value));
    //     }
    // }

    private async Task<ContentItemReference> CreateAssetContentItemFromCloudinary(JToken value)
    {
        const string KENTICO_DEFAULT_WORKSPACE_NAME = "KenticoDefault"; // value retrieved from the database
        const int GLOBAL_ADMINISTRATOR_USER_ID = 53; // 53 is an ID of the Global Administrator user
        const string ENGLISH_US_LANGUAGE = "en-US";

        // Use the value to retrieve the asset and it's metadata from Cloudinary

        // Prepare the Legacy media file properties from the data retrieved from Cloudinary
        var legacyMediaFileAsset = new ContentItemAsset();
        var legacyMediaFileTitle = value["buttonTarget"];
        var legacyMediaFileDescription = value["buttonText"];

        var ciManager = Service.Resolve<IContentItemManagerFactory>().Create(GLOBAL_ADMINISTRATOR_USER_ID);

        var createContentItemParameters = new CreateContentItemParameters(
            contentTypeName: "Legacy.MediaFile",
            name: $"MigratedAsset{Guid.NewGuid():N}",
            displayName: $"Asset - {legacyMediaFileTitle?.ToString() ?? "<null>"}",
            languageName: ENGLISH_US_LANGUAGE,
            workspaceName: KENTICO_DEFAULT_WORKSPACE_NAME
        );

        // the property names have to match the manually created content type's field names in the administration 
        var contentItemData = new ContentItemData();
        contentItemData.SetValue("HeroHeading", heroHeading?.ToString() ?? string.Empty);
        contentItemData.SetValue("HeroTarget", heroTarget?.ToString() ?? string.Empty);
        contentItemData.SetValue("HeroCallToAction", heroCallToAction?.ToString() ?? string.Empty);

        int itemId = await ciManager.Create(createContentItemParameters, contentItemData);

        if (itemId <= 0)
        {
            throw new Exception("Unable to create content item");
        }
        if (!await ciManager.TryPublish(itemId, ENGLISH_US_LANGUAGE))
        {
            throw new Exception("Could not publish Hero item");
        }
        return new ContentItemReference { Identifier = CMS.ContentEngine.Internal.ContentItemInfo.Provider.Get(itemId).ContentItemGUID };
    }
}
