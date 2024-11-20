using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ADORE.Generator.Metadata
{
    internal class ProviderQueryMapSpec
    {
        public static Dictionary<string, ProviderQueryMapSpec> ProviderQueryMaps { get; private set; }

        static ProviderQueryMapSpec()
        {
            LoadProviderQueryMaps();
        }
        private static void LoadProviderQueryMaps()
        {
            using (var s = typeof(ProviderQueryMapSpec).Assembly.GetManifestResourceStream("ADORE.Generator.Data.ProviderQueryMapData.json"))
            using (var sr = new StreamReader(s))
            {
                foreach (var pqm in JsonSerializer.Deserialize<List<ProviderQueryMapSpec>>(sr.ReadToEnd()))
                {
                    ProviderQueryMaps[pqm.ProviderName] = pqm;
                }
            }
        }

        public string ProviderName { get; set; }
        public string SchemaQuery { get; set; }
        public string TableQuery { get; set; }
        public string ViewQuery { get; set; }
        public string ProcedureQuery { get; set; }
        public string FunctionQuery { get; set; }
        public string RoutineResultsetQuery { get; set; }
    }
}
