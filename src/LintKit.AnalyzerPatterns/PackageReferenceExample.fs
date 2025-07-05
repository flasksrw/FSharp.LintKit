namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// <summary>
/// Example analyzer demonstrating how to handle external package references in test code
/// 
/// **AI Learning Reference**: This shows how to use mkOptionsFromProject with package references
/// when test code contains external dependencies like xUnit attributes or custom libraries.
/// 
/// **For AI Agents**: Use this pattern when test code references external packages
/// **For Humans**: Reference for testing analyzers with package dependencies
/// </summary>
module PackageReferenceExample =
    
    /// <summary>
    /// Simple analyzer that detects usage of Assert.True in code
    /// This is used to demonstrate testing with xUnit package references
    /// </summary>
    [<CliAnalyzer>]
    let packageReferenceAnalyzer: Analyzer<CliContext> =
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
                                for decl in decls do
                                    match decl with
                                    | SynModuleDecl.Let(_, bindings, _) ->
                                        for binding in bindings do
                                            match binding with
                                            | SynBinding(_, _, _, _, _, _, _, _, _, expr, _, _, _) ->
                                                let rec findAssertTrue expr =
                                                    match expr with
                                                    | SynExpr.App(_, _, funcExpr, argExpr, _) ->
                                                        match funcExpr with
                                                        | SynExpr.LongIdent(_, SynLongIdent([ident1; ident2], _, _), _, _) 
                                                            when ident1.idText = "Assert" && ident2.idText = "True" ->
                                                            messages.Add({
                                                                Type = "Package Reference Example"
                                                                Message = "Found Assert.True usage - this requires xUnit package reference"
                                                                Code = "PKGREF001"
                                                                Severity = Severity.Info
                                                                Range = funcExpr.Range
                                                                Fixes = []
                                                            })
                                                        | _ -> ()
                                                        findAssertTrue funcExpr
                                                        findAssertTrue argExpr
                                                    | SynExpr.LetOrUse(_, _, bindings, body, _, _) ->
                                                        for binding in bindings do
                                                            match binding with
                                                            | SynBinding(_, _, _, _, _, _, _, _, _, bindExpr, _, _, _) ->
                                                                findAssertTrue bindExpr
                                                        findAssertTrue body
                                                    | _ -> ()
                                                
                                                findAssertTrue expr
                                    | _ -> ()
                    
                    | ParsedInput.SigFile(_) ->
                        ()
                
                with
                | ex ->
                    messages.Add({
                        Type = "Package Reference Example"
                        Message = $"Error during analysis: {ex.Message}"
                        Code = "PKGREF999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }