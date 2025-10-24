using Migration.Tool.Source.Mappers.ContentItemMapperDirectives;
using Newtonsoft.Json.Linq;

namespace Migration.Tool.Extensions.CommunityMigrations;

public class AboutUsSectionsToWidgetsDirector : ContentItemDirectorBase
{
    public override void Direct(ContentItemSource source, IContentItemActionProvider options)
    {
        // Apply the conversions only to nodes under /About-Us (or your desired KX13 tree node)
        // The system will evaluate all following conditions for each tree node which node AliasPath starts with /About-Us
        // This includes the /About-Us node itself and all its child nodes (/About-Us/Our-philosophy, /About-Us/References, etc.)
        if (source.SourceNode!.NodeAliasPath.StartsWith("/About-Us"))
        {
            // Identify the KX13 tree node that will host the widget in XbyK
            if (source.SourceNode.NodeAliasPath == "/About-Us")
            {
                // Ensure this template exists in the XbyK target instance
                options.OverridePageTemplate("DancingGoat.LandingPageSingleColumn");
            }
            // Nodes to be converted to widgets. We will identify them by SourceClassName
            // In some cases we may have to specify more conditions here to match a specific node
            else if (source.SourceClassName == "DancingGoatCore.AboutUsSection")
            {
                // The widget identifier must match the one defined in the XbyK target project
                options.AsWidget("DancingGoat.Widgets.AboutUsSection", null, null, options =>
                {
                    // Determine where to embed the widget
                    options.Location
                        .OnAncestorPage(-1)
                        // The area has to match what's defined in the XbyK project template's view
                        .InEditableArea("top")
                        // The section name has to match what's defined in the XbyK project
                        .InSection("DancingGoat.SingleColumnSection")
                        .InFirstZone();

                    // Construct the widget's properties
                    options.Properties.Fill(true, (itemProps, reusableItemGuid, childGuids) =>
                    {
                        // Simple way to achieve basic conversion of all properties
                        // var widgetProps = JObject.FromObject(itemProps);
                        // Skipping this step because this is already shown in the samples.
                        // We want to link the object as reusable content item instead - there's no need to duplicate the values in properties

                        var widgetProps = new JObject();

                        // Convert the page to a reusable item using ConvertClassesToContentHub in appsettings.json
                        // Then use a single widget property to link the converted page
                        widgetProps["aboutUsSectionItem"] = LinkedItemPropertyValue(reusableItemGuid!.Value);
                        widgetProps["alignment"] = "ImageLeft"; // Example of setting a specific property value

                        return widgetProps;
                    });
                });
            }
            else
            {
                // Discard all other child nodes or handle them differently - based on your client's needs
                options.Drop();
            }
        }
        else
        {
            // Add any handling you want to apply to pages that are not under the /About-us node
        }
    }
}