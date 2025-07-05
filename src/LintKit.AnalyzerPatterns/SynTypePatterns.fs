namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text

/// <summary>
/// Complete SynType pattern matching reference with full type annotations
/// 
/// **AI Learning Reference**: This demonstrates how to:
/// - Handle ALL SynType patterns with complete pattern matching
/// - Use consistent message creation for all type expressions
/// - Apply explicit type annotations for AI learning
/// 
/// **For AI Agents**: This is the definitive pattern for SynType analysis.
/// Copy this approach when you need to analyze F# type expressions.
/// **For Humans**: Reference for building type expression analyzers.
/// 
/// SynType represents type expressions in F# code: int, string, List<int>, int -> string, etc.
/// </summary>
module SynTypePatterns =
    
    /// <summary>
    /// Creates an informational message for a type visit
    /// </summary>
    /// <param name="nodeType">Type of type expression visited</param>
    /// <param name="range">Location of the type expression</param>
    /// <param name="description">Description of what was found</param>
    let private createTypeVisitMessage (nodeType: string) (range: range) (description: string) : Message =
        {
            Type = "SynType Pattern Analyzer"
            Message = $"Visited {nodeType}: {description}"
            Code = "SYNTYPE001"
            Severity = Severity.Info
            Range = range
            Fixes = []
        }
    
    /// <summary>
    /// Analyzes a SynType with complete pattern matching
    /// **AI CRITICAL PATTERN**: This covers ALL SynType cases - use this as your template
    /// </summary>
    /// <param name="synType">The F# type syntax tree node to analyze</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzeType (synType: SynType) : Message list =
        match synType with
        
        // === LONG IDENTIFIER TYPES ===
        | SynType.LongIdent(longDotId: SynLongIdent) ->
            let nodeMsg = createTypeVisitMessage "SynType.LongIdent" longDotId.Range "long identifier type"
            [nodeMsg]
        
        // === TYPE APPLICATIONS ===
        | SynType.App(typeName: SynType, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, isPostfix: bool, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.App" range $"type application (postfix: {isPostfix}, args: {typeArgs.Length})"
            let baseTypeMessages = analyzeType typeName
            let typeArgMessages = typeArgs |> List.collect analyzeType
            nodeMsg :: (baseTypeMessages @ typeArgMessages)
        
        // === FUNCTION TYPES ===
        | SynType.Fun(argType: SynType, returnType: SynType, range: range, trivia: SynTypeFunTrivia) ->
            let nodeMsg = createTypeVisitMessage "SynType.Fun" range "function type"
            let argMessages = analyzeType argType
            let returnMessages = analyzeType returnType
            nodeMsg :: (argMessages @ returnMessages)
        
        // === TUPLE TYPES ===
        | SynType.Tuple(isStruct: bool, path: SynTupleTypeSegment list, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Tuple" range $"tuple type (struct: {isStruct}, segments: {path.Length})"
            let elementMessages = 
                path
                |> List.collect (fun segment ->
                    match segment with
                    | SynTupleTypeSegment.Type(synType) -> analyzeType synType
                    | SynTupleTypeSegment.Star(_) -> []
                    | SynTupleTypeSegment.Slash(_) -> [])
            nodeMsg :: elementMessages
        
        // === ARRAY TYPES ===
        | SynType.Array(rank: int, elementType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Array" range $"array type (rank: {rank})"
            let elementMessages = analyzeType elementType
            nodeMsg :: elementMessages
        
        // === TYPE VARIABLES ===
        | SynType.Var(typar: SynTypar, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Var" range "type variable"
            [nodeMsg]
        
        // === ANONYMOUS RECORD TYPES ===
        | SynType.AnonRecd(isStruct: bool, fields: (Ident * SynType) list, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.AnonRecd" range $"anonymous record (struct: {isStruct}, fields: {fields.Length})"
            let fieldMessages = fields |> List.collect (fun (_, fieldType) -> analyzeType fieldType)
            nodeMsg :: fieldMessages
        
        // === LONG IDENTIFIER APP ===
        | SynType.LongIdentApp(typeName: SynType, longDotId: SynLongIdent, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.LongIdentApp" range $"long identifier application (args: {typeArgs.Length})"
            let baseTypeMessages = analyzeType typeName
            let typeArgMessages = typeArgs |> List.collect analyzeType
            nodeMsg :: (baseTypeMessages @ typeArgMessages)
        
        // === OTHER TYPE PATTERNS ===
        | SynType.Anon(range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Anon" range "anonymous type"
            [nodeMsg]
        
        | SynType.StaticConstant(constant: SynConst, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstant" range "static constant in type"
            [nodeMsg]
        
        | SynType.StaticConstantExpr(expr: SynExpr, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstantExpr" range "static constant expression"
            [nodeMsg]
        
        | SynType.StaticConstantNamed(ident: SynType, value: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstantNamed" range "named static constant"
            let identMessages = analyzeType ident
            let valueMessages = analyzeType value
            nodeMsg :: (identMessages @ valueMessages)
        
        | SynType.WithGlobalConstraints(typeName: SynType, constraints: SynTypeConstraint list, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.WithGlobalConstraints" range $"type with constraints (count: {constraints.Length})"
            let typeMessages = analyzeType typeName
            nodeMsg :: typeMessages
        
        | SynType.HashConstraint(innerType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.HashConstraint" range "hash constraint (flexible type)"
            let innerMessages = analyzeType innerType
            nodeMsg :: innerMessages
        
        | SynType.MeasurePower(baseMeasure: SynType, exponent: SynRationalConst, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.MeasurePower" range "measure power"
            let baseMessages = analyzeType baseMeasure
            nodeMsg :: baseMessages
        
        | SynType.StaticConstantNull(range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstantNull" range "static null constant"
            [nodeMsg]
        
        | SynType.Paren(innerType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Paren" range "parenthesized type"
            let innerMessages = analyzeType innerType
            nodeMsg :: innerMessages
        
        | SynType.WithNull(innerType: SynType, ambivalent: bool, range: range, trivia: SynTypeWithNullTrivia) ->
            let nodeMsg = createTypeVisitMessage "SynType.WithNull" range $"nullable type (ambivalent: {ambivalent})"
            let innerMessages = analyzeType innerType
            nodeMsg :: innerMessages
        
        | SynType.SignatureParameter(attributes: SynAttributes, optional: bool, paramId: Ident option, usedType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.SignatureParameter" range $"signature parameter (optional: {optional})"
            let typeMessages = analyzeType usedType
            nodeMsg :: typeMessages
        
        | SynType.Or(lhsType: SynType, rhsType: SynType, range: range, trivia: SynTypeOrTrivia) ->
            let nodeMsg = createTypeVisitMessage "SynType.Or" range "or type"
            let lhsMessages = analyzeType lhsType
            let rhsMessages = analyzeType rhsType
            nodeMsg :: (lhsMessages @ rhsMessages)
        
        | SynType.Intersection(typar: SynTypar option, types: SynType list, range: range, trivia: SynTyparDeclTrivia) ->
            let nodeMsg = createTypeVisitMessage "SynType.Intersection" range $"intersection type (types: {types.Length})"
            let typeMessages = types |> List.collect analyzeType
            nodeMsg :: typeMessages
        
        | SynType.FromParseError(range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.FromParseError" range "type from parse error"
            [nodeMsg]
        
    
    /// <summary>
    /// Sample analyzer that uses the SynType pattern matching
    /// This demonstrates how to integrate the pattern analysis into a complete analyzer
    /// **AI PATTERN**: Use this structure for your own type expression analyzers
    /// </summary>
    [<CliAnalyzer>]
    let typePatternAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    // Note: This is a simplified example. Full type analysis would require
                    // traversing the entire AST to find type annotations in let bindings, type definitions, etc.
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
                                            | SynBinding(_, _, _, _, _, _, _, _, returnInfo, _, _, _, _) ->
                                                match returnInfo with
                                                | Some (SynBindingReturnInfo(typeName, _, _, _)) ->
                                                    let typeMessages = analyzeType typeName
                                                    messages.AddRange typeMessages
                                                | None -> ()
                                    | _ -> ()
                    | ParsedInput.SigFile(_) ->
                        ()
                        
                with
                | ex ->
                    messages.Add({
                        Type = "SynType Pattern Analyzer"
                        Message = $"Error analyzing types: {ex.Message}"
                        Code = "SYNTYPE999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }