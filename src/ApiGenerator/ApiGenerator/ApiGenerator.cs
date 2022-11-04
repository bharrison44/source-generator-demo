using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Text;

#if DEBUGGEN
using System.Diagnostics;
#endif

namespace ApiGenerator
{
    /// <summary>
    /// Generates API models and controllers from entries in a JSON document.
    /// </summary>
    [Generator]
    public class ApiGenerator : ISourceGenerator
    {
        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUGGEN
        // Attach debugger when using dedicated debugging configuration. Should not be used within VS.
        if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            AdditionalText? jsonFile = context.AdditionalFiles.Where(file => file.Path.EndsWith("api.json")).FirstOrDefault();

            if (jsonFile is null)
                return;

            string? json = jsonFile.GetText()?.ToString();

            if (string.IsNullOrWhiteSpace(json))
                return;

            ModelDefinition[] modelDefs = JsonConvert.DeserializeObject<ModelDefinition[]>(json);

            foreach (ModelDefinition modelDef in modelDefs)
            {
                GenerateForModelDefinition(modelDef, context);
            }

            GenerateRepositoryRegistration(modelDefs, context);
        }

        private void GenerateRepositoryRegistration(ModelDefinition[] modelDefs, GeneratorExecutionContext context)
        {
            var @namespace = context.Compilation.AssemblyName;
         
            var builder = new StringBuilder();

            builder.AppendLine($"namespace {@namespace}");
            builder.AppendLine($"{{");

            builder.AppendLine($"   public static class RepositoryExtensions");
            builder.AppendLine($"   {{");
            builder.AppendLine($"        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddGeneratedRepositories(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            builder.AppendLine($"        {{");

            foreach (var modelDef in modelDefs)
            {
                builder.AppendLine($"            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<{@namespace}.{modelDef.Name}Repository>(services);");
            }

            builder.AppendLine($"            return services;");
            builder.AppendLine($"        }}");
            builder.AppendLine($"    }}");
            builder.AppendLine($"}}");

            string generatedText = builder.ToString();

            SourceText sourceText = SourceText.From(generatedText, Encoding.UTF8);
            context.AddSource($"Helper.Generated.cs", sourceText);
        }

        private void GenerateForModelDefinition(ModelDefinition modelDefinition, GeneratorExecutionContext context)
        {
            var @namespace = context.Compilation.AssemblyName;
            var keyDefs = modelDefinition.Props.Where(p => p.Key).ToArray();
            var keyType = keyDefs.Length switch
            {
                1 => keyDefs[0].Type,
                > 1 => $"({string.Join(", ", keyDefs.Select(k => k.Type))})",
                _ => throw new Exception("No keys"),
            };
            var keyParams = string.Join(", ", keyDefs.Select(keyDef => $"[Microsoft.AspNetCore.Mvc.FromQuery] {keyDef.Type} {ToParamName(keyDef.Name)}"));
            var paramKeyValue = keyDefs.Length switch
            {
                1 => ToParamName(keyDefs[0].Name),
                > 1 => $"({string.Join(", ", keyDefs.Select(k => ToParamName(k.Name)))})",
                _ => throw new Exception("No keys"),
            };
            var modelKeyValue = keyDefs.Length switch
            {
                1 => $"model.{keyDefs[0].Name}",
                > 1 => $"({string.Join(", ", keyDefs.Select(k => $"model.{k.Name}"))})",
                _ => throw new Exception("No keys"),
            };

            var builder = new StringBuilder();

            builder.AppendLine($"namespace {@namespace}");
            builder.AppendLine($"{{");

            builder.AppendLine($"   public class {modelDefinition.Name}Repository : System.Collections.Generic.Dictionary<{keyType}, {modelDefinition.Name}Model>");
            builder.AppendLine($"   {{");
            builder.AppendLine($"   }}");
            builder.AppendLine();

            builder.AppendLine($"   public class {modelDefinition.Name}Model");
            builder.AppendLine($"   {{");

            foreach (var propDef in modelDefinition.Props)
            {
                builder.AppendLine($"       public {propDef.Type} {propDef.Name} {{ get; set; }}");
                builder.AppendLine();
            }

            builder.AppendLine($"   }}");
            builder.AppendLine();

            builder.AppendLine($"}}");
            builder.AppendLine();

            builder.AppendLine($"namespace {@namespace}.Controllers");
            builder.AppendLine($"{{");

            builder.AppendLine($"   [Microsoft.AspNetCore.Mvc.ApiController]");
            builder.AppendLine($"   [Microsoft.AspNetCore.Mvc.Route(\"[controller]\")]");
            builder.AppendLine($"   public class {modelDefinition.Name}Controller : Microsoft.AspNetCore.Mvc.ControllerBase");
            builder.AppendLine($"   {{");

            // private vars
            builder.AppendLine($"       private readonly {modelDefinition.Name}Repository _Repository;");

            // constructor
            builder.AppendLine($"       public {modelDefinition.Name}Controller({modelDefinition.Name}Repository repository)");
            builder.AppendLine($"       {{");
            builder.AppendLine($"           _Repository = repository;");
            builder.AppendLine($"       }}");
            builder.AppendLine();

            // get
            builder.AppendLine($"       [Microsoft.AspNetCore.Mvc.HttpGet]");
            builder.AppendLine($"       public {@namespace}.{modelDefinition.Name}Model? Get({keyParams})");
            builder.AppendLine($"       {{");
            builder.AppendLine($"           return _Repository.TryGetValue({paramKeyValue}, out {modelDefinition.Name}Model? model) ? model : null;");
            builder.AppendLine($"       }}");
            builder.AppendLine();

            // get all
            builder.AppendLine($"       [Microsoft.AspNetCore.Mvc.HttpGet(\"All\")]");
            builder.AppendLine($"       public {@namespace}.{modelDefinition.Name}Model[] GetAll()");
            builder.AppendLine($"       {{");
            builder.AppendLine($"           return System.Linq.Enumerable.ToArray(_Repository.Values);");
            builder.AppendLine($"       }}");
            builder.AppendLine();

            // post
            builder.AppendLine($"       [Microsoft.AspNetCore.Mvc.HttpPost]");
            builder.AppendLine($"       public {@namespace}.{modelDefinition.Name}Model? Post([Microsoft.AspNetCore.Mvc.FromBody] {modelDefinition.Name}Model model)");
            builder.AppendLine($"       {{");
            builder.AppendLine($"           _Repository[{modelKeyValue}] = model;");
            builder.AppendLine($"           return model;");
            builder.AppendLine($"       }}");
            builder.AppendLine();

            // patch
            builder.AppendLine($"       [Microsoft.AspNetCore.Mvc.HttpPatch]");
            builder.AppendLine($"       public {@namespace}.{modelDefinition.Name}Model? Patch([Microsoft.AspNetCore.Mvc.FromBody] {modelDefinition.Name}Model model)");
            builder.AppendLine($"       {{");
            builder.AppendLine($"           if (!_Repository.TryGetValue({modelKeyValue}, out {modelDefinition.Name}Model? existing))");
            builder.AppendLine($"               throw new System.Exception(\"Not found\");");
            builder.AppendLine();

            foreach (var prop in modelDefinition.Props.Where(p => !p.Key))
            {
                builder.AppendLine($"           existing.{prop.Name} = model.{prop.Name};");
            }

            builder.AppendLine();
            builder.AppendLine($"           return existing;");
            builder.AppendLine($"       }}");
            builder.AppendLine();

            builder.AppendLine($"       [Microsoft.AspNetCore.Mvc.HttpDelete]");
            builder.AppendLine($"       public void Delete({keyParams})");
            builder.AppendLine($"       {{");
            builder.AppendLine($"           _Repository.Remove({paramKeyValue}, out _);");
            builder.AppendLine($"       }}");

            builder.AppendLine($"   }}");

            builder.AppendLine($"}}");
            builder.AppendLine();

            string generatedText = builder.ToString();

            SourceText sourceText = SourceText.From(generatedText, Encoding.UTF8);
            context.AddSource($"{modelDefinition.Name}.Generated.cs", sourceText);
        }

        private string ToParamName(string propName)
        {
            char first = propName.ToLower()[0];
            string rest = new string(propName.Skip(1).ToArray());

            return $"{first}{rest}";
        }
    }
}