namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// <summary>
/// Simple analyzer example for detecting TODO comments in F# code
/// 
/// **AI Learning Reference**: This demonstrates the simplest possible analyzer implementation that:
/// - Scans for string literals containing "TODO"
/// - Creates appropriate messages with codes and severity
/// - Shows basic AST pattern matching for SynExpr.Const
/// - Includes proper error handling and async structure
/// 
/// **For AI Agents**: Copy and adapt these patterns when implementing new analyzers.
/// **For Humans**: Use this as a starting point for more complex analyzers.
/// </summary>
module SimpleAnalyzerExample =
    
    /// <summary>
    /// Recursively searches for TODO strings in expressions
    /// </summary>
    /// <param name="expr">F# expression to analyze</param>
    /// <returns>List of messages for found TODOs</returns>
    let rec findTodoInExpression (expr: SynExpr) : Message list =
        match expr with
        // Check string literals for TODO
        | SynExpr.Const(SynConst.String(text, _, _), range) when text.Contains("TODO") ->
            [{
                Type = "Simple Analyzer"
                Message = $"TODO found in string literal: '{text}'. Consider creating a proper task."
                Code = "SIMPLE001"
                Severity = Severity.Info
                Range = range
                Fixes = []
            }]
        
        // Recursively check other expressions that might contain nested expressions
        | SynExpr.App(_, _, funcExpr, argExpr, _) ->
            (findTodoInExpression funcExpr) @ (findTodoInExpression argExpr)
        
        | SynExpr.LetOrUse(_, _, bindings, body, _, _) ->
            let bindingMessages = 
                bindings |> List.collect (fun binding ->
                    match binding with
                    | SynBinding(_, _, _, _, _, _, _, _, _, expr, _, _, _) ->
                        findTodoInExpression expr)
            let bodyMessages = findTodoInExpression body
            bindingMessages @ bodyMessages
        
        // For all other expression types, no messages
        | _ -> []
    
    /// <summary>
    /// Analyzes module declarations for TODO strings
    /// </summary>
    /// <param name="decl">Module declaration to analyze</param>
    /// <returns>List of messages</returns>
    let rec private analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        match decl with
        | SynModuleDecl.Let(_, bindings, _) ->
            bindings |> List.collect (fun binding ->
                match binding with
                | SynBinding(_, _, _, _, _, _, _, _, _, expr, _, _, _) ->
                    findTodoInExpression expr)
        
        | SynModuleDecl.NestedModule(_, _, decls, _, _, _) ->
            decls |> List.collect analyzeModuleDeclaration
        
        | _ -> []
    
    /// <summary>
    /// Main analyzer function - entry point for the analyzer
    /// </summary>
    [<CliAnalyzer>]
    let simpleAnalyzer: Analyzer<CliContext> =
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
                                let declMessages = decls |> List.collect analyzeModuleDeclaration
                                messages.AddRange declMessages
                    
                    | ParsedInput.SigFile(_) ->
                        // No analysis for signature files in this simple example
                        ()
                
                with
                | ex ->
                    messages.Add({
                        Type = "Simple Analyzer"
                        Message = $"Error during analysis: {ex.Message}"
                        Code = "SIMPLE999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }