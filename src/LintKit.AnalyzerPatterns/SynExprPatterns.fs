namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text

/// <summary>
/// Complete SynExpr pattern matching reference for AI agents
/// 
/// This module demonstrates how to handle ALL SynExpr cases with:
/// - Detailed type annotations for every parameter
/// - Explicit pattern matching for AI learning
/// - Proper recursion into sub-expressions
/// - Appropriate message creation with different severities
/// 
/// Learning points for AI agents:
/// - How to destructure each SynExpr variant
/// - When to recurse into sub-expressions
/// - How to extract meaningful information from AST nodes
/// - Proper range and location handling
/// 
/// Reference documentation:
/// https://fsharp.github.io/fsharp-compiler-docs/reference/fsharp-compiler-syntax-synexpr.html
/// 
/// This reference covers 15 most commonly used SynExpr patterns out of 60+ total patterns.
/// For complete coverage, refer to the official documentation above.
/// </summary>
module SynExprPatterns =
    
    /// <summary>
    /// Analyzes a SynExpr and returns any issues found
    /// Currently implements basic patterns - more will be added incrementally
    /// </summary>
    /// <param name="expr">The F# expression syntax tree node to analyze</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzeExpression (expr: SynExpr) : Message list =
        match expr with
        
        // === BASIC LITERAL VALUES ===
        | SynExpr.Const(constant: SynConst, range: range) ->
            // Handle constant literals (numbers, strings, etc.)
            // Type annotations: constant contains the actual value, range contains location
            match constant with
            | SynConst.String(text: string, stringKind: SynStringKind, range: range) ->
                // Example: detect hardcoded sensitive strings
                if text.Contains("password") || text.Contains("secret") then
                    [{
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Potential hardcoded sensitive data in string literal: '{text}'"
                        Code = "SEP001"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }]
                else
                    []
            | SynConst.Int32(value: int32) ->
                // Example: detect magic numbers
                if value > 100 && value <> 200 && value <> 404 && value <> 500 then
                    [{
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Consider extracting magic number {value} to a named constant"
                        Code = "SEP002"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }]
                else
                    []
            | _ ->
                // Other constant types: Int64, Float, Char, Bool, etc.
                // No specific analysis for now
                []
        
        // === IDENTIFIERS ===
        | SynExpr.Ident(identifier: Ident) ->
            // Handle simple identifiers (variable names, function names)
            // Type annotation: identifier contains the name and range
            let identName: string = identifier.idText
            let identRange: range = identifier.idRange
            
            // Example: detect potential typos in common names
            if identName = "lenght" then
                [{
                    Type = "SynExpr Pattern Analyzer"
                    Message = "Possible typo: 'lenght' should be 'length'"
                    Code = "SEP003"
                    Severity = Severity.Warning
                    Range = identRange
                    Fixes = []
                }]
            else
                []
        
        // === FUNCTION APPLICATION ===
        | SynExpr.App(exprAtomicFlag: ExprAtomicFlag, 
                      isInfix: bool, 
                      funcExpr: SynExpr, 
                      argExpr: SynExpr, 
                      range: range) ->
            // Handle function applications: f(x), x |> f, etc.
            // Type annotations:
            // - exprAtomicFlag: indicates if parentheses are needed
            // - isInfix: true for infix operators like x + y
            // - funcExpr: the function being called
            // - argExpr: the argument being passed
            // - range: location of the entire application
            
            // Recurse into both the function and argument expressions
            let funcMessages: Message list = analyzeExpression funcExpr
            let argMessages: Message list = analyzeExpression argExpr
            
            // Combine results from sub-expressions
            funcMessages @ argMessages
        
        // === LAMBDA EXPRESSIONS ===
        | SynExpr.Lambda(fromMethod: bool,
                        inLambdaSeq: bool,
                        args: SynSimplePats,
                        body: SynExpr,
                        parsedData: (SynPat list * SynExpr) option,
                        range: range,
                        trivia: SynExprLambdaTrivia) ->
            // Handle lambda expressions: fun x -> x + 1
            // Type annotations:
            // - fromMethod: true if lambda originates from a method
            // - inLambdaSeq: true if this is part of an iterated sequence of lambdas
            // - args: the lambda parameters (patterns)
            // - body: the lambda body expression
            // - parsedData: original parsed patterns and expression before transformation
            // - range: location of the entire lambda
            // - trivia: additional syntax information
            
            let bodyMessages: Message list = analyzeExpression body
            
            // Example analysis: detect overly complex lambda expressions
            let lambdaComplexityMessage: Message option =
                // Simple heuristic: count nested function applications in body
                let rec countApplications (expr: SynExpr) : int =
                    match expr with
                    | SynExpr.App(_, _, funcExpr, argExpr, _) ->
                        1 + (countApplications funcExpr) + (countApplications argExpr)
                    | _ -> 0
                
                let appCount = countApplications body
                if appCount > 5 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Lambda expression has high complexity ({appCount} function applications). Consider extracting to a named function."
                        Code = "SEP004"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            match lambdaComplexityMessage with
            | Some msg -> msg :: bodyMessages
            | None -> bodyMessages
        
        // === LET OR USE BINDINGS ===
        | SynExpr.LetOrUse(isRecursive: bool,
                          isUse: bool,
                          bindings: SynBinding list,
                          body: SynExpr,
                          range: range,
                          trivia: SynExprLetOrUseTrivia) ->
            // Handle let/use expressions: let x = 1 in x + 2
            // Type annotations:
            // - isRecursive: true for 'let rec'
            // - isUse: true for 'use' (automatic disposal)
            // - bindings: the variable bindings
            // - body: the expression where bindings are in scope
            // - range: location of the entire let/use expression
            // - trivia: additional syntax information
            
            let bodyMessages: Message list = analyzeExpression body
            let bindingMessages: Message list = 
                bindings
                |> List.collect (fun binding ->
                    match binding with
                    | SynBinding(_, _, _, _, _, _, _, _, _, expr, _, _, _) ->
                        analyzeExpression expr)
            
            // Example analysis: warn about unused 'use' bindings
            let useAnalysisMessages: Message list =
                if isUse then
                    // Simple check: if use binding but body doesn't seem to use disposable resources
                    [{
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Consider whether 'use' binding is necessary for automatic disposal"
                        Code = "SEP005"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }]
                else
                    []
            
            // Combine all messages
            bodyMessages @ bindingMessages @ useAnalysisMessages
        
        // === CONDITIONAL EXPRESSIONS ===
        | SynExpr.IfThenElse(ifExpr: SynExpr,
                            thenExpr: SynExpr,
                            elseExpr: SynExpr option,
                            spIfToThen: DebugPointAtBinding,
                            isFromErrorRecovery: bool,
                            range: range,
                            trivia: SynExprIfThenElseTrivia) ->
            // Handle if-then-else expressions: if condition then value1 else value2
            // Type annotations:
            // - ifExpr: the condition expression
            // - thenExpr: expression executed when condition is true
            // - elseExpr: expression executed when condition is false (None for if-then without else)
            // - spIfToThen: debug point information
            // - isFromErrorRecovery: true if inserted during error recovery
            // - range: location of the entire conditional
            // - trivia: additional syntax information
            
            let ifMessages: Message list = analyzeExpression ifExpr
            let thenMessages: Message list = analyzeExpression thenExpr
            let elseMessages: Message list = 
                match elseExpr with
                | Some expr -> analyzeExpression expr
                | None -> []
            
            // Example analysis: detect constant conditions
            let constantConditionMessage: Message option =
                match ifExpr with
                | SynExpr.Const(SynConst.Bool(value), _) ->
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Conditional expression has constant condition ({value}). Consider simplifying."
                        Code = "SEP006"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Example analysis: warn about missing else clause in certain contexts
            let missingElseMessage: Message option =
                if elseExpr.IsNone then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Consider adding an 'else' clause for completeness"
                        Code = "SEP007"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine all messages
            let analysisMessages = 
                [constantConditionMessage; missingElseMessage]
                |> List.choose id
            
            ifMessages @ thenMessages @ elseMessages @ analysisMessages
        
        // === PATTERN MATCHING ===
        | SynExpr.Match(matchDebugPoint: DebugPointAtBinding,
                       expr: SynExpr,
                       clauses: SynMatchClause list,
                       range: range,
                       trivia: SynExprMatchTrivia) ->
            // Handle pattern matching: match expr with | pattern1 -> expr1 | pattern2 -> expr2
            // Type annotations:
            // - matchDebugPoint: debug point information
            // - expr: the expression being matched
            // - clauses: list of pattern match clauses
            // - range: location of the entire match expression
            // - trivia: additional syntax information
            
            let exprMessages: Message list = analyzeExpression expr
            let clauseMessages: Message list = 
                clauses
                |> List.collect (fun clause ->
                    match clause with
                    | SynMatchClause(pat, whenExpr, resultExpr, range, debugPoint, trivia) ->
                        let resultMessages = analyzeExpression resultExpr
                        let whenMessages = 
                            match whenExpr with
                            | Some whenExpr -> analyzeExpression whenExpr
                            | None -> []
                        resultMessages @ whenMessages)
            
            // Example analysis: detect match expressions with too many clauses
            let complexityMessage: Message option =
                if clauses.Length > 10 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Match expression has many clauses ({clauses.Length}). Consider refactoring for readability."
                        Code = "SEP008"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: detect single-clause matches (could use let instead)
            let singleClauseMessage: Message option =
                if clauses.Length = 1 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Single-clause match could potentially be simplified to a let binding"
                        Code = "SEP009"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine all messages
            let analysisMessages = 
                [complexityMessage; singleClauseMessage]
                |> List.choose id
            
            exprMessages @ clauseMessages @ analysisMessages
        
        // === RECORD CONSTRUCTION ===
        | SynExpr.Record(baseInfo: (SynType * SynExpr * range * BlockSeparator option * range) option,
                        copyInfo: (SynExpr * BlockSeparator) option,
                        recordFields: SynExprRecordField list,
                        range: range) ->
            // Handle record construction: { field1 = value1; field2 = value2 }
            // Type annotations:
            // - baseInfo: inheritance information for records
            // - copyInfo: copy-and-update syntax info (e.g., { existing with field = newValue })
            // - recordFields: list of field assignments
            // - range: location of the entire record expression
            
            let baseMessages: Message list =
                match baseInfo with
                | Some (_, baseExpr, _, _, _) -> analyzeExpression baseExpr
                | None -> []
            
            let copyMessages: Message list =
                match copyInfo with
                | Some (copyExpr, _) -> analyzeExpression copyExpr
                | None -> []
            
            let fieldMessages: Message list =
                recordFields
                |> List.collect (fun field ->
                    match field with
                    | SynExprRecordField((fieldName, _), equals, expr, blockSeparator) ->
                        match expr with
                        | Some fieldExpr -> analyzeExpression fieldExpr
                        | None -> [])
            
            // Example analysis: detect empty records
            let emptyRecordMessage: Message option =
                if recordFields.IsEmpty then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Empty record construction. Ensure all required fields are specified."
                        Code = "SEP010"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest using copy-and-update for records with many fields
            let copyUpdateSuggestion: Message option =
                if recordFields.Length > 5 && copyInfo.IsNone then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Consider using copy-and-update syntax for records with many fields"
                        Code = "SEP011"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine all messages
            let analysisMessages = 
                [emptyRecordMessage; copyUpdateSuggestion]
                |> List.choose id
            
            baseMessages @ copyMessages @ fieldMessages @ analysisMessages
        
        // === TUPLE CONSTRUCTION ===
        | SynExpr.Tuple(isStruct: bool,
                       exprs: SynExpr list,
                       commaRanges: range list,
                       range: range) ->
            // Handle tuple construction: (expr1, expr2, expr3) or struct (expr1, expr2)
            // Type annotations:
            // - isStruct: true for struct tuples
            // - exprs: list of expressions in the tuple
            // - commaRanges: locations of comma separators (for tooling)
            // - range: location of the entire tuple expression
            
            let exprMessages: Message list = 
                exprs
                |> List.collect analyzeExpression
            
            // Example analysis: detect large tuples (suggest using records instead)
            let largeTupleMessage: Message option =
                if exprs.Length > 5 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Tuple has many elements ({exprs.Length}). Consider using a record type for better readability."
                        Code = "SEP012"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: detect single-element tuples (usually unintended)
            let singleElementMessage: Message option =
                if exprs.Length = 1 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Single-element tuple detected. This is rarely intentional."
                        Code = "SEP013"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: struct tuple recommendation for performance
            let structTupleMessage: Message option =
                if not isStruct && exprs.Length <= 3 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Consider using struct tuple for better performance with small tuples"
                        Code = "SEP014"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine all messages
            let analysisMessages = 
                [largeTupleMessage; singleElementMessage; structTupleMessage]
                |> List.choose id
            
            exprMessages @ analysisMessages
        
        // === SEQUENTIAL EXPRESSIONS ===
        | SynExpr.Sequential(debugPoint: DebugPointAtSequential,
                            isTrueSeq: bool,
                            expr1: SynExpr,
                            expr2: SynExpr,
                            range: range,
                            trivia: SynExprSequentialTrivia) ->
            // Handle sequential expressions: expr1; expr2
            // Type annotations:
            // - debugPoint: debug point information
            // - isTrueSeq: false indicates "let v = a in b; v" pattern
            // - expr1: first expression in sequence
            // - expr2: second expression in sequence
            // - range: location of the entire sequential expression
            // - trivia: additional syntax information
            
            let expr1Messages: Message list = analyzeExpression expr1
            let expr2Messages: Message list = analyzeExpression expr2
            
            // Example analysis: detect unused expression results
            let unusedResultMessage: Message option =
                // Simple heuristic: if first expression is a function call that returns a value
                match expr1 with
                | SynExpr.App(_, _, _, _, _) when isTrueSeq ->
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Expression result may be unused. Consider using 'ignore' if intentional."
                        Code = "SEP015"
                        Severity = Severity.Info
                        Range = expr1.Range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine all messages
            let analysisMessages = 
                [unusedResultMessage]
                |> List.choose id
            
            expr1Messages @ expr2Messages @ analysisMessages
        
        // === MODULE-QUALIFIED IDENTIFIERS ===
        | SynExpr.LongIdent(isOptional: bool,
                           longDotId: SynLongIdent,
                           altNameRefCell: SynSimplePatAlternativeIdInfo ref option,
                           range: range) ->
            // Handle module-qualified identifiers: Module.Submodule.identifier
            // Type annotations:
            // - isOptional: true if preceded by '?' for optional named parameters
            // - longDotId: the qualified identifier path
            // - altNameRefCell: alternative names for pattern matching
            // - range: location of the identifier
            
            let identPath = 
                match longDotId with
                | SynLongIdent(id, _, _) -> 
                    id |> List.map (fun ident -> ident.idText) |> String.concat "."
            
            // Example analysis: detect potentially deprecated namespaces
            let deprecatedNamespaceMessage: Message option =
                if identPath.StartsWith("Microsoft.FSharp.Collections.List") then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Consider using 'List' module directly instead of fully qualified name"
                        Code = "SEP016"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: detect overly long qualification
            let longQualificationMessage: Message option =
                let parts = identPath.Split('.')
                if parts.Length > 4 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Very long qualified name ({parts.Length} parts). Consider using module aliases."
                        Code = "SEP017"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [deprecatedNamespaceMessage; longQualificationMessage]
                |> List.choose id
            
            analysisMessages
        
        // === PARENTHESIZED EXPRESSIONS ===
        | SynExpr.Paren(expr: SynExpr,
                       leftParenRange: range,
                       rightParenRange: range option,
                       range: range) ->
            // Handle parenthesized expressions: (expr)
            // Type annotations:
            // - expr: the expression inside parentheses
            // - leftParenRange: location of opening parenthesis
            // - rightParenRange: location of closing parenthesis
            // - range: location of the entire parenthesized expression
            
            let innerMessages: Message list = analyzeExpression expr
            
            // Example analysis: detect unnecessary parentheses
            let unnecessaryParensMessage: Message option =
                match expr with
                | SynExpr.Ident(_) | SynExpr.Const(_, _) ->
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Parentheses around simple expressions may be unnecessary"
                        Code = "SEP018"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [unnecessaryParensMessage]
                |> List.choose id
            
            innerMessages @ analysisMessages
        
        // === TYPE ANNOTATIONS ===
        | SynExpr.Typed(expr: SynExpr,
                       targetType: SynType,
                       range: range) ->
            // Handle type annotations: expr : type
            // Type annotations:
            // - expr: the expression being annotated
            // - targetType: the type annotation
            // - range: location of the entire typed expression
            
            let exprMessages: Message list = analyzeExpression expr
            
            // Example analysis: detect redundant type annotations
            let redundantTypeMessage: Message option =
                match expr with
                | SynExpr.Const(SynConst.Int32(_), _) when 
                    (match targetType with
                     | SynType.LongIdent(SynLongIdent([ident], _, _)) -> ident.idText = "int"
                     | _ -> false) ->
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Type annotation may be redundant for integer literals"
                        Code = "SEP019"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [redundantTypeMessage]
                |> List.choose id
            
            exprMessages @ analysisMessages
        
        // === ARRAYS AND LISTS ===
        | SynExpr.ArrayOrList(isArray: bool,
                             exprs: SynExpr list,
                             range: range) ->
            // Handle array/list construction: [expr1; expr2] or [|expr1; expr2|]
            // Type annotations:
            // - isArray: true for arrays [|...|], false for lists [...]
            // - exprs: list of expressions in the collection
            // - range: location of the entire collection expression
            
            let exprMessages: Message list = 
                exprs
                |> List.collect analyzeExpression
            
            // Example analysis: detect large collections (performance consideration)
            let largeCollectionMessage: Message option =
                if exprs.Length > 100 then
                    let collectionType = if isArray then "array" else "list"
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Large {collectionType} literal ({exprs.Length} elements). Consider using seq or reading from file."
                        Code = "SEP020"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest arrays for performance-critical scenarios
            let arrayPerformanceMessage: Message option =
                if not isArray && exprs.Length > 10 then
                    Some {
                        Type = "SynExpr Pattern Analyzer"
                        Message = "Consider using array instead of list for better performance with large collections"
                        Code = "SEP021"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [largeCollectionMessage; arrayPerformanceMessage]
                |> List.choose id
            
            exprMessages @ analysisMessages
        
        // === OBJECT CONSTRUCTION ===
        | SynExpr.New(isProtected: bool,
                     targetType: SynType,
                     expr: SynExpr,
                     range: range) ->
            // Handle object construction: new Type(args)
            // Type annotations:
            // - isProtected: true if known to be 'family' (protected) scope
            // - targetType: the type being constructed
            // - expr: constructor arguments
            // - range: location of the entire new expression
            
            let exprMessages: Message list = analyzeExpression expr
            
            // Example analysis: detect potentially expensive object creation
            let expensiveConstructionMessage: Message option =
                match targetType with
                | SynType.LongIdent(SynLongIdent(idents, _, _)) ->
                    let typeName = idents |> List.map (fun i -> i.idText) |> String.concat "."
                    if typeName.Contains("StringBuilder") || typeName.Contains("List") || typeName.Contains("Dictionary") then
                        Some {
                            Type = "SynExpr Pattern Analyzer"
                            Message = $"Creating {typeName} - consider object pooling or reuse for performance-critical code"
                            Code = "SEP022"
                            Severity = Severity.Info
                            Range = range
                            Fixes = []
                        }
                    else
                        None
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [expensiveConstructionMessage]
                |> List.choose id
            
            exprMessages @ analysisMessages
        
        | _ ->
            // Handle remaining SynExpr patterns not covered above
            []
    
    /// <summary>
    /// Sample analyzer that uses the SynExpr pattern matching
    /// This demonstrates how to integrate the pattern analysis into a complete analyzer
    /// </summary>
    [<CliAnalyzer>]
    let synExprPatternAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    // Helper function to recursively analyze all expressions in the AST
                    let rec analyzeDeclarations (decls: SynModuleDecl list) : unit =
                        for decl in decls do
                            match decl with
                            | SynModuleDecl.Let(isRecursive: bool, bindings: SynBinding list, range: range) ->
                                // Process let bindings - extract expressions from the bindings
                                for binding in bindings do
                                    match binding with
                                    | SynBinding(_, _, _, _, _, _, _, _, _, expr, _, _, _) ->
                                        let exprMessages = analyzeExpression expr
                                        messages.AddRange(exprMessages)
                            | SynModuleDecl.Expr(expr: SynExpr, range: range) ->
                                // Process standalone expressions
                                let exprMessages = analyzeExpression expr
                                messages.AddRange(exprMessages)
                            | _ ->
                                // Other declaration types - not processing expressions for now
                                ()
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                        for moduleOrNs in modules do
                            match moduleOrNs with
                            | SynModuleOrNamespace(_, _, _, decls, _, _, _, _, _) ->
                                analyzeDeclarations decls
                    | ParsedInput.SigFile(_) ->
                        // Signature files don't contain expressions to analyze
                        ()
                        
                with
                | ex ->
                    messages.Add({
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Error analyzing expressions: {ex.Message}"
                        Code = "SEP999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }