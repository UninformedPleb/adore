namespace ADORE.Configuration
{
    public class CodeGeneratorConfig
    {
		public string GeneratorName { get; set; }
		public string ConnectionName { get; set; }
		public string GeneratorProviderName { get; set; } = "default";

		#region base settings
		public string BaseFilename { get; set; }
		public string FilenameExtension { get; set; } = ".g.cs";
		public string PrototypeFilenameExtension { get; set; } = ".proto.g.cs";
		public string BaseNamespace { get; set; }
		#endregion

		#region generator templates
		public GeneratorTemplateConfig CatalogTemplate { get; set; } = GeneratorTemplateConfig.DefaultCatalogTemplate;
		public GeneratorTemplateConfig SchemaTemplate { get; set; } = GeneratorTemplateConfig.DefaultSchemaTemplate;
		public GeneratorTemplateConfig TableTemplate { get; set; } = GeneratorTemplateConfig.DefaultTableTemplate;
		public GeneratorTemplateConfig ViewTemplate { get; set; } = GeneratorTemplateConfig.DefaultViewTemplate;
		public GeneratorTemplateConfig ProcedureTemplate { get; set; } = GeneratorTemplateConfig.DefaultProcedureTemplate;
		public GeneratorTemplateConfig FunctionTemplate { get; set; } = GeneratorTemplateConfig.DefaultFunctionTemplate;
		public GeneratorTemplateConfig ResultsetTemplate { get; set; } = GeneratorTemplateConfig.DefaultResultsetTemplate;
		#endregion
	}
}
