namespace ADORE.Configuration
{
	public class GeneratorTemplateConfig
	{
		public string TemplateName { get; set; }
		public string FilenameFormat { get; set; }
		public string NamespaceFormat { get; set; }
		public string NameFormat { get; set; }

		public static readonly GeneratorTemplateConfig DefaultCatalogTemplate = new GeneratorTemplateConfig() {
			TemplateName = "Catalog",
			FilenameFormat = "_catalog_{Catalog}",
			NamespaceFormat = ".{Catalog}",
			NameFormat = "{Catalog}Catalog",
		};
		public static readonly GeneratorTemplateConfig DefaultSchemaTemplate = new GeneratorTemplateConfig() {
			TemplateName = "Schema",
			FilenameFormat = "_schema_{Catalog}_{Schema}",
			NamespaceFormat = ".{Catalog}.{Schema}",
			NameFormat = "{Schema}Schema",
		};
		public static readonly GeneratorTemplateConfig DefaultTableTemplate = new GeneratorTemplateConfig() {
			TemplateName = "Table",
			FilenameFormat = "_table_{Catalog}_{Schema}_{Table}",
			NamespaceFormat = ".{Catalog}.{Schema}.Tables",
		};
		public static readonly GeneratorTemplateConfig DefaultViewTemplate = new GeneratorTemplateConfig() {
			TemplateName = "View",
			FilenameFormat = "_view_{Catalog}_{Schema}_{Table}",
			NamespaceFormat = ".{Catalog}.{Schema}.Views",
		};
		public static readonly GeneratorTemplateConfig DefaultProcedureTemplate = new GeneratorTemplateConfig() {
			TemplateName = "Procedure",
			FilenameFormat = "_proc_{Catalog}_{Schema}_{Routine}",
		};
		public static readonly GeneratorTemplateConfig DefaultFunctionTemplate = new GeneratorTemplateConfig() {
			TemplateName = "Function",
			FilenameFormat = "_func_{Catalog}_{Schema}_{Routine}",
		};
		public static readonly GeneratorTemplateConfig DefaultResultsetTemplate = new GeneratorTemplateConfig()
		{
			TemplateName = "Resultset",
			FilenameFormat = "_resultset_{Catalog}_{Schema}_{Table}",
			NamespaceFormat = ".{Catalog}.{Schema}.Resultsets",
		};

	}
}
