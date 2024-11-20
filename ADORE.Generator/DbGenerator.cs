using ADORE.Configuration;
using ADORE.Generator.Models;
using ADORE.Generator.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace ADORE.Generator
{
    [Generator]
	public class DbGenerator : IIncrementalGenerator
	{
		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			// get config files
			var dbgens = context.AdditionalTextsProvider.Where(atp => {
				if(atp.Path.EndsWith(".config"))
				{
					var configRoot = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(atp.Path)).AddJsonFile(Path.GetFileName(atp.Path)).Build();
					var adoreConfigSection = configRoot.GetSection("ADORE:CodeGenerators");
					return adoreConfigSection != null;
				}
				return false;
			});

			// prototype generator runs always, VS will run this often to update Intellisense
			context.RegisterSourceOutput(dbgens, GeneratePrototypes);

			// full-code generator runs when full-code is needed. VS will usually skip it, but MSBuild will always run it
			context.RegisterImplementationSourceOutput(dbgens, GenerateFullCode);
		}

		#region prototype-only generator
		public void GeneratePrototypes(SourceProductionContext context, AdditionalText dbgen)
		{
			SetConfig(dbgen);

			// go through each code generator config and run it
			foreach(var codegen in _config.CodeGenerators)
			{
				// get the config dependencies
				_codegenConfig = codegen;
				_connectionConfig = _config.ConnectionStrings.Where(cs => cs.ConnectionName == codegen.ConnectionName).First();
				_providerConfig = _config.ProviderFactories.Where(pf => pf.ProviderName == _connectionConfig.ProviderName).First();

				// set up the DB stuff with the current configs
				SetCurrentConnection();

				// get data
				var schemas = GetSchemas();
				var tables = GetTables(_queryMap.TableQuery);
				var views = GetTables(_queryMap.ViewQuery);
				var procedures = GetRoutines(_queryMap.ProcedureQuery, _queryMap.RoutineResultsetQuery);
				var functions = GetRoutines(_queryMap.FunctionQuery, _queryMap.RoutineResultsetQuery);

				// generate tables
				foreach(var table in tables)
				{
					GenerateTableClass(context, table, _codegenConfig.TableTemplate, true);
				}
				// generate views
				foreach(var view in views)
				{
					GenerateTableClass(context, view, _codegenConfig.ViewTemplate, true);
				}
				// generate resultset "tables"
				foreach(var proc in procedures.Where(p => p.Resultset != null))
				{
					GenerateTableClass(context, proc.Resultset, _codegenConfig.ResultsetTemplate, true);
				}
				foreach(var func in functions.Where(f => f.Resultset != null))
				{
					GenerateTableClass(context, func.Resultset, _codegenConfig.ResultsetTemplate, true);
				}
				// generate schemas with routines
				// generate catalog
			}
		}
		#endregion

		#region full-code generator
		public void GenerateFullCode(SourceProductionContext context, AdditionalText dbgen)
		{
			SetConfig(dbgen);
		}
		#endregion

		#region code file content generators
		private void GenerateTableClass(SourceProductionContext context, TableSpec table, GeneratorTemplateConfig templateConfig, bool isPrototype = false)
		{
			var mergeValues = new Dictionary<string, string>() {
				{"Catalog", _connectionConfig.CatalogName},
				{"Schema", table.SchemaName},
				{"Table", FixCase(table.TableName)},
			};
			string filename = Merge(_codegenConfig.BaseFilename + templateConfig.FilenameFormat + _codegenConfig.PrototypeFilenameExtension, mergeValues);
			string ns = Merge(templateConfig.NamespaceFormat, mergeValues);
			string classname = Merge(templateConfig.NameFormat ?? "{Table}", mergeValues);
			context.AddSource(filename, SourceText.From(table.Generate(ns, classname, isPrototype), Encoding.UTF8));
		}
		private void GenerateSchemaClass(SourceProductionContext context, SchemaSpec schema, IEnumerable<RoutineSpec> procedures, IEnumerable<RoutineSpec> functions, bool isPrototype = false)
		{
		}
		private void GenerateCatalogClass(SourceProductionContext context, IEnumerable<SchemaSpec> schemas, bool isPrototype = false)
		{
		}
		#endregion

		#region configs
		private AdoreConfig _config;
		private ConnectionStringConfig _connectionConfig;
		private ProviderFactoryConfig _providerConfig;
		private CodeGeneratorConfig _codegenConfig;

		private void SetConfig(AdditionalText dbgen)
		{
			_config = new ConfigurationBuilder()
				.SetBasePath(Path.GetDirectoryName(dbgen.Path))
				.AddJsonFile(Path.GetFileName(dbgen.Path))
				.Build()
				.GetSection("ADORE")
				.Get<AdoreConfig>();

			// TODO: add azure keyvault support for config secrets
		}
		#endregion

		#region data retrieval
		private string _providerName;
		private DbProviderFactory _factory;
		private ProviderQueryMapSpec _queryMap;

		private void SetCurrentConnection()
		{
			// only get a new provider if the provider name changed
			if(_providerName != _providerConfig.ProviderName)
			{
				_providerName = _providerConfig.ProviderName;
				_factory = (DbProviderFactory)Activator.CreateInstance(Type.GetType(_providerConfig.FactoryTypeName));
				_queryMap = ProviderQueryMapSpec.ProviderQueryMaps[_factory.GetType().Name];
			}
		}

		private DataTable RunQuery(string sql, Dictionary<string,object> parameters = null)
		{
			// get a connection
			using(var conn = _factory.CreateConnection())
			{
				conn.ConnectionString = _connectionConfig.ConnectionString;
				// get a command
				using(var cmd = conn.CreateCommand())
				{
					cmd.CommandText = sql;
					// set parameters, if needed
					if(parameters != null)
					{
						foreach(var kvp in parameters)
						{
							var p = cmd.CreateParameter();
							p.ParameterName = kvp.Key;
							p.Value = kvp.Value;
							cmd.Parameters.Add(p);
						}
					}

					// get a data adapter
					using(var dda = _factory.CreateDataAdapter())
					{
						dda.SelectCommand = cmd;
						DataSet ds = new DataSet();
						try
						{
							// run the query by having the data adapter fill a dataset
							conn.Open();
							dda.Fill(ds);
						}
						finally
						{
							conn.Close();
						}
						// and only send back the first table of the dataset
						return ds.Tables[0];
					}
				}
			}
		}

		private IEnumerable<SchemaSpec> GetSchemas()
		{
			var dt = RunQuery(_queryMap.SchemaQuery);
			List<SchemaSpec> schemas = new List<SchemaSpec>();
			foreach(DataRow dr in dt.Rows)
			{
				schemas.Add(new SchemaSpec() {
					CatalogName = _connectionConfig.CatalogName,
					SchemaName = dr["schema_name"] as string,
				});
			}
			return schemas;
		}
		private IEnumerable<TableSpec> GetTables(string sql)
		{
			var dt = RunQuery(sql);
			List<TableSpec> tables = new List<TableSpec>();
			TableSpec curTable = new TableSpec();
			string prevTableName = null;
			foreach(DataRow dr in dt.Rows)
			{
				// resultset is flattened tables + columns, when the table name changes, start a new table
				if(prevTableName is null || prevTableName != dr["table_name"] as string)
				{
					curTable = new TableSpec() {
						SchemaName = dr["schema_name"] as string,
						TableName = dr["table_name"] as string,
					};
					tables.Add(curTable);
					prevTableName = curTable.TableName;
				}
				// always add the column!
				curTable.Columns.Add(new ColumnSpec() {
					Name = dr["column_name"] as string,
					Order = (int)dr["column_order"],
					MaxLength = (int)dr["max_length"],
					Precision = (int)dr["precision"],
					Scale = (int)dr["scale"],
					IsNullable = ((int)dr["is_nullable"] == 1),
					TypeName = dr["type_name"] as string,
				});
			}
			return tables;
		}
		private IEnumerable<RoutineSpec> GetRoutines(string sql, string resultsetSql)
		{
			var dt = RunQuery(sql);
			List<RoutineSpec> routines = new List<RoutineSpec>();

			#region routine list
			RoutineSpec curRoutine = new RoutineSpec();
			string prevRoutineName = null;
			foreach(DataRow dr in dt.Rows)
			{
				// resultset is flattened routine + parameters, when the routine name changes, start a new routine
				if(prevRoutineName is null || prevRoutineName != dr["routine_name"] as string)
				{
					curRoutine = new RoutineSpec() {
						SchemaName = dr["schema_name"] as string,
						RoutineName = dr["routine_name"] as string,
					};
					routines.Add(curRoutine);
					prevRoutineName = curRoutine.RoutineName;
				}
				// always add the parameter!
				curRoutine.Parameters.Add(new ParameterSpec() {
					Name = dr["parameter_name"] as string,
					Order = (int)dr["parameter_order"],
					MaxLength = (int)dr["max_length"],
					Precision = (int)dr["precision"],
					Scale = (int)dr["scale"],
					IsNullable = true,
					TypeName = dr["type_name"] as string,
					Direction = (dr["parameter_mode"] as string == "IN" ? ParameterDirection.Input : ParameterDirection.InputOutput),
				});
			}
			#endregion

			#region routine resultsets
			if(!string.IsNullOrEmpty(resultsetSql))
			{
				Dictionary<string,object> parameters = new Dictionary<string, object>();
				parameters.Add("@routine_name", string.Empty);

				foreach(var routine in routines)
				{
					try
					{
						parameters["@routine_name"] = routine.RoutineName;
						dt = RunQuery(resultsetSql, parameters);

						if(dt.Rows.Count > 0) // only bother to set the resultset object if there was anything found
						{
							// getting here means there won't be a SqlException at least...
							routine.Resultset = new TableSpec() {
								SchemaName = routine.SchemaName,
								TableName = routine.RoutineName + "_resultset",
							};

							foreach(DataRow dr in dt.Rows)
							{
								routine.Resultset.Columns.Add(new ColumnSpec() {
									Name = dr["name"] as string,
									Order = (int)dr["column_ordinal"],
									MaxLength = (int)dr["max_length"],
									Precision = (int)dr["precision"],
									Scale = (int)dr["scale"],
									IsNullable = ((int)dr["is_nullable"] == 1),
									TypeName = dr["system_type_name"] as string ?? dr["user_type_name"] as string,
								});
							}
						}
					}
					catch { continue; } // ignore the exception and go to the next item
				}
			}
			#endregion

			return routines;
		}
		#endregion

		#region utility methods
		private const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const string lower = "abcdefghijklmnopqrstuvwxyz";
		private const string numeric = "1234567890";
		private const string alpha = upper + lower;
		private const string alphanumeric = alpha + numeric;

		public static string FixCase(string s)
		{
			string[] tokens = Tokenize(s);
			for(int x = 0; x < tokens.Length; x++)
			{
				// force lowercase, then CapFirst()
				tokens[x] = CapFirst(tokens[x].ToLowerInvariant());
			}
			// join them all with an empty string and return
			return string.Join(string.Empty, tokens);
		}
		public static string CapFirst(string s)
		{
			// if the first character is lowercase, return with it replaced with an upper
			int idx = lower.IndexOf(s[0]);
			if(idx > -1) { return upper[idx] + s.Substring(1); }
			// otherwise, return as-is
			return s;
		}
		public static string[] Tokenize(string s)
		{
			List<string> tokens = new List<string>();
			bool zerolen;
			for(int x = 0, pos = 0; x < s.Length; x++)
			{
				// check if it's zero-length. we don't tokenize zero-length under any circumstances.
				zerolen = (x - pos <= 0);

				// if this is the last character in the string, tokenize it.
				if(!zerolen && x == s.Length - 1)
				{
					tokens.Add(s.Substring(pos, x - pos));
					continue;
				}

				// non-alphanumerics cause a token break.
				if(alphanumeric.IndexOf(s[x]) == -1)
				{
					// make a token for everything since the last token, minus this character
					if(!zerolen) { tokens.Add(s.Substring(pos, x - pos)); }

					// set the new position past this character, since this character is unusable in the next token.
					pos = x + 1;
					
					continue;
				}

				// lower-upper gets a token break between them.
				// handled as current character is upper, previous character is lower. this avoids weird out-of-loop advancing.
				// also, this test only happens when x > 0 because x == 0 has no previous character.
				if(x > 0 && upper.IndexOf(s[x]) > -1 && lower.IndexOf(s[x - 1]) > -1)
				{
					// tokenize what's before the upper
					if(!zerolen) { tokens.Add(s.Substring(pos, x - pos)); }

					// DO NOT set the position past the current character. the next token needs to begin with this character.
					pos = x;

					continue;
				}
			}
			return tokens.ToArray();
		}

		public static string Merge(string s, Dictionary<string, string> mergeValues)
		{
			foreach(var kvp in mergeValues)
			{
				s = s.Replace("{" + kvp.Key + "}", kvp.Value);
			}
			return s;
		}
		#endregion
	}
}
