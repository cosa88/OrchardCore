using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Contents.Services;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Feeds;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Lists.AdminNodes;
using OrchardCore.Lists.Drivers;
using OrchardCore.Lists.Feeds;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Liquid;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.Settings;
using OrchardCore.Lists.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using YesSql.Indexes;

namespace OrchardCore.Lists
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ListPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IIndexProvider, ContainedPartIndexProvider>();
            services.AddScoped<IContentDisplayDriver, ContainedPartDisplayDriver>();
            services.AddContentPart<ContainedPart>();
            services.AddTransient<IContentAdminFilter, ListPartContentAdminFilter>();

            // List Part
            services.AddContentPart<ListPart>()
                .UseDisplayDriver<ListPartDisplayDriver>()
                .AddHandler<ListPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ListPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentItemIndexHandler, ContainedPartContentIndexHandler>();

            // Feeds
            // TODO: Create feature
            services.AddScoped<IFeedQueryProvider, ListFeedQuery>();
            services.AddContentPart<ListPart>()
                .UseDisplayDriver<ListPartFeedDisplayDriver>()
                .AddHandler<ListPartFeedHandler>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "ListFeed",
                areaName: "OrchardCore.Feeds",
                pattern: "Contents/Lists/{contentItemId}/rss",
                defaults: new { controller = "Feed", action = "Index", format = "rss" }
            );
        }
    }


    [RequireFeatures("OrchardCore.AdminMenu")]
    public class AdminMenuStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<ListsAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, ListsAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, ListsAdminNodeDriver>();
        }
    }
    [RequireFeatures("OrchardCore.ContentLocalization")]
    public class ContentLocalizationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentLocalizationPartHandler, ContainedPartLocalizationHandler>();
            services.AddScoped<IContentLocalizationPartHandler, ListPartLocalizationHandler>();
            services.AddContentPart<LocalizationPart>()
                .AddHandler<ContainedPartHandler>();
        }
    }

    [RequireFeatures("OrchardCore.Liquid")]
    public class LiquidStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLiquidFilter<ListCountFilter>("list_count");
            services.AddLiquidFilter<ListItemsFilter>("list_items");
        }
    }

}
