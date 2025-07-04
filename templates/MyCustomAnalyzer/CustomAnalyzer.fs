/// <summary>
/// Custom F# Analyzer Template - EDIT THIS FILE
/// 
/// **🤖 FOR AI AGENTS (THIS MEANS YOU)**: REPLACE the TODO implementations in this file with your analyzer logic:
/// 1. EDIT the analyzeExpression function to implement your expression analysis
/// 2. EDIT the analyzeModuleDeclaration function to implement your module analysis
/// 3. EDIT the customAnalyzer function to process your specific rules
/// 4. Study SimpleAnalyzerExample.fs for complete implementation patterns
/// 5. Reference LintKit.AnalyzerPatterns files for specific AST pattern matching
/// 
/// **FOR HUMAN DEVELOPERS**: 
/// - Fill out rule specifications in AI_RULE_IMPLEMENTATION.md
/// - Use AI agent to generate complete implementation by editing this file
/// - Reference LintKit.AnalyzerPatterns for advanced AST patterns
/// </summary>
namespace MyCustomAnalyzer

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

module CustomAnalyzer =
    
    /// <summary>
    /// EDIT THIS FUNCTION: Replace the empty list with your expression analysis logic
    /// 
    /// **🤖 AI GUIDANCE (FOR YOU)**: 
    /// - Study SimpleAnalyzerExample.fs for the complete implementation pattern
    /// - Use SynExprPatterns.fs for all expression pattern matching examples
    /// - Follow exhaustive pattern matching without wildcards
    /// - Return Message list with rule violations found
    /// </summary>
    let private analyzeExpression (expr: SynExpr) : Message list =
        // TODO: Implement your expression analysis logic
        // Reference: LintKit.AnalyzerPatterns/SynExprPatterns.fs for all expression types
        []
    
    /// <summary>
    /// EDIT THIS FUNCTION: Replace the empty list with your module declaration analysis logic
    /// 
    /// **AI GUIDANCE**:
    /// - Reference SynModuleDeclPatterns.fs for complete pattern matching
    /// - Handle let bindings, type definitions, nested modules, etc.
    /// - Return Message list with rule violations found
    /// </summary>
    let private analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        // TODO: Implement your module declaration analysis
        // Reference: LintKit.AnalyzerPatterns/SynModuleDeclPatterns.fs
        []
    
    /// <summary>
    /// EDIT THIS FUNCTION: Replace the TODO comments with your analyzer logic
    /// 
    /// **AI GUIDANCE**:
    /// - Keep this structure: [<CliAnalyzer>] attribute, Analyzer<CliContext> type
    /// - EDIT the module processing logic to call your helper functions
    /// - The async computation expression and error handling are already set up
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