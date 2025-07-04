namespace LintKit.Analyzers

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// <summary>
/// Severity level usage examples and guidelines for F# analyzers
/// 
/// This module demonstrates proper usage of different severity levels:
/// - Error: Critical issues that must be fixed (compilation breaks, runtime failures)
/// - Warning: Important issues that should be addressed (potential bugs, deprecated usage)
/// - Info: Suggestions for improvement (style, performance, maintainability)
/// - Hint: Subtle suggestions for enhancement (advanced optimizations, best practices)
/// 
/// Educational purpose for AI agents:
/// - When to use each severity level
/// - How different severities affect CI/CD pipelines
/// - Balancing between helpful guidance and noise reduction
/// - Examples of escalation patterns (Hint -> Info -> Warning -> Error)
/// 
/// Best practices:
/// - Use Error sparingly for actual breaking issues
/// - Use Warning for likely problems that need attention
/// - Use Info for style suggestions and optimizations
/// - Use Hint for advanced tips that won't distract novice developers
/// - Consider configurable severity levels for team preferences
/// </summary>
module SeverityGuide =
    
    /// <summary>
    /// Analyzes expressions for Error-level issues (critical problems)
    /// </summary>
    /// <param name="expr">The expression to analyze</param>
    /// <returns>List of Error-level messages</returns>
    let private analyzeForErrors (expr: SynExpr) : Message list =
        let messages = ResizeArray<Message>()
        
        match expr with
        // ERROR: Potential null reference access
        | SynExpr.App(_, _, SynExpr.LongIdent(_, SynLongIdent([ident], _, _), _, _), _, _) 
            when ident.idText = "failwith" ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Direct use of 'failwith' can cause unhandled exceptions. Consider using Result type or more specific exception handling."
                Code = "SGE001"
                Severity = Severity.Error
                Range = expr.Range
                Fixes = []
            })
        
        // ERROR: Potential division by zero
        | SynExpr.App(_, _, SynExpr.App(_, _, SynExpr.LongIdent(_, SynLongIdent([ident], _, _), _, _), _, _), SynExpr.Const(SynConst.Int32(0), _), _)
            when ident.idText = "/" ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Division by zero detected. This will cause a runtime exception."
                Code = "SGE002"
                Severity = Severity.Error
                Range = expr.Range
                Fixes = []
            })
        
        // ERROR: Use of deprecated unsafe functions
        | SynExpr.App(_, _, SynExpr.LongIdent(_, SynLongIdent(parts, _, _), _, _), _, _) ->
            let fullPath = parts |> List.map (fun i -> i.idText) |> String.concat "."
            if fullPath.Contains("System.Runtime.InteropServices.Marshal.AllocHGlobal") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "Direct memory allocation without proper disposal can cause memory leaks. Use 'use' binding or implement IDisposable."
                    Code = "SGE003"
                    Severity = Severity.Error
                    Range = expr.Range
                    Fixes = []
                })
        
        | _ -> ()
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes expressions for Warning-level issues (important problems)
    /// </summary>
    /// <param name="expr">The expression to analyze</param>
    /// <returns>List of Warning-level messages</returns>
    let private analyzeForWarnings (expr: SynExpr) : Message list =
        let messages = ResizeArray<Message>()
        
        match expr with
        // WARNING: Potential performance issue
        | SynExpr.App(_, _, SynExpr.LongIdent(_, SynLongIdent(parts, _, _), _, _), _, _) ->
            let fullPath = parts |> List.map (fun i -> i.idText) |> String.concat "."
            if fullPath.EndsWith("List.append") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "List.append can be inefficient for large lists. Consider using List.concat or @ operator for better performance."
                    Code = "SGW001"
                    Severity = Severity.Warning
                    Range = expr.Range
                    Fixes = []
                })
            elif fullPath.Contains("Console.WriteLine") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "Console.WriteLine in production code may not be intended. Consider using proper logging framework."
                    Code = "SGW002"
                    Severity = Severity.Warning
                    Range = expr.Range
                    Fixes = []
                })
        
        // WARNING: Nested if-then-else chains
        | SynExpr.IfThenElse(_, _, Some(SynExpr.IfThenElse(_, _, Some(SynExpr.IfThenElse(_, _, _, _, _, _, _)), _, _, _, _)), _, _, _, _) ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Deeply nested if-then-else statements detected. Consider using pattern matching for better readability."
                Code = "SGW003"
                Severity = Severity.Warning
                Range = expr.Range
                Fixes = []
            })
        
        // WARNING: Large constant values that might be typos
        | SynExpr.Const(SynConst.Int32(value), _) when value > 1000000 ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = $"Large constant value {value} detected. Verify this is not a typo and consider using named constants."
                Code = "SGW004"
                Severity = Severity.Warning
                Range = expr.Range
                Fixes = []
            })
        
        // WARNING: String concatenation in loops (potential performance issue)
        | SynExpr.App(_, _, SynExpr.App(_, _, SynExpr.LongIdent(_, SynLongIdent([ident], _, _), _, _), _, _), _, _)
            when ident.idText = "+" ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "String concatenation with + operator. For multiple concatenations, consider StringBuilder or String.concat."
                Code = "SGW005"
                Severity = Severity.Warning
                Range = expr.Range
                Fixes = []
            })
        
        | _ -> ()
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes expressions for Info-level suggestions (style and optimization)
    /// </summary>
    /// <param name="expr">The expression to analyze</param>
    /// <returns>List of Info-level messages</returns>
    let private analyzeForInfo (expr: SynExpr) : Message list =
        let messages = ResizeArray<Message>()
        
        match expr with
        // INFO: Style suggestions
        | SynExpr.App(_, _, SynExpr.LongIdent(_, SynLongIdent(parts, _, _), _, _), _, _) ->
            let fullPath = parts |> List.map (fun i -> i.idText) |> String.concat "."
            
            if fullPath.EndsWith("List.map") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "Consider using List.map with function composition for cleaner functional style."
                    Code = "SGI001"
                    Severity = Severity.Info
                    Range = expr.Range
                    Fixes = []
                })
            elif fullPath.Contains("Array.") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "Array operations detected. Consider if List or Seq would be more appropriate for functional programming style."
                    Code = "SGI002"
                    Severity = Severity.Info
                    Range = expr.Range
                    Fixes = []
                })
        
        // INFO: Lambda expression style suggestions
        | SynExpr.Lambda(_, _, _, SynExpr.App(_, _, _, SynExpr.Ident(ident), _), _, _, _)
            when ident.idText.Length = 1 ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Simple lambda expression detected. Consider using partial application or function composition."
                Code = "SGI003"
                Severity = Severity.Info
                Range = expr.Range
                Fixes = []
            })
        
        // INFO: Parentheses usage suggestions
        | SynExpr.Paren(SynExpr.Const(_, _), _, _, _) ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Unnecessary parentheses around constant value. Consider removing for cleaner code."
                Code = "SGI004"
                Severity = Severity.Info
                Range = expr.Range
                Fixes = []
            })
        
        // INFO: Tuple vs record suggestions
        | SynExpr.Tuple(false, exprs, _, _) when exprs.Length > 3 ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = $"Large tuple with {exprs.Length} elements. Consider using a record type for better readability and maintainability."
                Code = "SGI005"
                Severity = Severity.Info
                Range = expr.Range
                Fixes = []
            })
        
        // INFO: Function naming suggestions
        | SynExpr.LetOrUse(_, _, bindings, _, _, _) ->
            for binding in bindings do
                match binding with
                | SynBinding(_, _, _, _, _, _, _, SynPat.Named(SynIdent(ident, _), _, _, _), _, _, _, _, _) ->
                    if ident.idText.Length > 25 then
                        messages.Add({
                            Type = "Severity Guide Analyzer"
                            Message = $"Function name '{ident.idText}' is very long. Consider shortening for better readability."
                            Code = "SGI006"
                            Severity = Severity.Info
                            Range = ident.idRange
                            Fixes = []
                        })
                | _ -> ()
        
        | _ -> ()
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes expressions for Hint-level suggestions (subtle enhancements)
    /// </summary>
    /// <param name="expr">The expression to analyze</param>
    /// <returns>List of Hint-level messages</returns>
    let private analyzeForHints (expr: SynExpr) : Message list =
        let messages = ResizeArray<Message>()
        
        match expr with
        // HINT: Advanced functional programming patterns
        | SynExpr.App(_, _, SynExpr.LongIdent(_, SynLongIdent(parts, _, _), _, _), _, _) ->
            let fullPath = parts |> List.map (fun i -> i.idText) |> String.concat "."
            
            if fullPath.EndsWith("List.fold") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "Consider if List.reduce or List.scan would be more semantically appropriate than List.fold."
                    Code = "SGH001"
                    Severity = Severity.Hint
                    Range = expr.Range
                    Fixes = []
                })
            elif fullPath.Contains("List.map") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "For performance-critical code, consider using Array.map or Seq.map depending on usage patterns."
                    Code = "SGH002"
                    Severity = Severity.Hint
                    Range = expr.Range
                    Fixes = []
                })
        
        // HINT: Advanced pattern matching suggestions
        | SynExpr.Match(_, _, clauses, _, _) when clauses.Length = 2 ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Two-clause match expression could potentially be simplified to if-then-else for readability."
                Code = "SGH003"
                Severity = Severity.Hint
                Range = expr.Range
                Fixes = []
            })
        
        // HINT: Function composition opportunities
        | SynExpr.App(_, _, SynExpr.App(_, _, _, _, _), _, _) ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Consider function composition (>>) or piping (|>) for cleaner functional style."
                Code = "SGH004"
                Severity = Severity.Hint
                Range = expr.Range
                Fixes = []
            })
        
        // HINT: Type annotation opportunities for learning
        | SynExpr.Lambda(_, _, _, _, _, _, _) ->
            messages.Add({
                Type = "Severity Guide Analyzer"
                Message = "Advanced: Consider explicit type annotations on lambdas for better IDE support and documentation."
                Code = "SGH005"
                Severity = Severity.Hint
                Range = expr.Range
                Fixes = []
            })
        
        | _ -> ()
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes module declarations for severity-based issues
    /// </summary>
    /// <param name="decl">The module declaration to analyze</param>
    /// <returns>List of analysis messages with appropriate severity levels</returns>
    let private analyzeModuleDeclarationSeverity (decl: SynModuleDecl) : Message list =
        match decl with
        // ERROR: Module-level issues that break compilation
        | SynModuleDecl.Open(target, range) ->
            let messages = ResizeArray<Message>()
            
            let openPath = 
                match target with
                | SynOpenDeclTarget.ModuleOrNamespace(SynLongIdent(longId, _, _), _) ->
                    longId |> List.map (fun ident -> ident.idText) |> String.concat "."
                | _ -> "unknown"
            
            // ERROR: Opening potentially dangerous namespaces
            if openPath.Contains("System.Runtime.InteropServices") then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "Opening System.Runtime.InteropServices namespace introduces unsafe operations. Ensure proper error handling and memory management."
                    Code = "SGE004"
                    Severity = Severity.Error
                    Range = range
                    Fixes = []
                })
            // WARNING: Opening broad namespaces
            elif openPath = "System" then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = "Opening entire System namespace can cause naming conflicts. Consider opening specific submodules."
                    Code = "SGW006"
                    Severity = Severity.Warning
                    Range = range
                    Fixes = []
                })
            // INFO: Style suggestions for opens
            elif openPath.Split('.').Length = 1 && openPath.Length > 15 then
                messages.Add({
                    Type = "Severity Guide Analyzer"
                    Message = $"Long namespace name '{openPath}'. Consider using module abbreviation for readability."
                    Code = "SGI007"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            messages |> Seq.toList
        
        // WARNING: Large type definitions
        | SynModuleDecl.Types(typeDefns, range) ->
            let messages = ResizeArray<Message>()
            
            for typeDefn in typeDefns do
                match typeDefn with
                | SynTypeDefn(SynComponentInfo(_, _, _, longId, _, _, _, _), repr, members, _, _, _) ->
                    let typeName = longId |> List.map (fun i -> i.idText) |> String.concat "."
                    
                    // WARNING: Very large types
                    if members.Length > 20 then
                        messages.Add({
                            Type = "Severity Guide Analyzer"
                            Message = $"Type '{typeName}' has many members ({members.Length}). Consider breaking into smaller, more focused types."
                            Code = "SGW007"
                            Severity = Severity.Warning
                            Range = range
                            Fixes = []
                        })
                    // INFO: Moderate-sized types
                    elif members.Length > 10 then
                        messages.Add({
                            Type = "Severity Guide Analyzer"
                            Message = $"Type '{typeName}' is growing large ({members.Length} members). Consider if it has too many responsibilities."
                            Code = "SGI008"
                            Severity = Severity.Info
                            Range = range
                            Fixes = []
                        })
            
            messages |> Seq.toList
        
        // Process expressions within declarations
        | SynModuleDecl.Let(_, bindings, _) ->
            bindings
            |> List.collect (fun binding ->
                match binding with
                | SynBinding(_, _, _, _, _, _, _, _, _, expr, _, _, _) ->
                    let errorMessages = analyzeForErrors expr
                    let warningMessages = analyzeForWarnings expr
                    let infoMessages = analyzeForInfo expr
                    let hintMessages = analyzeForHints expr
                    errorMessages @ warningMessages @ infoMessages @ hintMessages
            )
        
        | SynModuleDecl.Expr(expr, _) ->
            let errorMessages = analyzeForErrors expr
            let warningMessages = analyzeForWarnings expr
            let infoMessages = analyzeForInfo expr
            let hintMessages = analyzeForHints expr
            errorMessages @ warningMessages @ infoMessages @ hintMessages
        
        | _ -> []
    
    /// <summary>
    /// Demonstrates severity escalation patterns
    /// </summary>
    /// <param name="context">The analysis context</param>
    /// <returns>Messages showing escalation from Hint to Info to Warning to Error</returns>
    let private demonstrateSeverityEscalation (context: CliContext) : Message list =
        let messages = ResizeArray<Message>()
        
        // Example: Function complexity escalation
        // This would be based on actual analysis, but shown as example
        messages.Add({
            Type = "Severity Guide Analyzer"
            Message = "Example Hint: Function could benefit from extracted helper functions for better composition."
            Code = "SGH006"
            Severity = Severity.Hint
            Range = Range.Zero
            Fixes = []
        })
        
        messages.Add({
            Type = "Severity Guide Analyzer"
            Message = "Example Info: Function has moderate complexity (5-10 branches). Consider refactoring for clarity."
            Code = "SGI009"
            Severity = Severity.Info
            Range = Range.Zero
            Fixes = []
        })
        
        messages.Add({
            Type = "Severity Guide Analyzer"
            Message = "Example Warning: Function has high complexity (10-20 branches). Refactoring recommended for maintainability."
            Code = "SGW008"
            Severity = Severity.Warning
            Range = Range.Zero
            Fixes = []
        })
        
        messages.Add({
            Type = "Severity Guide Analyzer"
            Message = "Example Error: Function has extreme complexity (>20 branches). This violates team coding standards."
            Code = "SGE005"
            Severity = Severity.Error
            Range = Range.Zero
            Fixes = []
        })
        
        messages |> Seq.toList
    
    /// <summary>
    /// Main severity guide analyzer demonstrating proper severity usage
    /// </summary>
    [<CliAnalyzer>]
    let severityGuideAnalyzer: Analyzer<CliContext> =
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
                                // Analyze all declarations for severity examples
                                let declMessages = decls |> List.collect analyzeModuleDeclarationSeverity
                                messages.AddRange declMessages
                        
                        // Add escalation examples
                        let escalationMessages = demonstrateSeverityEscalation context
                        messages.AddRange escalationMessages
                    
                    | ParsedInput.SigFile(_) ->
                        // Signature files could have their own severity analysis
                        messages.Add({
                            Type = "Severity Guide Analyzer"
                            Message = "Info: Signature file detected. Consider if all exposed types need documentation."
                            Code = "SGI010"
                            Severity = Severity.Info
                            Range = Range.Zero
                            Fixes = []
                        })
                
                with
                | ex ->
                    messages.Add({
                        Type = "Severity Guide Analyzer"
                        Message = $"Error analyzing severity patterns: {ex.Message}"
                        Code = "SGE999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }