namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

module FunctionApplicationDetector =
    
    /// Searches for function applications in the AST
    let rec findFunctionApplications (expr: SynExpr) : Range list =
        match expr with
        | SynExpr.App(_, _, func, arg, _) ->
            // Found a function application - return its range and recurse
            expr.Range :: (findFunctionApplications func @ findFunctionApplications arg)
        | _ ->
            // Other expressions - no function application here
            []
    
    /// Searches for function applications in declarations  
    let rec findInDeclarations (decls: SynModuleDecl list) : Range list =
        let mutable foundDecls = []
        for decl in decls do
            match decl with
            | SynModuleDecl.Let(_, bindings, _) ->
                foundDecls <- "Let" :: foundDecls
                // Process let bindings
                ()
            | SynModuleDecl.Expr(_, _) ->
                foundDecls <- "Expr" :: foundDecls
            | SynModuleDecl.Types(_, _) ->
                foundDecls <- "Types" :: foundDecls
            | SynModuleDecl.NestedModule(_, _, _, _, _, _) ->
                foundDecls <- "NestedModule" :: foundDecls
            | _ ->
                foundDecls <- "Other" :: foundDecls
        
        // Return a single range to indicate we found something
        if foundDecls.Length > 0 then
            [Range.Zero] // Just to see if we're finding any declarations
        else
            []
    
    [<CliAnalyzer>]
    let functionApplicationDetector: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    // Get the parsed AST from the context
                    let parseResults = context.ParseFileResults
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                        for moduleOrNs in modules do
                            match moduleOrNs with
                            | SynModuleOrNamespace(_, _, _, decls, _, _, _, _, _) ->
                                let ranges = findInDeclarations decls
                                
                                for range in ranges do
                                    messages.Add({
                                        Type = "Function Application Detector"
                                        Message = "Found function application in AST"
                                        Code = "FA001"
                                        Severity = Severity.Warning
                                        Range = range
                                        Fixes = []
                                    })
                    | ParsedInput.SigFile(_) ->
                        // Signature files - no implementation to check
                        ()
                with
                | ex ->
                    // If AST parsing fails, add an error message
                    messages.Add({
                        Type = "Function Application Detector"
                        Message = $"Error analyzing file: {ex.Message}"
                        Code = "FA002"
                        Severity = Severity.Info
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }