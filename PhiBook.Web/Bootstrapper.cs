using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;

namespace PhiBook.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var store = new DocumentStore
                            {
                                ConnectionStringName = "RavenDB"
                            };

            store.Initialize();
            store.DatabaseCommands.EnsureDatabaseExists("PhiBook");
            store.DefaultDatabase = "PhiBook";
            container.Register<IDocumentStore>(store);
        }

        protected override void ConfigureRequestContainer(Nancy.TinyIoc.TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var docStore = container.Resolve<IDocumentStore>();
            var docSession = docStore.OpenSession();
            container.Register(docSession);
        }

        protected override void RequestStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            pipelines.AfterRequest.AddItemToEndOfPipeline(
                (ctx) =>
                    {
                        var session = container.Resolve<IDocumentSession>();

                        if (session == null)
                            return;

                        session.SaveChanges();
                        session.Dispose();
                    });

        }
    }
}