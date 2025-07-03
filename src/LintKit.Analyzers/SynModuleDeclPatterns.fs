namespace LintKit.Analyzers

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text

/// <summary>
/// Complete SynModuleDecl pattern matching reference for AI agents
/// 
/// This module demonstrates how to handle SynModuleDecl cases with:
/// - Detailed type annotations for every parameter
/// - Explicit pattern matching for AI learning
/// - Proper analysis of module-level declarations
/// - Appropriate message creation with different severities
/// 
/// Learning points for AI agents:
/// - How to destructure each SynModuleDecl variant
/// - When to recurse into nested declarations
/// - How to extract meaningful information from module declarations
/// - Proper range and location handling
/// 
/// SynModuleDecl represents declarations within F# modules, such as:
/// - let bindings, type definitions, open statements
/// - nested modules, attributes, hash directives
/// 
/// IMPORTANT: SynModuleDecl also has a Range member (decl.Range) that provides
/// unified access to position information across all patterns. This is different
/// from SynExpr where each pattern has its own range parameter.
/// 
/// This covers all 11 SynModuleDecl patterns found in typical F# modules.
/// </summary>
module SynModuleDeclPatterns =
    
    /// <summary>
    /// Analyzes a SynModuleDecl and returns any issues found
    /// </summary>
    /// <param name="decl">The F# module declaration syntax tree node to analyze</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        match decl with
        
        // === LET BINDINGS ===
        | SynModuleDecl.Let(isRecursive: bool,
                           bindings: SynBinding list,
                           range: range) ->
            // Handle let bindings at module level: let x = value, let rec f x = ...
            // Type annotations:
            // - isRecursive: true for 'let rec' bindings
            // - bindings: list of variable/function bindings
            // - range: location of the entire let declaration
            
            let bindingMessages = 
                bindings
                |> List.collect (fun binding ->
                    match binding with
                    | SynBinding(_, _, _, _, _, _, _, pat, _, expr, _, _, _) ->
                        // Could analyze the pattern and expression here
                        // For now, just return empty list
                        [])
            
            // Example analysis: detect very long functions
            let longFunctionMessage: Message option =
                if bindings.Length = 1 then
                    // Simple heuristic for function length
                    let bindingRange = bindings.Head.RangeOfBindingWithRhs
                    let lineCount = bindingRange.EndLine - bindingRange.StartLine + 1
                    if lineCount > 50 then
                        Some {
                            Type = "SynModuleDecl Pattern Analyzer"
                            Message = $"Function definition is very long ({lineCount} lines). Consider breaking it into smaller functions."
                            Code = "SMD001"
                            Severity = Severity.Info
                            Range = range
                            Fixes = []
                        }
                    else
                        None
                else
                    None
            
            // Example analysis: suggest using let rec for mutually recursive functions
            let recursiveMessage: Message option =
                if not isRecursive && bindings.Length > 1 then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "Multiple bindings in single let - consider if 'let rec' is needed for mutual recursion"
                        Code = "SMD002"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [longFunctionMessage; recursiveMessage]
                |> List.choose id
            
            bindingMessages @ analysisMessages
        
        // === TYPE DEFINITIONS ===
        | SynModuleDecl.Types(typeDefns: SynTypeDefn list,
                             range: range) ->
            // Handle type definitions: type Person = { Name: string }, type Color = Red | Blue
            // Type annotations:
            // - typeDefns: list of type definitions in this declaration
            // - range: location of the entire type declaration
            
            // Example analysis: detect large discriminated unions
            let largeUnionMessage: Message option =
                let unionCaseCounts = 
                    typeDefns
                    |> List.map (fun typeDefn ->
                        match typeDefn with
                        | SynTypeDefn(_, repr, _, _, _, _) ->
                            match repr with
                            | SynTypeDefnRepr.Simple(SynTypeDefnSimpleRepr.Union(_, cases, _), _) ->
                                cases.Length
                            | _ -> 0)
                    |> List.filter (fun count -> count > 0)
                
                match unionCaseCounts with
                | [] -> None
                | counts when List.max counts > 15 ->
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = $"Discriminated union has many cases ({List.max counts}). Consider breaking into smaller unions."
                        Code = "SMD003"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Example analysis: suggest documentation for public types
            let documentationMessage: Message option =
                if typeDefns.Length > 0 then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "Consider adding XML documentation for public types"
                        Code = "SMD004"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [largeUnionMessage; documentationMessage]
                |> List.choose id
            
            analysisMessages
        
        // === OPEN STATEMENTS ===
        | SynModuleDecl.Open(target: SynOpenDeclTarget,
                            range: range) ->
            // Handle open statements: open System, open System.Collections.Generic
            // Type annotations:
            // - target: the module/namespace being opened
            // - range: location of the open statement
            
            let openPath = 
                match target with
                | SynOpenDeclTarget.ModuleOrNamespace(SynLongIdent(longId, _, _), _) ->
                    longId |> List.map (fun ident -> ident.idText) |> String.concat "."
                | _ -> "unknown"
            
            // Example analysis: detect potentially problematic opens
            let problematicOpenMessage: Message option =
                if openPath = "System.IO" then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "Opening System.IO namespace - prefer explicit qualification for security-sensitive operations"
                        Code = "SMD005"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                elif openPath.Contains("Internal") then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "Opening internal namespace - this may break in future versions"
                        Code = "SMD006"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: detect redundant opens
            let redundantOpenMessage: Message option =
                if openPath = "System" then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "Consider if 'open System' is necessary - many System types are available by default"
                        Code = "SMD007"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [problematicOpenMessage; redundantOpenMessage]
                |> List.choose id
            
            analysisMessages
        
        // === NESTED MODULES ===
        | SynModuleDecl.NestedModule(moduleInfo: SynComponentInfo,
                                    isRecursive: bool,
                                    decls: SynModuleDecl list,
                                    isContinuing: bool,
                                    range: range,
                                    trivia: SynModuleDeclNestedModuleTrivia) ->
            // Handle nested modules: module InnerModule = ...
            // Type annotations:
            // - componentInfo: module name and attributes
            // - isRecursive: true if module is recursive
            // - decls: declarations within the nested module
            // - isContinuing: continuation information
            // - range: location of the nested module
            // - trivia: additional syntax information
            
            // Recursively analyze nested declarations
            let nestedMessages = 
                decls
                |> List.collect analyzeModuleDeclaration
            
            // Example analysis: detect deeply nested modules
            let nestingMessage: Message option =
                let rec countNesting (declarations: SynModuleDecl list) : int =
                    declarations
                    |> List.map (function
                        | SynModuleDecl.NestedModule(_, _, innerDecls, _, _, _) ->
                            1 + (countNesting innerDecls)
                        | _ -> 0)
                    |> List.fold max 0
                
                let maxNesting = countNesting decls
                if maxNesting > 3 then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = $"Deeply nested modules (depth {maxNesting}). Consider flattening the structure."
                        Code = "SMD008"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest module organization
            let organizationMessage: Message option =
                if decls.Length > 20 then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = $"Large nested module ({decls.Length} declarations). Consider splitting into separate files."
                        Code = "SMD009"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [nestingMessage; organizationMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === MODULE ABBREVIATIONS ===
        | SynModuleDecl.ModuleAbbrev(ident: Ident,
                                    longId: LongIdent,
                                    range: range) ->
            // Handle module abbreviations: module M = Some.Long.Module.Name
            // Type annotations:
            // - ident: the abbreviation name
            // - longId: the full module path being abbreviated
            // - range: location of the abbreviation
            
            let abbreviation = ident.idText
            let fullPath = 
                longId |> List.map (fun i -> i.idText) |> String.concat "."
            
            // Example analysis: suggest meaningful abbreviation names
            let namingMessage: Message option =
                if abbreviation.Length <= 2 then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = $"Module abbreviation '{abbreviation}' is very short. Consider a more descriptive name."
                        Code = "SMD010"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: detect unnecessary abbreviations
            let unnecessaryMessage: Message option =
                let pathParts = fullPath.Split('.')
                if pathParts.Length <= 2 then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = $"Module abbreviation may be unnecessary for short path '{fullPath}'"
                        Code = "SMD011"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [namingMessage; unnecessaryMessage]
                |> List.choose id
            
            analysisMessages
        
        // === ATTRIBUTES ===
        | SynModuleDecl.Attributes(attributes: SynAttributes,
                                  range: range) ->
            // Handle standalone attributes: [<assembly: SomeAttribute>]
            // Type annotations:
            // - attributes: list of attributes in this declaration
            // - range: location of the attribute declaration
            
            // Example analysis: detect deprecated attributes
            let deprecatedMessage: Message option =
                // Simplified attribute analysis - just check if any attributes exist
                let hasDeprecated = attributes.Length > 0
                
                if hasDeprecated then
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "Consider providing migration guidance when using deprecation attributes"
                        Code = "SMD012"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [deprecatedMessage]
                |> List.choose id
            
            analysisMessages
        
        // === EXPRESSIONS ===
        | SynModuleDecl.Expr(expr: SynExpr,
                            range: range) ->
            // Handle standalone expressions at module level: printfn "Hello"
            // Type annotations:
            // - expr: the expression being executed
            // - range: location of the expression
            
            // Example analysis: detect side effects at module level
            let sideEffectMessage: Message option =
                match expr with
                | SynExpr.App(_, _, _, _, _) ->
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "Module-level expression detected. Consider moving to a main function for better testability."
                        Code = "SMD013"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [sideEffectMessage]
                |> List.choose id
            
            analysisMessages
        
        // === EXCEPTION DEFINITIONS ===
        | SynModuleDecl.Exception(exnDefn: SynExceptionDefn,
                                 range: range) ->
            // Handle exception definitions: exception MyException of string
            // Type annotations:
            // - exnDefn: the exception definition
            // - range: location of the exception definition
            
            // Example analysis: suggest documentation for custom exceptions
            let exceptionDocumentationMessage: Message option =
                Some {
                    Type = "SynModuleDecl Pattern Analyzer"
                    Message = "Consider adding XML documentation for custom exceptions to explain when they are thrown"
                    Code = "SMD015"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            // Combine messages
            let analysisMessages = 
                [exceptionDocumentationMessage]
                |> List.choose id
            
            analysisMessages
        
        // === NAMESPACE FRAGMENTS ===
        | SynModuleDecl.NamespaceFragment(fragment: SynModuleOrNamespace) ->
            // Handle namespace fragments (less common pattern)
            // Type annotations:
            // - fragment: the namespace or module fragment
            
            // Recursively analyze declarations within the namespace fragment
            let fragmentMessages = 
                match fragment with
                | SynModuleOrNamespace(_, _, _, decls, _, _, _, _, _) ->
                    decls |> List.collect analyzeModuleDeclaration
            
            // Example analysis: detect deeply nested namespaces
            let fragmentMessage: Message option =
                Some {
                    Type = "SynModuleDecl Pattern Analyzer"
                    Message = "Namespace fragment detected - consider if this organization is necessary"
                    Code = "SMD016"
                    Severity = Severity.Info
                    Range = fragment.Range
                    Fixes = []
                }
            
            let analysisMessages = 
                [fragmentMessage]
                |> List.choose id
            
            fragmentMessages @ analysisMessages
        
        // === HASH DIRECTIVES ===
        | SynModuleDecl.HashDirective(hashDirective: ParsedHashDirective,
                                     range: range) ->
            // Handle hash directives: #load "file.fsx", #r "assembly.dll"
            // Type annotations:
            // - hashDirective: the parsed directive
            // - range: location of the directive
            
            // Example analysis: detect script-specific directives in compiled code
            let scriptDirectiveMessage: Message option =
                match hashDirective with
                | ParsedHashDirective("load", _, _) ->
                    Some {
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = "#load directive found - this is typically for F# scripts, not compiled code"
                        Code = "SMD014"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [scriptDirectiveMessage]
                |> List.choose id
            
            analysisMessages
        
        // === PATTERN MATCHING COMPLETE ===
        // All 10 SynModuleDecl pattern cases have been implemented above:
        // 1. Let, 2. Types, 3. Open, 4. NestedModule, 5. ModuleAbbrev
        // 6. Attributes, 7. Expr, 8. HashDirective, 9. Exception, 10. NamespaceFragment
        // + Range (member property, not a pattern case)
        // 
        // Pattern matching is exhaustive - no wildcard case needed.
    
    /// <summary>
    /// Sample analyzer that uses the SynModuleDecl pattern matching
    /// This demonstrates how to integrate the pattern analysis into a complete analyzer
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
                        // Signature files have different structure
                        ()
                        
                with
                | ex ->
                    messages.Add({
                        Type = "SynModuleDecl Pattern Analyzer"
                        Message = $"Error analyzing module declarations: {ex.Message}"
                        Code = "SMD999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }