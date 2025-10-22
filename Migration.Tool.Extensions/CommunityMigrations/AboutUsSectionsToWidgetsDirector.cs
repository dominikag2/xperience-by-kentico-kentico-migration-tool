using Migration.Tool.Source.Mappers.ContentItemMapperDirectives;
using Newtonsoft.Json.Linq;

namespace Migration.Tool.Extensions.CommunityMigrations;

public class AboutUsSectionsToWidgetsDirector : ContentItemDirectorBase
{
    public override void Direct(ContentItemSource source, IContentItemActionProvider options)
    {
        if (source.SourceNode!.NodeAliasPath.StartsWith("/About-Us"))
        {
            // 1. Widget host page tree node: Ensure the template exists and is available in XbyK target project
            if (source.SourceNode.NodeAliasPath == "/About-Us")
            {
                options.OverridePageTemplate("PageWithWidgetsDefaultTemplate");
            }
            // 2. Nodes to be converted to widgets. Here we identify them by alias path. Other methods like SourceClassName are also possible
            //in some cases we may have to specify mode conditions here to match a specific node
            else if (source.SourceClassName == "DancingGoatCore.AboutUsSection")
            {
                // The widget identifier must match the one defined in the XbyK target project
                options.AsWidget("DancingGoat.Widgets.AboutUsSection", null, null, options =>
                {
                    // Determine where to embed the widget
                    options.Location
                        .OnAncestorPage(-1)
                        // the area has to match what's defined in the XbyK project's view
                        .InEditableArea("main-area")
                        // the section name has to match what's defined in the XbyK project
                        .InSection("DancingGoat.SingleColumnSection")
                        .InFirstZone();

                    // Construct the widget's properties
                    options.Properties.Fill(true, (itemProps, reusableItemGuid, childGuids) =>
                    {
                        // Simple way to achieve basic conversion of all properties
                        // Skipping this because this is already shown in the samples.
                        // we want to link the object as reusable content item instead - there's no need to duplicate the values in properties
                        // var widgetProps = JObject.FromObject(itemProps);

                        var widgetProps = new JObject();

                        // Linked the converted page as a reusable content item into a single property of the widget.
                        // Be sure to list the page class name appsettings in ConvertClassesToContentHub to make it reusable 
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
            // don't do anything if the node is not under /About-Us node
        }
    }
}