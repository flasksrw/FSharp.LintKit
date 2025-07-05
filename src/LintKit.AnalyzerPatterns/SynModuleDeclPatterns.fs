namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// <summary>
/// Complete SynModuleDecl pattern matching reference with full type annotations
/// 
/// **AI Learning Reference**: This demonstrates how to:
/// - Handle ALL SynModuleDecl patterns with complete pattern matching
/// - Use consistent message creation for all module declarations
/// - Apply explicit type annotations for AI learning
/// 
/// **For AI Agents**: This is the definitive pattern for SynModuleDecl analysis.
/// Copy this approach when you need to analyze F# module-level declarations.
/// **For Humans**: Reference for building module declaration analyzers.
/// 
/// SynModuleDecl represents declarations within F# modules:
/// let bindings, type definitions, open statements, nested modules, etc.
/// </summary>
module SynModuleDeclPatterns =
    
    /// <summary>
    /// Creates an informational message for a module declaration visit
    /// </summary>
    /// <param name="nodeType">Type of module declaration visited</param>
    /// <param name="range">Location of the declaration</param>
    /// <param name="description">Description of what was found</param>
    let private createNodeVisitMessage (nodeType: string) (range: range) (description: string) : Message =
        {
            Type = "SynModuleDecl Pattern Analyzer"
            Message = $"Visited {nodeType}: {description}"
            Code = "MODULEDECL001"
            Severity = Severity.Info
            Range = range
            Fixes = []
        }
    
    /// <summary>
    /// Analyzes a SynModuleDecl with complete pattern matching
    /// **AI CRITICAL PATTERN**: This covers ALL SynModuleDecl cases - use this as your template
    /// </summary>
    /// <param name="decl">The F# module declaration syntax tree node to analyze</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        match decl with
        
        // === LET BINDINGS ===
        | SynModuleDecl.Let(isRecursive: bool, bindings: SynBinding list, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.Let" range $"let binding (recursive: {isRecursive}, count: {bindings.Length})"
            [nodeMsg]
        
        // === TYPE DEFINITIONS ===
        | SynModuleDecl.Types(typeDefns: SynTypeDefn list, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.Types" range $"type definitions (count: {typeDefns.Length})"
            [nodeMsg]
        
        // === EXCEPTION DEFINITIONS ===
        | SynModuleDecl.Exception(exnDefn: SynExceptionDefn, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.Exception" range "exception definition"
            [nodeMsg]
        
        // === OPEN STATEMENTS ===
        | SynModuleDecl.Open(target: SynOpenDeclTarget, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.Open" range "open statement"
            [nodeMsg]
        
        // === MODULE DECLARATIONS ===
        | SynModuleDecl.ModuleAbbrev(ident: Ident, longId: LongIdent, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.ModuleAbbrev" range $"module abbreviation: {ident.idText}"
            [nodeMsg]
        
        | SynModuleDecl.NestedModule(componentInfo: SynComponentInfo, isRecursive: bool, decls: SynModuleDecl list, isContinuing: bool, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.NestedModule" range $"nested module (recursive: {isRecursive}, declarations: {decls.Length})"
            [nodeMsg]
        
        // === ATTRIBUTES ===
        | SynModuleDecl.Attributes(attributes: SynAttributes, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.Attributes" range $"attributes (count: {attributes.Length})"
            [nodeMsg]
        
        // === HASH DIRECTIVES ===
        | SynModuleDecl.HashDirective(hashDirective: ParsedHashDirective, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.HashDirective" range "hash directive"
            [nodeMsg]
        
        // === NAMESPACE FRAGMENT ===
        | SynModuleDecl.NamespaceFragment(moduleOrNamespace: SynModuleOrNamespace) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.NamespaceFragment" moduleOrNamespace.Range "namespace fragment"
            [nodeMsg]
        
        // === STANDALONE EXPRESSIONS ===
        | SynModuleDecl.Expr(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynModuleDecl.Expr" range "standalone expression"
            [nodeMsg]
    
    /// <summary>
    /// Sample analyzer that uses the SynModuleDecl pattern matching
    /// This demonstrates how to integrate the pattern analysis into a complete analyzer
    /// **AI PATTERN**: Use this structure for your own module declaration analyzers
    /// </summary>
    [<CliAnalyzer>]
    let synModuleDeclPatternAnalyzer: Analyzer<CliContext> =
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
                                    let declMessages = analyzeModuleDeclaration decl
                                    messages.AddRange(declMessages)
                    | ParsedInput.SigFile(_) ->
                        ()
                        
                with
                | ex ->
                    messages.Add({
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = $"Error analyzing module declarations: {ex.Message}"
                        Code = "MODULEDECL999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }