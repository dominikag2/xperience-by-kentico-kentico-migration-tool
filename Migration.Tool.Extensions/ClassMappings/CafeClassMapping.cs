using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Kentico.Xperience.UMT.Model;
using Kentico.Xperience.UMT.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Migration.Tool.Common.Builders;
using Migration.Tool.Common.Helpers;

// ReSharper disable ArrangeMethodOrOperatorBody

namespace Migration.Tool.Extensions.ClassMappings;

public static class CafeClassMapping
{

    public static IServiceCollection AddTaxonomyFieldToCafe(this IServiceCollection serviceCollection)
    {
        const string targetClassName = "DancingGoatCore.CafeWithTaxonomy";
        // declare target class
        var mapping = new MultiClassMapping(targetClassName, target =>
        {
            target.ClassName = targetClassName;
            target.ClassTableName = "DancingGoatCore_CafeWithTaxonomy";
            target.ClassDisplayName = "Cafe with Taxonomy";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.REUSABLE;
        });

        // set new primary key
        mapping.BuildField("CafeID").AsPrimaryKey();

        // change fields according to new requirements
        const string sourceClassName = "DancingGoatCore.Cafe";

        mapping.BuildField("CafeName").SetFrom(sourceClassName, "CafeName", true);
        mapping.BuildField("CafeCity").SetFrom(sourceClassName, "CafeCity", true);

        // var taxonomyService = new TaxonomyFieldService(Service.Resolve<IImporter>(), Service.Resolve<ILogger<TaxonomyFieldService>>());
        // var taxonomyGuid = taxonomyService.CreateTaxonomy("MojaTax", "Moja Tax", "blablabla")
        //     .GetAwaiter()
        //     .GetResult();

        mapping.AddField("CafeCategories", "taxonomy")
            .WithFieldPatch(field =>
            {
                field.AllowEmpty = true;
                field.System = false;
                // Configure as taxonomy/content item reference
                field.DataType = "taxonomy";
                field.Settings["controlname"] = "Kentico.Administration.TagSelector";
                // field.Settings["TaxonomyGroup"] = "[\"" + taxonomyGuid + "\"]";
                field.Settings["TaxonomyGroup"] = "[\"1C9D79E0-482E-468C-9C2A-6CBB53BE53F7\"]";
            });

        // register class mapping
        serviceCollection.AddSingleton<IClassMapping>(mapping);

        return serviceCollection;
    }
}

public interface ITaxonomyFieldService
{
    public Task<Guid> CreateTaxonomy(string taxonomyName, string taxonomyTitle, string taxonomyDescription);
}

public class TaxonomyFieldService : ITaxonomyFieldService
{
    private readonly IImporter importer;
    private readonly ILogger<TaxonomyFieldService> logger;
    public TaxonomyFieldService(IImporter importer, ILogger<TaxonomyFieldService> logger)
    {
        this.importer = importer;
        this.logger = logger;
    }

    public async Task<Guid> CreateTaxonomy(string taxonomyName, string taxonomyTitle, string taxonomyDescription)
    {
        logger.LogInformation("Starting the taxonomy creation.");

        var importedTaxonomy = await importer.ImportAsync(new TaxonomyModel
        {
            TaxonomyName = taxonomyName,
            TaxonomyGUID = GuidHelper.CreateTaxonomyGuid(taxonomyName),
            TaxonomyTitle = taxonomyTitle,
            TaxonomyDescription = taxonomyDescription,
            TaxonomyTranslations = null
        });

        if (importedTaxonomy.Imported is TaxonomyInfo taxonomy)
        {
            return taxonomy.TaxonomyGUID;
        }
        else
        {
            return Guid.Empty;
        }
    }
}