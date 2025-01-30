using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SimpleJSON;

namespace MeidoPhotoStudio.Plugin.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]

public class TranslationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ValidCategoryRule = new(
        "MPS0001",
        "Translation category does not exist",
        "Translation category '{0}' does not exist",
        "Translation",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ValidKeyRule = new(
        "MPS0002",
        "Translation key does not exist",
        "Translation key '{0}' does not exist in category '{1}'",
        "Translation",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [ValidCategoryRule, ValidKeyRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(CompilationStartAction);

        void CompilationStartAction(CompilationStartAnalysisContext context)
        {
            RegisterTranslationAnalyzer(context, context.Options.AdditionalFiles);

            static void RegisterTranslationAnalyzer(
                CompilationStartAnalysisContext context, IEnumerable<AdditionalText> additionalTexts)
            {
                var translationFiles = additionalTexts.Where(static file =>
                    Path.GetExtension(file.Path).Equals(".json", StringComparison.OrdinalIgnoreCase));

                if (!translationFiles.Any())
                    return;

                var translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

                foreach (var translationFile in translationFiles)
                {
                    var fileText = translationFile.GetText(context.CancellationToken);

                    if (fileText is null)
                        continue;

                    JSONNode translationsJson;

                    try
                    {
                        translationsJson = JSON.Parse(fileText.ToString());
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (var categoryNode in translationsJson)
                    {
                        var category = translations[categoryNode.Key] = new Dictionary<string, string>(StringComparer.Ordinal);

                        foreach (var keyNode in categoryNode.Value)
                            category[keyNode.Key] = keyNode.Value;
                    }
                }

                if (translations.Count is 0)
                    return;

                context.RegisterSyntaxNodeAction(AnalyzeTranslationAccessArguments, SyntaxKind.ElementAccessExpression);
                context.RegisterSyntaxNodeAction(AnalyzeLocalizableGUIContentArguments, SyntaxKind.ObjectCreationExpression);

                void AnalyzeTranslationAccessArguments(SyntaxNodeAnalysisContext context)
                {
                    if (context.Node is not ElementAccessExpressionSyntax access)
                        return;

                    if (context.SemanticModel.GetSymbolInfo(access, context.CancellationToken).Symbol is not IPropertySymbol symbol)
                        return;

                    if (access.ArgumentList?.Arguments is not { Count: 2 } args)
                        return;

                    AnalyzeTranslationArguments(context, args[0].Expression, args[1].Expression);
                }

                void AnalyzeLocalizableGUIContentArguments(SyntaxNodeAnalysisContext context)
                {
                    if (context.Node is not ObjectCreationExpressionSyntax creation)
                        return;

                    if (context.SemanticModel.GetTypeInfo(creation, context.CancellationToken) is not { Type.Name: "LocalizableGUIContent" })
                        return;

                    if (creation.ArgumentList?.Arguments is not { Count: 3 } args)
                        return;

                    AnalyzeTranslationArguments(context, args[1].Expression, args[2].Expression);
                }

                void AnalyzeTranslationArguments(SyntaxNodeAnalysisContext context, ExpressionSyntax? categoryExpression, ExpressionSyntax? keyExpression)
                {
                    var model = context.SemanticModel;

                    if (categoryExpression is null || !GetConstantValue(model, categoryExpression, out var category))
                        return;

                    if (!translations.TryGetValue(category, out var categoryTranslations))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ValidCategoryRule, categoryExpression.GetLocation(), category));

                        return;
                    }

                    if (keyExpression is null || !GetConstantValue(model, keyExpression, out var key))
                        return;

                    if (!categoryTranslations.TryGetValue(key, out var translation))
                        context.ReportDiagnostic(Diagnostic.Create(ValidKeyRule, keyExpression.GetLocation(), key, category));

                    bool GetConstantValue(SemanticModel model, ExpressionSyntax expression, out string value)
                    {
                        value = string.Empty;

                        var symbol = model.GetSymbolInfo(expression);
                        var constantValue = model.GetConstantValue(expression, context.CancellationToken);

                        if (constantValue.Value is not string constant)
                            return false;

                        value = constant;

                        return true;
                    }
                }
            }
        }
    }
}
