namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text
open FSharp.Compiler.Xml

/// <summary>
/// Complete F# AST pattern matching reference - consolidates all major syntax tree nodes
/// 
/// **AI Learning Reference**: This demonstrates how to:
/// - Handle ALL F# AST patterns (SynExpr, SynModuleDecl, SynPat, SynType)
/// - Use consistent message creation across all AST node types  
/// - Apply explicit type annotations for AI learning
/// - Implement proper recursive traversal patterns
/// 
/// **For AI Agents**: This is the definitive pattern collection for F# AST analysis.
/// Copy and adapt these patterns when you need to analyze F# syntax trees.
/// **For Humans**: Complete reference for building comprehensive analyzers.
/// 
/// This file consolidates patterns from:
/// - SynExpr: All F# expressions (73 patterns) ✅ COMPLETE
/// - SynModuleDecl: Module declarations (10 patterns) ✅ COMPLETE  
/// - SynPat: Pattern matching constructs (20 patterns) ✅ COMPLETE
/// - SynType: Type expressions (23 patterns) ✅ COMPLETE
/// </summary>
module CompleteASTPatterns =

    let flip f x y = f y x
    let trd (_, _, a) = a
    let createListAnalyzer<'a> (analyzer: 'a -> list<Message> -> list<Message>) = flip (List.fold (flip analyzer))
    let createOptionAnalyzer<'a> (analyzer: 'a -> list<Message> -> list<Message>) = Option.map analyzer >> Option.defaultValue id
    
    /// <summary>
    /// Extracts all SynAttribute from SynAttributes
    /// SynAttributes -> List<SynAttributeList> -> List<SynAttribute>
    /// </summary>
    /// <param name="attributes">The SynAttributes to extract attributes from</param>
    /// <returns>List of all SynAttribute from all attribute lists</returns>
    let extractAttributes (attributes: SynAttributes) : SynAttribute list =
        attributes 
        |> List.collect (fun attrList -> attrList.Attributes)

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
    /// Creates an informational message for a pattern visit
    /// </summary>
    /// <param name="nodeType">Type of pattern visited</param>
    /// <param name="range">Location of the pattern</param>
    /// <param name="description">Description of what was found</param>
    let private createPatternVisitMessage (nodeType: string) (range: range) (description: string) : Message =
        {
            Type = "SynPat Pattern Analyzer"
            Message = $"Visited {nodeType}: {description}"
            Code = "SYNPAT001"
            Severity = Severity.Info
            Range = range
            Fixes = []
        }
    
    /// <summary>
    /// Creates an informational message for an AST node visit
    /// </summary>
    /// <param name="nodeType">Type of AST node visited</param>
    /// <param name="range">Location of the node</param>
    /// <param name="description">Description of what was found</param>
    let private createNodeVisitMessage (nodeType: string) (range: range) (description: string) : Message =
        {
            Type = "SynExpr Pattern Analyzer"
            Message = $"Visited {nodeType}: {description}"
            Code = "SYNEXPR001"
            Severity = Severity.Info
            Range = range
            Fixes = []
        }
    
    /// <summary>
    /// Creates an informational message for a module declaration visit
    /// </summary>
    /// <param name="nodeType">Type of module declaration visited</param>
    /// <param name="range">Location of the declaration</param>
    /// <param name="description">Description of what was found</param>
    let private createModuleDeclVisitMessage (nodeType: string) (range: range) (description: string) : Message =
        {
            Type = "SynModuleDecl Pattern Analyzer"
            Message = $"Visited {nodeType}: {description}"
            Code = "MODULEDECL001"
            Severity = Severity.Info
            Range = range
            Fixes = []
        }

    /// <summary>
    /// Creates an informational message for a module or namespace visit
    /// </summary>
    /// <param name="nodeType">Type of module or namespace visited</param>
    /// <param name="range">Location of the module or namespace</param>
    /// <param name="description">Description of what was found</param>
    let private createModuleOrNamespaceVisitMessage (nodeType: string) (range: range) (description: string) : Message =
        {
            Type = "SynModuleOrNamespace Pattern Analyzer"
            Message = $"Visited {nodeType}: {description}"
            Code = "SYNMODNS001"
            Severity = Severity.Info
            Range = range
            Fixes = []
        }

    /// <summary>
    /// Analyzes a SynType with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynType cases - use this as your template
    /// </summary>
    /// <param name="synType">The F# type syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzeTypeAcc (synType: SynType) (acc: Message list) : Message list =
        let analyzeTypes = createListAnalyzer analyzeTypeAcc
        let analyzeExpressions = createListAnalyzer analyzeExpressionAcc
        
        match synType with
        
        // === LONG IDENTIFIER TYPES ===
        | SynType.LongIdent(longDotId: SynLongIdent) ->
            let nodeMsg = createTypeVisitMessage "SynType.LongIdent" longDotId.Range "long identifier type"
            let acc = nodeMsg :: acc

            acc
        
        // === TYPE APPLICATIONS ===
        | SynType.App(typeName: SynType, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, isPostfix: bool, range: range) ->            
            let nodeMsg = createTypeVisitMessage "SynType.App" range $"type application (postfix: {isPostfix}, args: {typeArgs.Length})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypeAcc typeName
            |> analyzeTypes typeArgs
        
        // === FUNCTION TYPES ===
        | SynType.Fun(argType: SynType, returnType: SynType, range: range, trivia: SynTypeFunTrivia) ->
            let nodeMsg = createTypeVisitMessage "SynType.Fun" range "function type"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypeAcc argType
            |> analyzeTypeAcc returnType
        
        // === TUPLE TYPES ===
        | SynType.Tuple(isStruct: bool, path: SynTupleTypeSegment list, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Tuple" range $"tuple type (struct: {isStruct}, segments: {path.Length})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypes (path |> List.choose (function
                | SynTupleTypeSegment.Type(typeName: SynType) -> Some typeName
                | SynTupleTypeSegment.Star(_) -> None
                | SynTupleTypeSegment.Slash(_) -> None))
        
        // === ARRAY TYPES ===
        | SynType.Array(rank: int, elementType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Array" range $"array type (rank: {rank})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc elementType
        
        // === TYPE VARIABLES ===
        | SynType.Var(typar: SynTypar, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Var" range "type variable"
            let acc = nodeMsg :: acc
            
            acc
        
        // === ANONYMOUS RECORD TYPES ===
        | SynType.AnonRecd(isStruct: bool, fields: (Ident * SynType) list, range: range) ->            
            let nodeMsg = createTypeVisitMessage "SynType.AnonRecd" range $"anonymous record (struct: {isStruct}, fields: {fields.Length})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypes (fields |> List.map snd)
        
        // === LONG IDENTIFIER APP ===
        | SynType.LongIdentApp(typeName: SynType, longDotId: SynLongIdent, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, range: range) ->            
            let nodeMsg = createTypeVisitMessage "SynType.LongIdentApp" range $"long identifier application (args: {typeArgs.Length})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc typeName
            |> analyzeTypes typeArgs
        
        // === OTHER TYPE PATTERNS ===
        | SynType.Anon(range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Anon" range "anonymous type"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynType.StaticConstant(constant: SynConst, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstant" range "static constant in type"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynType.StaticConstantExpr(expr: SynExpr, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstantExpr" range "static constant expression"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
        
        | SynType.StaticConstantNamed(ident: SynType, value: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstantNamed" range "named static constant"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc ident
            |> analyzeTypeAcc value
        
        | SynType.WithGlobalConstraints(typeName: SynType, constraints: SynTypeConstraint list, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.WithGlobalConstraints" range $"type with constraints (count: {constraints.Length})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc typeName
        
        | SynType.HashConstraint(innerType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.HashConstraint" range "hash constraint (flexible type)"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc innerType
        
        | SynType.MeasurePower(baseMeasure: SynType, exponent: SynRationalConst, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.MeasurePower" range "measure power"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc baseMeasure
        
        | SynType.StaticConstantNull(range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.StaticConstantNull" range "static null constant"
            let acc = nodeMsg :: acc

            acc
        
        | SynType.Paren(innerType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.Paren" range "parenthesized type"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc innerType
        
        | SynType.WithNull(innerType: SynType, ambivalent: bool, range: range, trivia: SynTypeWithNullTrivia) ->
            let nodeMsg = createTypeVisitMessage "SynType.WithNull" range $"nullable type (ambivalent: {ambivalent})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc innerType
        
        | SynType.SignatureParameter(attributes: SynAttributes, optional: bool, paramId: Ident option, usedType: SynType, range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.SignatureParameter" range $"signature parameter (optional: {optional})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressions (extractAttributes attributes |> List.map (fun attr -> attr.ArgExpr))
            |> analyzeTypeAcc usedType
        
        | SynType.Or(lhsType: SynType, rhsType: SynType, range: range, trivia: SynTypeOrTrivia) ->
            let nodeMsg = createTypeVisitMessage "SynType.Or" range "or type"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc lhsType
            |> analyzeTypeAcc rhsType
        
        | SynType.Intersection(typar: SynTypar option, types: SynType list, range: range, trivia: SynTyparDeclTrivia) ->            
            let nodeMsg = createTypeVisitMessage "SynType.Intersection" range $"intersection type (types: {types.Length})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypes types
        
        | SynType.FromParseError(range: range) ->
            let nodeMsg = createTypeVisitMessage "SynType.FromParseError" range "type from parse error"
            let acc = nodeMsg :: acc

            acc
    
    /// <summary>
    /// Analyzes a SynPat with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynPat cases - use this as your template
    /// </summary>
    /// <param name="pat">The F# pattern syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzePatternAcc (pat: SynPat) (acc: Message list) : Message list =
        let analyzePatterns = createListAnalyzer analyzePatternAcc
        let analyzeArgPats (argPats: SynArgPats) (acc: Message list) : Message list = 
            match argPats with
            | SynArgPats.Pats(pats: SynPat list) ->
                acc
                |> analyzePatterns pats
            | SynArgPats.NamePatPairs(pats: (Ident * range option * SynPat) list, range: range, trivia: SynArgPatsNamePatPairsTrivia) ->
                let nodeMsg = createPatternVisitMessage "SynArgPats.NamePatPairs" range $"named pattern pairs (count: {pats.Length})"
                let acc = nodeMsg :: acc
                
                acc
                |> analyzePatterns (pats |> List.map trd)
        
        match pat with
        
        // === BASIC PATTERNS ===
        | SynPat.Const(constant: SynConst, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Const" range $"constant pattern: {constant}"
            let acc = nodeMsg :: acc

            acc
        
        | SynPat.Wild(range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Wild" range "wildcard pattern (_)"
            let acc = nodeMsg :: acc

            acc
        
        | SynPat.Named(ident: SynIdent, isThisVal: bool, accessibility: SynAccess option, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Named" range $"named pattern (isThis: {isThisVal})"
            let acc = nodeMsg :: acc

            acc
        
        | SynPat.Typed(pat: SynPat, targetType: SynType, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Typed" range "typed pattern"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc targetType
            |> analyzePatternAcc pat
        
        // === COLLECTION PATTERNS ===
        | SynPat.Tuple(isStruct: bool, elementPats: SynPat list, commaRanges: range list, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Tuple" range $"tuple pattern (struct: {isStruct}, elements: {elementPats.Length})"
            let acc = nodeMsg :: acc
        
            acc
            |> analyzePatterns elementPats
        
        | SynPat.ArrayOrList(isArray: bool, elementPats: SynPat list, range: range) ->
            let patType = if isArray then "array" else "list"
            let nodeMsg = createPatternVisitMessage "SynPat.ArrayOrList" range $"{patType} pattern (elements: {elementPats.Length})"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatterns elementPats
        
        | SynPat.Record(fieldPats: ((LongIdent * Ident) * range option * SynPat) list, range: range) ->            
            let nodeMsg = createPatternVisitMessage "SynPat.Record" range $"record pattern (fields: {fieldPats.Length})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzePatterns (fieldPats |> List.map trd)

        // === IDENTIFIER PATTERNS ===
        | SynPat.LongIdent(longDotId: SynLongIdent, extraId: Ident option, typarDecls: SynValTyparDecls option, argPats: SynArgPats, accessibility: SynAccess option, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.LongIdent" range "long identifier pattern"
            let acc = nodeMsg :: acc

            acc
            |> analyzeArgPats argPats
        
        | SynPat.Paren(pat: SynPat, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Paren" range "parenthesized pattern"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatternAcc pat
        
        // === ADVANCED PATTERNS ===
        | SynPat.Attrib(pat: SynPat, attributes: SynAttributes, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Attrib" range $"attributed pattern (attributes: {attributes.Length})"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatternAcc pat
        
        | SynPat.Or(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia: SynPatOrTrivia) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Or" range "or pattern (|)"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatternAcc lhsPat
            |> analyzePatternAcc rhsPat
        
        | SynPat.ListCons(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia: SynPatListConsTrivia) ->
            let nodeMsg = createPatternVisitMessage "SynPat.ListCons" range "list cons pattern (::)"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatternAcc lhsPat
            |> analyzePatternAcc rhsPat
        
        | SynPat.Ands(pats: SynPat list, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Ands" range $"and pattern (&) with {pats.Length} patterns"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatterns pats
        
        | SynPat.As(lhsPat: SynPat, rhsPat: SynPat, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.As" range "as pattern"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatternAcc lhsPat
            |> analyzePatternAcc rhsPat
        
        | SynPat.Null(range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Null" range "null pattern"
            let acc = nodeMsg :: acc

            acc
        
        | SynPat.OptionalVal(ident: Ident, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.OptionalVal" range "optional value pattern"
            let acc = nodeMsg :: acc

            acc
        
        | SynPat.IsInst(targetType: SynType, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.IsInst" range "type test pattern (:? type)"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc targetType
        
        | SynPat.QuoteExpr(expr: SynExpr, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.QuoteExpr" range "quoted expression pattern"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynPat.InstanceMember(thisId: Ident, memberId: Ident, toolId: Ident option, accessibility: SynAccess option, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.InstanceMember" range "instance member pattern"
            let acc = nodeMsg :: acc

            acc
        
        | SynPat.FromParseError(pat: SynPat, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.FromParseError" range "pattern from parse error"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatternAcc pat
    
    /// <summary>
    /// Recursively analyzes SynExpr with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL 73 SynExpr cases - use this as your template
    /// </summary>
    /// <param name="expr">Expression to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>Updated accumulator with messages from this expression and all sub-expressions</returns>
    and analyzeExpressionAcc (expr: SynExpr) (acc: Message list) : Message list =
        let analyzeOptionalExpression = createOptionAnalyzer analyzeExpressionAcc
        let analyzeExpressions = createListAnalyzer analyzeExpressionAcc
        let analyzeTypes = createListAnalyzer analyzeTypeAcc
        let analyzeBindings = createListAnalyzer analyzeBindingAcc
        let analyzeInterfaceImpls = createListAnalyzer analyzeSynInterfaceImplAcc
        let analyzePatterns = createListAnalyzer analyzePatternAcc
        
        match expr with
        
        // === BASIC CASES ===
        | SynExpr.Paren(expr: SynExpr, leftParenRange: range, rightParenRange: range option, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Paren" range "parenthesized expression"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.Quote(operator: SynExpr, isRaw: bool, quotedExpr: SynExpr, isFromQueryExpression: bool, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Quote" range $"quoted expression (raw: {isRaw})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc operator
            |> analyzeExpressionAcc quotedExpr
        
        | SynExpr.Const(constant: SynConst, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Const" range $"constant value: {constant}"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynExpr.Typed(expr: SynExpr, targetType: SynType, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Typed" range "type annotation"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
            |> analyzeTypeAcc targetType
        
        | SynExpr.Tuple(isStruct: bool, exprs: SynExpr list, commaRanges: range list, range: range) ->
            let analyzeExprs = createListAnalyzer analyzeExpressionAcc
            
            let nodeMsg = createNodeVisitMessage "SynExpr.Tuple" range $"""tuple (struct: {isStruct}, count: {exprs.Length})"""
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExprs exprs
        
        | SynExpr.AnonRecd(isStruct: bool, copyInfo: (SynExpr * BlockSeparator) option, recordFields: (SynLongIdent * range option * SynExpr) list, range: range, trivia) ->            
            let nodeMsg = createNodeVisitMessage "SynExpr.AnonRecd" range $"""anonymous record (struct: {isStruct})"""
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeOptionalExpression (copyInfo |> Option.map fst)
            |> analyzeExpressions (recordFields |> List.map trd)
        
        | SynExpr.ArrayOrList(isArray: bool, exprs: SynExpr list, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.ArrayOrList" range $"""{if isArray then "array" else "list"} with {exprs.Length} elements"""
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressions exprs
        
        | SynExpr.Record(baseInfo: (SynType * SynExpr * range * BlockSeparator option * range) option, copyInfo: (SynExpr * BlockSeparator) option, recordFields: SynExprRecordField list, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Record" range "record expression"
            let acc = nodeMsg :: acc

            acc
            |> analyzeOptionalExpression (baseInfo |> Option.map (fun (_, expr, _, _, _) -> expr))
            |> analyzeOptionalExpression (copyInfo |> Option.map fst)
            |> analyzeExpressions (recordFields |> List.choose (function SynExprRecordField(_, _, expr, _) -> expr))
        
        | SynExpr.New(isProtected: bool, targetType: SynType, expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.New" range $"""new expression (protected: {isProtected})"""
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc targetType
            |> analyzeExpressionAcc expr
        
        | SynExpr.ObjExpr(objType: SynType, argOptions: (SynExpr * Ident option) option, withKeyword: range option, bindings: SynBinding list, members: SynMemberDefns, extraImpls: SynInterfaceImpl list, newExprRange: range, range: range) ->            
            let nodeMsg = createNodeVisitMessage "SynExpr.ObjExpr" range "object expression"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypeAcc objType
            |> analyzeOptionalExpression (argOptions |> Option.map fst)
            |> analyzeBindings bindings
            |> analyzeInterfaceImpls extraImpls
        
        // === CONTROL FLOW ===
        | SynExpr.While(whileDebugPoint: DebugPointAtWhile, whileExpr: SynExpr, doExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.While" range "while loop"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc whileExpr
            |> analyzeExpressionAcc doExpr
        
        | SynExpr.For(forDebugPoint: DebugPointAtFor, toDebugPoint: DebugPointAtInOrTo, ident: Ident, equalsRange: range option, identBody: SynExpr, direction: bool, toBody: SynExpr, doBody: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.For" range $"for loop with identifier: {ident.idText}"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc identBody
            |> analyzeExpressionAcc toBody
            |> analyzeExpressionAcc doBody
        
        | SynExpr.ForEach(forDebugPoint: DebugPointAtFor, inDebugPoint: DebugPointAtInOrTo, seqExprOnly: SeqExprOnly, isFromSource: bool, pat: SynPat, enumExpr: SynExpr, bodyExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.ForEach" range "foreach loop"
            let acc = nodeMsg :: acc

            acc
            |> analyzePatternAcc pat
            |> analyzeExpressionAcc enumExpr
            |> analyzeExpressionAcc bodyExpr
        
        | SynExpr.ArrayOrListComputed(isArray: bool, expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.ArrayOrListComputed" range $"""computed {if isArray then "array" else "list"}"""
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.IndexRange(expr1: SynExpr option, opm: range, expr2: SynExpr option, range1: range, range2: range, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.IndexRange" range "index range"
            let acc = nodeMsg :: acc

            acc
            |> analyzeOptionalExpression expr1
            |> analyzeOptionalExpression expr2
        
        | SynExpr.IndexFromEnd(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.IndexFromEnd" range "index from end"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.ComputationExpr(hasSeqBuilder: bool, expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.ComputationExpr" range $"computation expression (seq builder: {hasSeqBuilder})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.Lambda(fromMethod: bool, inLambdaSeq: bool, args: SynSimplePats, body: SynExpr, parsedData: (SynPat list * SynExpr) option, range: range, trivia: SynExprLambdaTrivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Lambda" range "lambda expression"
            let acc = nodeMsg :: acc

            let accWithParsedData = 
                match parsedData with
                | Some (patterns, expr) ->
                    acc
                    |> analyzePatterns patterns
                    |> analyzeExpressionAcc expr
                | None -> acc
            
            accWithParsedData
            |> analyzeExpressionAcc body
        
        | SynExpr.MatchLambda(isExnMatch: bool, keywordRange: range, matchClauses: SynMatchClause list, matchDebugPoint: DebugPointAtBinding, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.MatchLambda" range $"match lambda (exception match: {isExnMatch})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzePatterns (matchClauses |> List.map (function SynMatchClause(pat, _, _, _, _, _) -> pat))
            |> analyzeExpressions (matchClauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.Match(matchDebugPoint: DebugPointAtBinding, expr: SynExpr, clauses: SynMatchClause list, range: range, trivia: SynExprMatchTrivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Match" range "match expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
            |> analyzePatterns (clauses |> List.map (function SynMatchClause(pat, _, _, _, _, _) -> pat))
            |> analyzeExpressions (clauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.Do(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Do" range "do expression"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.Assert(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Assert" range "assert expression"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
        
        // === FUNCTION APPLICATION ===
        | SynExpr.App(flag: ExprAtomicFlag, isInfix: bool, funcExpr: SynExpr, argExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.App" range $"function application (infix: {isInfix})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc funcExpr
            |> analyzeExpressionAcc argExpr
        
        | SynExpr.TypeApp(expr: SynExpr, lessRange: range, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, typeArgsRange: range, range: range) ->            
            let nodeMsg = createNodeVisitMessage "SynExpr.TypeApp" range $"type application with {typeArgs.Length} type arguments"
            let acc = nodeMsg :: acc

            acc
            |> analyzeExpressionAcc expr
            |> analyzeTypes typeArgs
        
        // === BINDINGS ===
        | SynExpr.LetOrUse(isRecursive: bool, isUse: bool, bindings: SynBinding list, body: SynExpr, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LetOrUse" range $"let binding (recursive: {isRecursive}, use: {isUse})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeBindings bindings
            |> analyzeExpressionAcc body
        
        // === ERROR HANDLING ===
        | SynExpr.TryWith(tryExpr: SynExpr, withCases: SynMatchClause list, range: range, tryDebugPoint: DebugPointAtTry, withDebugPoint: DebugPointAtWith, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.TryWith" range "try-with expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc tryExpr
            |> analyzeExpressions (withCases |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.TryFinally(tryExpr: SynExpr, finallyExpr: SynExpr, range: range, tryDebugPoint: DebugPointAtTry, finallyDebugPoint: DebugPointAtFinally, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.TryFinally" range "try-finally expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc tryExpr
            |> analyzeExpressionAcc finallyExpr
        
        | SynExpr.Lazy(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Lazy" range "lazy expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.Sequential(debugPoint: DebugPointAtSequential, isTrueSeq: bool, expr1: SynExpr, expr2: SynExpr, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Sequential" range $"sequential expression (true seq: {isTrueSeq})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr1
            |> analyzeExpressionAcc expr2
        
        | SynExpr.IfThenElse(ifExpr: SynExpr, thenExpr: SynExpr, elseExpr: SynExpr option, spIfToThen: DebugPointAtBinding, isFromErrorRecovery: bool, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.IfThenElse" range "if-then-else expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc ifExpr
            |> analyzeExpressionAcc thenExpr
            |> analyzeOptionalExpression elseExpr
        
        // === IDENTIFIERS ===
        | SynExpr.Typar(typar: SynTypar, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Typar" range $"type parameter: {typar}"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynExpr.Ident(ident: Ident) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Ident" ident.idRange $"identifier: {ident.idText}"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynExpr.LongIdent(isOptional: bool, longDotId: SynLongIdent, altNameRefCell: SynSimplePatAlternativeIdInfo ref option, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LongIdent" range $"long identifier (optional: {isOptional})"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynExpr.LongIdentSet(longDotId: SynLongIdent, expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LongIdentSet" range "long identifier assignment"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        // === MEMBER ACCESS ===
        | SynExpr.DotGet(expr: SynExpr, rangeOfDot: range, longDotId: SynLongIdent, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DotGet" range "dot get access"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.DotLambda(expr: SynExpr, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DotLambda" range "dot lambda"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.DotSet(targetExpr: SynExpr, longDotId: SynLongIdent, rhsExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DotSet" range "dot set assignment"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc targetExpr
            |> analyzeExpressionAcc rhsExpr
        
        | SynExpr.Set(targetExpr: SynExpr, rhsExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Set" range "set assignment"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc targetExpr
            |> analyzeExpressionAcc rhsExpr
        
        | SynExpr.DotIndexedGet(objectExpr: SynExpr, indexArgs: SynExpr, dotRange: range, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DotIndexedGet" range "indexed get access"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc objectExpr
            |> analyzeExpressionAcc indexArgs
        
        | SynExpr.DotIndexedSet(objectExpr: SynExpr, indexArgs: SynExpr, valueExpr: SynExpr, leftOfSetRange: range, dotRange: range, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DotIndexedSet" range "indexed set assignment"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc objectExpr
            |> analyzeExpressionAcc indexArgs
            |> analyzeExpressionAcc valueExpr
        
        | SynExpr.NamedIndexedPropertySet(longDotId: SynLongIdent, expr1: SynExpr, expr2: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.NamedIndexedPropertySet" range "named indexed property set"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr1
            |> analyzeExpressionAcc expr2
        
        | SynExpr.DotNamedIndexedPropertySet(targetExpr: SynExpr, longDotId: SynLongIdent, argExpr: SynExpr, rhsExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DotNamedIndexedPropertySet" range "dot named indexed property set"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc targetExpr
            |> analyzeExpressionAcc argExpr
            |> analyzeExpressionAcc rhsExpr
        
        // === TYPE OPERATIONS ===
        | SynExpr.TypeTest(expr: SynExpr, targetType: SynType, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.TypeTest" range "type test"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypeAcc targetType
            |> analyzeExpressionAcc expr
        
        | SynExpr.Upcast(expr: SynExpr, targetType: SynType, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Upcast" range "upcast"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypeAcc targetType
            |> analyzeExpressionAcc expr
        
        | SynExpr.Downcast(expr: SynExpr, targetType: SynType, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Downcast" range "downcast"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypeAcc targetType
            |> analyzeExpressionAcc expr
        
        | SynExpr.InferredUpcast(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.InferredUpcast" range "inferred upcast"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.InferredDowncast(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.InferredDowncast" range "inferred downcast"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.Null(range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Null" range "null value"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynExpr.AddressOf(isByref: bool, expr: SynExpr, opRange: range, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.AddressOf" range $"address of (byref: {isByref})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.TraitCall(supportTys: SynType, traitSig: SynMemberSig, argExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.TraitCall" range "trait call"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeTypeAcc supportTys
            |> analyzeExpressionAcc argExpr
        
        | SynExpr.JoinIn(lhsExpr: SynExpr, lhsRange: range, rhsExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.JoinIn" range "join in"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc lhsExpr
            |> analyzeExpressionAcc rhsExpr
        
        | SynExpr.ImplicitZero(range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.ImplicitZero" range "implicit zero"
            let acc = nodeMsg :: acc
            
            acc
        
        // === COMPUTATION EXPRESSIONS ===
        | SynExpr.SequentialOrImplicitYield(debugPoint: DebugPointAtSequential, expr1: SynExpr, expr2: SynExpr, ifNotStmt: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.SequentialOrImplicitYield" range "sequential or implicit yield"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr1
            |> analyzeExpressionAcc expr2
            |> analyzeExpressionAcc ifNotStmt
        
        | SynExpr.YieldOrReturn((flags1: bool, flags2: bool), expr: SynExpr, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.YieldOrReturn" range $"yield or return (flags: {flags1}, {flags2})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.YieldOrReturnFrom((flags1: bool, flags2: bool), expr: SynExpr, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.YieldOrReturnFrom" range $"yield or return from (flags: {flags1}, {flags2})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.LetOrUseBang(bindDebugPoint: DebugPointAtBinding, isUse: bool, isFromSource: bool, pat: SynPat, rhs: SynExpr, andBangs: SynExprAndBang list, body: SynExpr, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LetOrUseBang" range $"let or use bang (use: {isUse})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzePatternAcc pat
            |> analyzeExpressionAcc rhs
            |> analyzeExpressions (andBangs |> List.map (function SynExprAndBang(_, _, _, _, expr, _, _) -> expr))
            |> analyzeExpressionAcc body
        
        | SynExpr.MatchBang(matchDebugPoint: DebugPointAtBinding, expr: SynExpr, clauses: SynMatchClause list, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.MatchBang" range "match bang"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
            |> analyzeExpressions (clauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.DoBang(expr: SynExpr, range: range, trivia) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DoBang" range "do bang"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.WhileBang(whileDebugPoint: DebugPointAtWhile, whileExpr: SynExpr, doExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.WhileBang" range "while bang"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc whileExpr
            |> analyzeExpressionAcc doExpr
        
        // === LIBRARY/COMPILER INTERNALS ===
        | SynExpr.LibraryOnlyILAssembly(ilCode: obj, typeArgs: SynType list, args: SynExpr list, retTy: SynType list, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LibraryOnlyILAssembly" range "library only IL assembly"
            let acc = nodeMsg :: acc

            acc
            |> analyzeTypes typeArgs
            |> analyzeExpressions args
            |> analyzeTypes retTy
        
        | SynExpr.LibraryOnlyStaticOptimization(constraints: SynStaticOptimizationConstraint list, expr: SynExpr, optimizedExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LibraryOnlyStaticOptimization" range "library only static optimization"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
            |> analyzeExpressionAcc optimizedExpr
        
        | SynExpr.LibraryOnlyUnionCaseFieldGet(expr: SynExpr, longId: LongIdent, fieldNum: int, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LibraryOnlyUnionCaseFieldGet" range $"library only union case field get (field: {fieldNum})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.LibraryOnlyUnionCaseFieldSet(expr: SynExpr, longId: LongIdent, fieldNum: int, rhsExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.LibraryOnlyUnionCaseFieldSet" range $"library only union case field set (field: {fieldNum})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
            |> analyzeExpressionAcc rhsExpr
        
        // === ERROR RECOVERY ===
        | SynExpr.ArbitraryAfterError(debugStr: string, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.ArbitraryAfterError" range $"arbitrary after error: {debugStr}"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynExpr.FromParseError(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.FromParseError" range "from parse error"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.DiscardAfterMissingQualificationAfterDot(expr: SynExpr, dotRange: range, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DiscardAfterMissingQualificationAfterDot" range "discard after missing qualification after dot"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        | SynExpr.Fixed(expr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Fixed" range "fixed expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
        
        // === STRING INTERPOLATION ===
        | SynExpr.InterpolatedString(contents: SynInterpolatedStringPart list, synStringKind: SynStringKind, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.InterpolatedString" range $"interpolated string with {contents.Length} parts"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressions (contents |> List.choose (function 
                | SynInterpolatedStringPart.String(_, _) -> None
                | SynInterpolatedStringPart.FillExpr(expr, _) -> Some expr))
        
        // === DEBUG SUPPORT ===
        | SynExpr.DebugPoint(debugPoint: DebugPointAtLeafExpr, isControlFlow: bool, innerExpr: SynExpr) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.DebugPoint" innerExpr.Range $"debug point (control flow: {isControlFlow})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc innerExpr
        
        | SynExpr.Dynamic(funcExpr: SynExpr, qmark: range, argExpr: SynExpr, range: range) ->
            let nodeMsg = createNodeVisitMessage "SynExpr.Dynamic" range "dynamic expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc funcExpr
            |> analyzeExpressionAcc argExpr
    
    /// <summary>
    /// Analyzes a SynModuleDecl with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynModuleDecl cases - use this as your template
    /// </summary>
    /// <param name="decl">The F# module declaration syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeModuleDeclAcc (decl: SynModuleDecl) (acc: Message list) : Message list =
        let analyzeModuleDecls = createListAnalyzer analyzeModuleDeclAcc
        let analyzeBindings = createListAnalyzer analyzeBindingAcc
        let analyzeExpressions = createListAnalyzer analyzeExpressionAcc
        
        match decl with
        
        // === LET BINDINGS ===
        | SynModuleDecl.Let(isRecursive: bool, bindings: SynBinding list, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.Let" range $"let binding (recursive: {isRecursive}, count: {bindings.Length})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeBindings bindings
        
        // === TYPE DEFINITIONS ===
        | SynModuleDecl.Types(typeDefns: SynTypeDefn list, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.Types" range $"type definitions (count: {typeDefns.Length})"
            let acc = nodeMsg :: acc
            
            acc
        
        // === EXCEPTION DEFINITIONS ===
        | SynModuleDecl.Exception(exnDefn: SynExceptionDefn, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.Exception" range "exception definition"
            let acc = nodeMsg :: acc
            
            acc
        
        // === OPEN STATEMENTS ===
        | SynModuleDecl.Open(target: SynOpenDeclTarget, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.Open" range "open statement"
            let acc = nodeMsg :: acc
            
            acc
        
        // === MODULE DECLARATIONS ===
        | SynModuleDecl.ModuleAbbrev(ident: Ident, longId: LongIdent, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.ModuleAbbrev" range $"module abbreviation: {ident.idText}"
            let acc = nodeMsg :: acc
            
            acc
        
        | SynModuleDecl.NestedModule(componentInfo: SynComponentInfo, isRecursive: bool, decls: SynModuleDecl list, isContinuing: bool, range: range, trivia) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.NestedModule" range $"nested module (recursive: {isRecursive}, declarations: {decls.Length})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeModuleDecls decls
        
        // === ATTRIBUTES ===
        | SynModuleDecl.Attributes(attributes: SynAttributes, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.Attributes" range $"attributes (count: {attributes.Length})"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressions (extractAttributes attributes |> List.map (fun attr -> attr.ArgExpr))
        
        // === HASH DIRECTIVES ===
        | SynModuleDecl.HashDirective(hashDirective: ParsedHashDirective, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.HashDirective" range "hash directive"
            let acc = nodeMsg :: acc
            
            acc
        
        // === NAMESPACE FRAGMENT ===
        | SynModuleDecl.NamespaceFragment(moduleOrNamespace: SynModuleOrNamespace) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.NamespaceFragment" moduleOrNamespace.Range "namespace fragment"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeSynModuleOrNamespaceAcc moduleOrNamespace
        
        // === STANDALONE EXPRESSIONS ===
        | SynModuleDecl.Expr(expr: SynExpr, range: range) ->
            let nodeMsg = createModuleDeclVisitMessage "SynModuleDecl.Expr" range "standalone expression"
            let acc = nodeMsg :: acc
            
            acc
            |> analyzeExpressionAcc expr
    
    /// <summary>
    /// Analyzes a SynModuleOrNamespace with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers SynModuleOrNamespace cases - use this as your template
    /// </summary>
    /// <param name="moduleOrNs">The F# module or namespace syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeSynModuleOrNamespaceAcc (moduleOrNs: SynModuleOrNamespace) (acc: Message list) : Message list =
        let analyzeModuleDecls = createListAnalyzer analyzeModuleDeclAcc
        
        match moduleOrNs with
        | SynModuleOrNamespace(longId: LongIdent, isRecursive: bool, kind: SynModuleOrNamespaceKind, decls: SynModuleDecl list, xmlDoc: PreXmlDoc, attribs: SynAttributes, accessibility: SynAccess option, range: range, trivia: SynModuleOrNamespaceTrivia) ->
            let kindStr = function
            | SynModuleOrNamespaceKind.NamedModule -> "named module"
            | SynModuleOrNamespaceKind.AnonModule -> "anonymous module" 
            | SynModuleOrNamespaceKind.DeclaredNamespace -> "declared namespace"
            | SynModuleOrNamespaceKind.GlobalNamespace -> "global namespace"
            let nodeMsg = createModuleOrNamespaceVisitMessage "SynModuleOrNamespace" range $"{kindStr} (recursive: {isRecursive}, declarations: {decls.Length})"
            let acc = nodeMsg :: acc

            acc
            |> analyzeModuleDecls decls
    
    /// <summary>
    /// Analyzes a SynBinding with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynBinding which contains both SynPat and SynExpr
    /// </summary>
    /// <param name="binding">The F# binding syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeBindingAcc (binding: SynBinding) (acc: Message list) : Message list =
        let analyzeExpressions = createListAnalyzer analyzeExpressionAcc
        let analyzeOptionalReturnInfo = createOptionAnalyzer analyzeReturnInfoAcc
            
        match binding with
        | SynBinding(accessibility: SynAccess option, kind: SynBindingKind, isInline: bool, isMutable: bool, attrs: SynAttributes, xmlDoc: PreXmlDoc, valData: SynValData, headPat: SynPat, returnInfo: SynBindingReturnInfo option, expr: SynExpr, range: range, debugPoint: DebugPointAtBinding, trivia: SynBindingTrivia) ->
            acc
            |> analyzeExpressions (extractAttributes attrs |> List.map (fun attr -> attr.ArgExpr))
            |> analyzePatternAcc headPat
            |> analyzeOptionalReturnInfo returnInfo
            |> analyzeExpressionAcc expr
    
    /// <summary>
    /// Analyzes a SynInterfaceImpl with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynInterfaceImpl containing SynType and SynBinding list
    /// </summary>
    /// <param name="interfaceImpl">The F# interface implementation syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeSynInterfaceImplAcc (interfaceImpl: SynInterfaceImpl) (acc: Message list) : Message list =
        let analyzeBindings = createListAnalyzer analyzeBindingAcc
        
        match interfaceImpl with
        | SynInterfaceImpl(interfaceTy: SynType, withKeyword: range option, bindings: SynBinding list, members: SynMemberDefns, range: range) ->
            acc
            |> analyzeTypeAcc interfaceTy
            |> analyzeBindings bindings
    
    /// <summary>
    /// Analyzes a SynBindingReturnInfo with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynBindingReturnInfo containing SynType and SynAttributes
    /// </summary>
    /// <param name="returnInfo">The F# binding return info syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeReturnInfoAcc (returnInfo: SynBindingReturnInfo) (acc: Message list) : Message list =
        let analyzeExpressions = createListAnalyzer analyzeExpressionAcc
            
        match returnInfo with
        | SynBindingReturnInfo(typeName: SynType, range: range, attributes: SynAttributes, trivia: SynBindingReturnInfoTrivia) ->
            acc
            |> analyzeTypeAcc typeName
            |> analyzeExpressions (extractAttributes attributes |> List.map (fun attr -> attr.ArgExpr))
    
    /// <summary>
    /// Sample analyzer that uses the complete SynExpr pattern matching
    /// This demonstrates how to integrate the pattern analysis into a complete analyzer
    /// **AI PATTERN**: Use this structure for your own complete analyzers
    /// </summary>
    [<CliAnalyzer>]
    let exprPatternAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                try
                    let parseResults = context.ParseFileResults
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, contents, _, _, _)) ->
                        let analyzeContents = createListAnalyzer analyzeSynModuleOrNamespaceAcc
                        return [] |> analyzeContents contents |> List.rev
                    | ParsedInput.SigFile(_) ->
                        // Signature files don't contain expressions to analyze
                        return []
                        
                with
                | ex ->
                    return [{
                        Type = "SynExpr Pattern Analyzer"
                        Message = $"Error analyzing expressions: {ex.Message}"
                        Code = "SYNEXPR999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    }]
            }