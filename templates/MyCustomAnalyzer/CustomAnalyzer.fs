/// <summary>
/// Custom F# Analyzer Template
/// 
/// **FOR AI AGENTS**: This is a minimal template. Follow these steps:
/// 1. Study the working patterns in LintKit.AnalyzerPatterns project
/// 2. Reference SimpleAnalyzerExample.fs for complete implementation structure
/// 3. Use the rule specifications provided in AI_RULE_IMPLEMENTATION.md
/// 4. Implement your analyzer following the established patterns
/// 
/// **FOR HUMAN DEVELOPERS**: 
/// - Fill out rule specifications in AI_RULE_IMPLEMENTATION.md
/// - Use AI agent to generate complete implementation
/// - Reference LintKit.AnalyzerPatterns for advanced AST patterns
/// </summary>
namespace MyCustomAnalyzer

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

module CustomAnalyzer =
    
    /// <summary>
    /// TODO: Implement your analysis logic here
    /// 
    /// **AI GUIDANCE**: 
    /// - Study SimpleAnalyzerExample.fs for the complete implementation pattern
    /// - Use helper functions to analyze specific AST nodes
    /// - Follow exhaustive pattern matching without wildcards
    /// - Include proper error handling
    /// </summary>
    let private analyzeExpression (expr: SynExpr) : Message list =
        // TODO: Implement your expression analysis logic
        // Reference: LintKit.AnalyzerPatterns/SynExprPatterns.fs for all expression types
        []
    
    /// <summary>
    /// TODO: Implement module declaration analysis
    /// 
    /// **AI GUIDANCE**:
    /// - Reference SynModuleDeclPatterns.fs for complete pattern matching
    /// - Handle let bindings, type definitions, nested modules, etc.
    /// </summary>
    let private analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        // TODO: Implement your module declaration analysis
        // Reference: LintKit.AnalyzerPatterns/SynModuleDeclPatterns.fs
        []
    
    /// <summary>
    /// Main analyzer function - entry point for the analyzer
    /// 
    /// **AI GUIDANCE**:
    /// - Keep this structure: [<CliAnalyzer>] attribute, Analyzer<CliContext> type
    /// - Use async computation expression
    /// - Include try-catch for error handling
    /// - Return messages as List<Message>
    /// </summary>
    [<CliAnalyzer>]
    let customAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                        for moduleOrNs in modules do
                            match moduleOrNs with
                            | SynModuleOrNamespace(_, _, _, decls, _, _, _, _, _) ->
                                // TODO: Process each declaration
                                let declMessages = decls |> List.collect analyzeModuleDeclaration
                                messages.AddRange declMessages
                    
                    | ParsedInput.SigFile(_) ->
                        // TODO: Handle signature files if needed
                        ()
                
                with
                | ex ->
                    // Error handling - always include this pattern
                    messages.Add({
                        Type = "Custom Analyzer"
                        Message = $"Error during analysis: {ex.Message}"
                        Code = "CUSTOM999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }