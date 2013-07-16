using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace TimeTracker
{
    static class Raven
    {
        public static readonly IDocumentStore DocStore =
            new EmbeddableDocumentStore { UseEmbeddedHttpServer = true }.Initialize();

        static Raven()
        {
            IndexCreation.CreateIndexes(typeof(TimeRecordsByTime).Assembly, DocStore);

        }

        public static IDocumentSession OpenSession()
        {
            return DocStore.OpenSession();
        }
    }
}