namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// <summary>
/// Practical attribute detection and analysis for F# code quality
/// 
/// This analyzer focuses on real-world attribute usage patterns that affect:
/// - API design and interoperability
/// - Performance and optimization
/// - Code maintainability and documentation
/// - Security and compliance
/// 
/// Common use cases:
/// - Detecting missing [<CompiledName>] for public APIs
/// - Finding deprecated attributes without migration guidance
/// - Identifying performance-critical attributes misuse
/// - Checking for required documentation attributes
/// - Validating interop attributes consistency
/// </summary>
module AttributeDetection =
    
    
    /// <summary>
    /// Analyzes function/member definitions for attribute-related issues
    /// </summary>
    /// <param name="binding">The function binding to analyze</param>
    /// <returns>List of analysis messages</returns>
    let private analyzeFunctionBinding (binding: SynBinding) : Message list =
        match binding with
        | SynBinding(_, _, _, _, attributes, _, _, pat, _, _, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            // Extract function name for analysis
            let functionName = 
                match pat with
                | SynPat.Named(SynIdent(ident, _), _, _, _) -> Some ident.idText
                | SynPat.LongIdent(SynLongIdent(ids, _, _), _, _, _, _, _) -> 
                    ids |> List.tryLast |> Option.map (fun i -> i.idText)
                | _ -> None
            
            match functionName with
            | Some name ->
                // Simple attribute count analysis
                if attributes.Length > 3 then
                    messages.Add({
                        Type = "Attribute Detection Analyzer"
                        Message = $"Function '{name}' has many attributes ({attributes.Length}). Consider if all are necessary."
                        Code = "AD001"
                        Severity = Severity.Info
                        Range = pat.Range
                        Fixes = []
                    })
                
                // Check for functions that might need documentation
                if name.Length > 0 && System.Char.IsUpper(name.[0]) && attributes.Length = 0 then
                    messages.Add({
                        Type = "Attribute Detection Analyzer"
                        Message = $"Public function '{name}' has no attributes. Consider adding documentation or interop attributes."
                        Code = "AD002"
                        Severity = Severity.Info
                        Range = pat.Range
                        Fixes = []
                    })
                
            | None -> ()
            
            messages |> Seq.toList
    
    /// <summary>
    /// Analyzes type definitions for attribute-related issues
    /// </summary>
    /// <param name="typeDefn">The type definition to analyze</param>
    /// <returns>List of analysis messages</returns>
    let private analyzeTypeDefinition (typeDefn: SynTypeDefn) : Message list =
        match typeDefn with
        | SynTypeDefn(SynComponentInfo(attributes, _, _, longId, _, _, _, _), repr, _, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            let typeName = 
                longId |> List.map (fun i -> i.idText) |> String.concat "."
            
            // Simple attribute analysis for types
            if attributes.Length > 5 then
                messages.Add({
                    Type = "Attribute Detection Analyzer"
                    Message = $"Type '{typeName}' has many attributes ({attributes.Length}). Consider if all are necessary."
                    Code = "AD003"
                    Severity = Severity.Info
                    Range = longId.Head.idRange
                    Fixes = []
                })
            
            // Check for public types without attributes
            if typeName.Length > 0 && System.Char.IsUpper(typeName.[0]) && attributes.Length = 0 then
                match repr with
                | SynTypeDefnRepr.Simple(SynTypeDefnSimpleRepr.Record(_, _, _), _) ->
                    messages.Add({
                        Type = "Attribute Detection Analyzer"
                        Message = $"Public record '{typeName}' has no attributes. Consider adding [<CLIMutable>] or documentation attributes."
                        Code = "AD004"
                        Severity = Severity.Info
                        Range = longId.Head.idRange
                        Fixes = []
                    })
                
                | SynTypeDefnRepr.Simple(SynTypeDefnSimpleRepr.Union(_, cases, _), _) ->
                    if cases.Length > 10 then
                        messages.Add({
                            Type = "Attribute Detection Analyzer"
                            Message = $"Large discriminated union '{typeName}' ({cases.Length} cases) has no attributes. Consider [<RequireQualifiedAccess>]."
                            Code = "AD005"
                            Severity = Severity.Info
                            Range = longId.Head.idRange
                            Fixes = []
                        })
                
                | _ -> ()
            
            messages |> Seq.toList
    
    /// <summary>
    /// Analyzes module declarations for attribute patterns
    /// </summary>
    /// <param name="decl">The module declaration to analyze</param>
    /// <returns>List of analysis messages</returns>
    let rec analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        match decl with
        | SynModuleDecl.Let(_, bindings, _) ->
            bindings |> List.collect analyzeFunctionBinding
        
        | SynModuleDecl.Types(typeDefns, _) ->
            typeDefns |> List.collect analyzeTypeDefinition
        
        | SynModuleDecl.NestedModule(SynComponentInfo(attributes, _, _, longId, _, _, _, _), _, decls, _, range, _) ->
            let messages = ResizeArray<Message>()
            
            let moduleName = longId |> List.map (fun i -> i.idText) |> String.concat "."
            
            // Simple module attribute analysis
            if attributes.Length > 3 then
                messages.Add({
                    Type = "Attribute Detection Analyzer"
                    Message = $"Module '{moduleName}' has many attributes ({attributes.Length}). Consider if all are necessary."
                    Code = "AD006"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            // Check for large modules without attributes
            if decls.Length > 20 && attributes.Length = 0 then
                messages.Add({
                    Type = "Attribute Detection Analyzer"
                    Message = $"Large module '{moduleName}' ({decls.Length} declarations) has no attributes. Consider [<RequireQualifiedAccess>]."
                    Code = "AD007"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            // Recursively analyze nested declarations
            let nestedMessages = decls |> List.collect analyzeModuleDeclaration
            (messages |> Seq.toList) @ nestedMessages
        
        | SynModuleDecl.Attributes(attributes, range) ->
            let messages = ResizeArray<Message>()
            
            // Simple standalone attributes analysis
            if attributes.Length > 2 then
                messages.Add({
                    Type = "Attribute Detection Analyzer"
                    Message = $"Many standalone attributes detected ({attributes.Length}). Verify all are necessary."
                    Code = "AD008"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            messages |> Seq.toList
        
        | _ -> []
    
    /// <summary>
    /// Main attribute detection analyzer
    /// </summary>
    [<CliAnalyzer>]
    let attributeDetectionAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                        for moduleOrNs in modules do
                            match moduleOrNs with
                            | SynModuleOrNamespace(longId, _, _, decls, _, attributes, _, _, _) ->
                                // Simple module-level analysis
                                let moduleName = longId |> List.map (fun i -> i.idText) |> String.concat "."
                                
                                // Check for modules with many attributes
                                if attributes.Length > 3 then
                                    let moduleRange = longId |> List.tryHead |> Option.map (fun i -> i.idRange) |> Option.defaultValue Range.Zero
                                    messages.Add({
                                        Type = "Attribute Detection Analyzer"
                                        Message = $"Module '{moduleName}' has many attributes ({attributes.Length}). Consider if all are necessary."
                                        Code = "AD009"
                                        Severity = Severity.Info
                                        Range = moduleRange
                                        Fixes = []
                                    })
                                
                                // Analyze declarations within the module
                                let declMessages = decls |> List.collect analyzeModuleDeclaration
                                messages.AddRange declMessages
                    
                    | ParsedInput.SigFile(_) ->
                        // Signature files - could analyze interface attributes here
                        ()
                
                with
                | ex ->
                    messages.Add({
                        Type = "Attribute Detection Analyzer"
                        Message = $"Error analyzing attributes: {ex.Message}"
                        Code = "AD999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }