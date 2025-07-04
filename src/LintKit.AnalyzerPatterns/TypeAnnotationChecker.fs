namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// <summary>
/// Type annotation presence checking for F# code quality
/// 
/// This analyzer focuses on detecting missing or excessive type annotations:
/// - Public API functions without explicit type annotations
/// - Complex expressions that would benefit from type hints
/// - Overly verbose type annotations where inference suffices
/// - Parameter type annotations for better documentation
/// 
/// Common use cases:
/// - Ensuring public APIs have explicit type signatures
/// - Detecting when type annotations improve code clarity
/// - Finding redundant type annotations that clutter code
/// - Validating interface and abstract member type annotations
/// - Checking generic type parameter constraints
/// 
/// AI guidance approach: Provide clear patterns for when type annotations
/// are beneficial vs. when they should be omitted for cleaner code.
/// </summary>
module TypeAnnotationChecker =
    
    
    /// <summary>
    /// Extracts function name from binding pattern for analysis
    /// </summary>
    /// <param name="pat">The binding pattern</param>
    /// <returns>Function name if extractable</returns>
    let private getFunctionName (pat: SynPat) : string option =
        match pat with
        | SynPat.Named(SynIdent(ident, _), _, _, _) -> Some ident.idText
        | SynPat.LongIdent(SynLongIdent(ids, _, _), _, _, _, _, _) ->
            ids |> List.tryLast |> Option.map (fun i -> i.idText)
        | _ -> None
    
    /// <summary>
    /// Determines if a function appears to be public based on naming conventions
    /// </summary>
    /// <param name="functionName">The function name to check</param>
    /// <returns>True if the function appears to be public</returns>
    let private isPublicFunction (functionName: string) : bool =
        functionName.Length > 0 && System.Char.IsUpper(functionName.[0])
    
    /// <summary>
    /// Analyzes function bindings for type annotation issues
    /// </summary>
    /// <param name="binding">The function binding to analyze</param>
    /// <returns>List of analysis messages</returns>
    let private analyzeFunctionBinding (binding: SynBinding) : Message list =
        match binding with
        | SynBinding(_, _, _, _, _, _, _, pat, returnInfo, expr, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            match getFunctionName pat with
            | Some functionName ->
                let hasReturnTypeAnnotation = returnInfo.IsSome
                let isPublic = isPublicFunction functionName
                
                // Check public functions without type annotations
                if isPublic && not hasReturnTypeAnnotation then
                    messages.Add({
                        Type = "Type Annotation Checker"
                        Message = $"Public function '{functionName}' lacks explicit return type annotation. Consider adding for API clarity."
                        Code = "TAC001"
                        Severity = Severity.Info
                        Range = pat.Range
                        Fixes = []
                    })
                
                // Check for very simple functions with unnecessary type annotations
                if hasReturnTypeAnnotation then
                    match expr with
                    | SynExpr.Const(SynConst.Int32(_), _) ->
                        messages.Add({
                            Type = "Type Annotation Checker"
                            Message = $"Function '{functionName}' returns simple integer constant. Type annotation may be redundant."
                            Code = "TAC002"
                            Severity = Severity.Info
                            Range = pat.Range
                            Fixes = []
                        })
                    | SynExpr.Const(SynConst.String(_, _, _), _) ->
                        messages.Add({
                            Type = "Type Annotation Checker"
                            Message = $"Function '{functionName}' returns simple string constant. Type annotation may be redundant."
                            Code = "TAC003"
                            Severity = Severity.Info
                            Range = pat.Range
                            Fixes = []
                        })
                    | _ -> ()
                
                // Check for complex expressions that might benefit from type hints
                if not hasReturnTypeAnnotation then
                    let rec isComplexExpression (expr: SynExpr) : bool =
                        match expr with
                        | SynExpr.App(_, _, _, _, _) -> true
                        | SynExpr.Lambda(_, _, _, _, _, _, _) -> true
                        | SynExpr.Match(_, _, _, _, _) -> true
                        | SynExpr.IfThenElse(_, _, _, _, _, _, _) -> true
                        | SynExpr.LetOrUse(_, _, _, body, _, _) -> isComplexExpression body
                        | _ -> false
                    
                    if isComplexExpression expr && functionName.Length > 10 then
                        messages.Add({
                            Type = "Type Annotation Checker"
                            Message = $"Complex function '{functionName}' might benefit from explicit type annotation for clarity."
                            Code = "TAC004"
                            Severity = Severity.Info
                            Range = pat.Range
                            Fixes = []
                        })
                
            | None -> ()
            
            messages |> Seq.toList
    
    /// <summary>
    /// Analyzes type definitions for type annotation completeness
    /// </summary>
    /// <param name="typeDefn">The type definition to analyze</param>
    /// <returns>List of analysis messages</returns>
    let private analyzeTypeDefinition (typeDefn: SynTypeDefn) : Message list =
        match typeDefn with
        | SynTypeDefn(SynComponentInfo(_, _, _, longId, _, _, _, _), repr, members, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            let typeName = longId |> List.map (fun i -> i.idText) |> String.concat "."
            
            // Check record types for field type annotations
            match repr with
            | SynTypeDefnRepr.Simple(SynTypeDefnSimpleRepr.Record(_, fields, _), _) ->
                // Simplified analysis: check if record has many fields
                if fields.Length > 3 && isPublicFunction typeName then
                    messages.Add({
                        Type = "Type Annotation Checker"
                        Message = $"Public record '{typeName}' has many fields. Ensure all field types are explicitly annotated for API clarity."
                        Code = "TAC005"
                        Severity = Severity.Info
                        Range = longId.Head.idRange
                        Fixes = []
                    })
            
            | SynTypeDefnRepr.Simple(SynTypeDefnSimpleRepr.Union(_, cases, _), _) ->
                // Simplified analysis: check if union has many cases
                if cases.Length > 5 && isPublicFunction typeName then
                    messages.Add({
                        Type = "Type Annotation Checker"
                        Message = $"Public union '{typeName}' has many cases ({cases.Length}). Ensure all case types are clearly annotated."
                        Code = "TAC006"
                        Severity = Severity.Info
                        Range = longId.Head.idRange
                        Fixes = []
                    })
            
            | _ -> ()
            
            // Check member type annotations
            let membersWithoutAnnotations = 
                members |> List.filter (fun memberDefn ->
                    match memberDefn with
                    | SynMemberDefn.Member(SynBinding(_, _, _, _, _, _, _, pat, returnInfo, _, _, _, _), _) ->
                        returnInfo.IsNone && 
                        (match getFunctionName pat with
                         | Some name -> isPublicFunction name
                         | None -> false)
                    | _ -> false
                )
            
            if membersWithoutAnnotations.Length > 0 then
                messages.Add({
                    Type = "Type Annotation Checker"
                    Message = $"Type '{typeName}' has public members without explicit type annotations."
                    Code = "TAC007"
                    Severity = Severity.Info
                    Range = longId.Head.idRange
                    Fixes = []
                })
            
            messages |> Seq.toList
    
    /// <summary>
    /// Analyzes expressions for type annotation opportunities
    /// </summary>
    /// <param name="expr">The expression to analyze</param>
    /// <returns>List of analysis messages</returns>
    let rec private analyzeExpression (expr: SynExpr) : Message list =
        match expr with
        
        // Check let bindings within expressions
        | SynExpr.LetOrUse(_, _, bindings, body, _, _) ->
            let bindingMessages = 
                bindings |> List.collect (fun binding ->
                    match binding with
                    | SynBinding(_, _, _, _, _, _, _, pat, returnInfo, bindingExpr, _, _, _) ->
                        let messages = ResizeArray<Message>()
                        
                        // Check for complex let bindings without type hints
                        if returnInfo.IsNone then
                            match bindingExpr with
                            | SynExpr.App(_, _, _, _, _) ->
                                match getFunctionName pat with
                                | Some name when name.Length > 1 && not (name.StartsWith("_")) ->
                                    messages.Add({
                                        Type = "Type Annotation Checker"
                                        Message = $"Complex let binding '{name}' might benefit from type annotation for clarity."
                                        Code = "TAC008"
                                        Severity = Severity.Info
                                        Range = pat.Range
                                        Fixes = []
                                    })
                                | _ -> ()
                            | _ -> ()
                        
                        messages |> Seq.toList
                )
            
            let bodyMessages = analyzeExpression body
            bindingMessages @ bodyMessages
        
        // Check lambda expressions for parameter type annotations
        | SynExpr.Lambda(_, _, _, body, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            // Simple heuristic: suggest type annotations for complex lambdas
            messages.Add({
                Type = "Type Annotation Checker"
                Message = "Lambda expression detected. Consider explicit parameter type annotations for complex cases."
                Code = "TAC009"
                Severity = Severity.Info
                Range = expr.Range
                Fixes = []
            })
            
            let bodyMessages = analyzeExpression body
            (messages |> Seq.toList) @ bodyMessages
        
        // Check type annotations in expressions
        | SynExpr.Typed(innerExpr, _, _) ->
            let messages = ResizeArray<Message>()
            
            // Check for redundant type annotations on simple expressions
            match innerExpr with
            | SynExpr.Const(_, _) ->
                messages.Add({
                    Type = "Type Annotation Checker"
                    Message = "Type annotation on constant may be redundant - F# can infer this type."
                    Code = "TAC010"
                    Severity = Severity.Info
                    Range = expr.Range
                    Fixes = []
                })
            | SynExpr.Ident(_) ->
                messages.Add({
                    Type = "Type Annotation Checker"
                    Message = "Type annotation on simple identifier may be redundant if type is clear from context."
                    Code = "TAC011"
                    Severity = Severity.Info
                    Range = expr.Range
                    Fixes = []
                })
            | _ -> ()
            
            let innerMessages = analyzeExpression innerExpr
            (messages |> Seq.toList) @ innerMessages
        
        // Recursively analyze other expression types
        | SynExpr.App(_, _, funcExpr, argExpr, _) ->
            (analyzeExpression funcExpr) @ (analyzeExpression argExpr)
        
        | SynExpr.IfThenElse(ifExpr, thenExpr, elseExpr, _, _, _, _) ->
            let ifMessages = analyzeExpression ifExpr
            let thenMessages = analyzeExpression thenExpr
            let elseMessages = 
                match elseExpr with
                | Some expr -> analyzeExpression expr
                | None -> []
            ifMessages @ thenMessages @ elseMessages
        
        | SynExpr.Match(_, expr, clauses, _, _) ->
            let exprMessages = analyzeExpression expr
            let clauseMessages = 
                clauses |> List.collect (fun clause ->
                    match clause with
                    | SynMatchClause(_, whenExpr, resultExpr, _, _, _) ->
                        let resultMessages = analyzeExpression resultExpr
                        let whenMessages = 
                            match whenExpr with
                            | Some whenExpr -> analyzeExpression whenExpr
                            | None -> []
                        resultMessages @ whenMessages
                )
            exprMessages @ clauseMessages
        
        | _ -> []
    
    /// <summary>
    /// Analyzes module declarations for type annotation patterns
    /// </summary>
    /// <param name="decl">The module declaration to analyze</param>
    /// <returns>List of analysis messages</returns>
    let rec analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        match decl with
        | SynModuleDecl.Let(_, bindings, _) ->
            bindings |> List.collect analyzeFunctionBinding
        
        | SynModuleDecl.Types(typeDefns, _) ->
            typeDefns |> List.collect analyzeTypeDefinition
        
        | SynModuleDecl.Expr(expr, _) ->
            analyzeExpression expr
        
        | SynModuleDecl.NestedModule(_, _, decls, _, _, _) ->
            decls |> List.collect analyzeModuleDeclaration
        
        | _ -> []
    
    /// <summary>
    /// Main type annotation checking analyzer
    /// </summary>
    [<CliAnalyzer>]
    let typeAnnotationChecker: Analyzer<CliContext> =
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
                                
                                // Analyze all declarations within the module
                                let declMessages = decls |> List.collect analyzeModuleDeclaration
                                messages.AddRange declMessages
                    
                    | ParsedInput.SigFile(_) ->
                        // Signature files could have their own type annotation analysis
                        ()
                
                with
                | ex ->
                    messages.Add({
                        Type = "Type Annotation Checker"
                        Message = $"Error analyzing type annotations: {ex.Message}"
                        Code = "TAC999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }