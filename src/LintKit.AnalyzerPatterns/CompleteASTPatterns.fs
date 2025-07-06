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
    /// Analyzes a SynType with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynType cases - use this as your template
    /// </summary>
    /// <param name="synType">The F# type syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzeType (synType: SynType) (acc: Message list) : Message list =
        let analyzeTypes = createListAnalyzer analyzeType
        let analyzeExpressions = createListAnalyzer analyzeExpression
        
        match synType with
        
        // === LONG IDENTIFIER TYPES ===
        | SynType.LongIdent(longDotId: SynLongIdent) ->
            acc
        
        // === TYPE APPLICATIONS ===
        | SynType.App(typeName: SynType, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, isPostfix: bool, range: range) ->            
            acc
            |> analyzeType typeName
            |> analyzeTypes typeArgs
        
        // === FUNCTION TYPES ===
        | SynType.Fun(argType: SynType, returnType: SynType, range: range, trivia: SynTypeFunTrivia) ->
            acc
            |> analyzeType argType
            |> analyzeType returnType
        
        // === TUPLE TYPES ===
        | SynType.Tuple(isStruct: bool, path: SynTupleTypeSegment list, range: range) ->
            acc
            |> analyzeTypes (path |> List.choose (function
                | SynTupleTypeSegment.Type(typeName: SynType) -> Some typeName
                | SynTupleTypeSegment.Star(_) -> None
                | SynTupleTypeSegment.Slash(_) -> None))
        
        // === ARRAY TYPES ===
        | SynType.Array(rank: int, elementType: SynType, range: range) ->
            acc
            |> analyzeType elementType
        
        // === TYPE VARIABLES ===
        | SynType.Var(typar: SynTypar, range: range) ->
            acc
        
        // === ANONYMOUS RECORD TYPES ===
        | SynType.AnonRecd(isStruct: bool, fields: (Ident * SynType) list, range: range) ->            
            acc
            |> analyzeTypes (fields |> List.map snd)
        
        // === LONG IDENTIFIER APP ===
        | SynType.LongIdentApp(typeName: SynType, longDotId: SynLongIdent, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, range: range) ->            
            acc
            |> analyzeType typeName
            |> analyzeTypes typeArgs
        
        // === OTHER TYPE PATTERNS ===
        | SynType.Anon(range: range) ->
            acc
        
        | SynType.StaticConstant(constant: SynConst, range: range) ->
            acc
        
        | SynType.StaticConstantExpr(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynType.StaticConstantNamed(ident: SynType, value: SynType, range: range) ->
            acc
            |> analyzeType ident
            |> analyzeType value
        
        | SynType.WithGlobalConstraints(typeName: SynType, constraints: SynTypeConstraint list, range: range) ->
            acc
            |> analyzeType typeName
        
        | SynType.HashConstraint(innerType: SynType, range: range) ->
            acc
            |> analyzeType innerType
        
        | SynType.MeasurePower(baseMeasure: SynType, exponent: SynRationalConst, range: range) ->
            acc
            |> analyzeType baseMeasure
        
        | SynType.StaticConstantNull(range: range) ->
            acc
        
        | SynType.Paren(innerType: SynType, range: range) ->
            acc
            |> analyzeType innerType
        
        | SynType.WithNull(innerType: SynType, ambivalent: bool, range: range, trivia: SynTypeWithNullTrivia) ->
            acc
            |> analyzeType innerType
        
        | SynType.SignatureParameter(attributes: SynAttributes, optional: bool, paramId: Ident option, usedType: SynType, range: range) ->
            acc
            |> analyzeExpressions (extractAttributes attributes |> List.map (fun attr -> attr.ArgExpr))
            |> analyzeType usedType
        
        | SynType.Or(lhsType: SynType, rhsType: SynType, range: range, trivia: SynTypeOrTrivia) ->
            acc
            |> analyzeType lhsType
            |> analyzeType rhsType
        
        | SynType.Intersection(typar: SynTypar option, types: SynType list, range: range, trivia: SynTyparDeclTrivia) ->            
            acc
            |> analyzeTypes types
        
        | SynType.FromParseError(range: range) ->
            acc
    
    /// <summary>
    /// Analyzes a SynPat with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynPat cases - use this as your template
    /// </summary>
    /// <param name="pat">The F# pattern syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzePattern (pat: SynPat) (acc: Message list) : Message list =
        let analyzePatterns = createListAnalyzer analyzePattern
        let analyzeArgPats (argPats: SynArgPats) (acc: Message list) : Message list = 
            match argPats with
            | SynArgPats.Pats(pats: SynPat list) ->
                acc
                |> analyzePatterns pats
            | SynArgPats.NamePatPairs(pats: (Ident * range option * SynPat) list, range: range, trivia: SynArgPatsNamePatPairsTrivia) ->
                acc
                |> analyzePatterns (pats |> List.map trd)
        
        match pat with
        
        // === BASIC PATTERNS ===
        | SynPat.Const(constant: SynConst, range: range) ->
            acc
        
        | SynPat.Wild(range: range) ->
            acc
        
        | SynPat.Named(ident: SynIdent, isThisVal: bool, accessibility: SynAccess option, range: range) ->
            acc
        
        | SynPat.Typed(pat: SynPat, targetType: SynType, range: range) ->
            acc
            |> analyzeType targetType
            |> analyzePattern pat
        
        // === COLLECTION PATTERNS ===
        | SynPat.Tuple(isStruct: bool, elementPats: SynPat list, commaRanges: range list, range: range) ->
            acc
            |> analyzePatterns elementPats
        
        | SynPat.ArrayOrList(isArray: bool, elementPats: SynPat list, range: range) ->
            let patType = if isArray then "array" else "list"
            acc
            |> analyzePatterns elementPats
        
        | SynPat.Record(fieldPats: ((LongIdent * Ident) * range option * SynPat) list, range: range) ->            
            acc
            |> analyzePatterns (fieldPats |> List.map trd)

        // === IDENTIFIER PATTERNS ===
        | SynPat.LongIdent(longDotId: SynLongIdent, extraId: Ident option, typarDecls: SynValTyparDecls option, argPats: SynArgPats, accessibility: SynAccess option, range: range) ->
            acc
            |> analyzeArgPats argPats
        
        | SynPat.Paren(pat: SynPat, range: range) ->
            acc
            |> analyzePattern pat
        
        // === ADVANCED PATTERNS ===
        | SynPat.Attrib(pat: SynPat, attributes: SynAttributes, range: range) ->
            acc
            |> analyzePattern pat
        
        | SynPat.Or(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia: SynPatOrTrivia) ->
            acc
            |> analyzePattern lhsPat
            |> analyzePattern rhsPat
        
        | SynPat.ListCons(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia: SynPatListConsTrivia) ->
            acc
            |> analyzePattern lhsPat
            |> analyzePattern rhsPat
        
        | SynPat.Ands(pats: SynPat list, range: range) ->
            acc
            |> analyzePatterns pats
        
        | SynPat.As(lhsPat: SynPat, rhsPat: SynPat, range: range) ->
            acc
            |> analyzePattern lhsPat
            |> analyzePattern rhsPat
        
        | SynPat.Null(range: range) ->
            acc
        
        | SynPat.OptionalVal(ident: Ident, range: range) ->
            acc
        
        | SynPat.IsInst(targetType: SynType, range: range) ->
            acc
            |> analyzeType targetType
        
        | SynPat.QuoteExpr(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynPat.InstanceMember(thisId: Ident, memberId: Ident, toolId: Ident option, accessibility: SynAccess option, range: range) ->
            acc
        
        | SynPat.FromParseError(pat: SynPat, range: range) ->
            acc
            |> analyzePattern pat
    
    /// <summary>
    /// Recursively analyzes SynExpr with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL 73 SynExpr cases - use this as your template
    /// </summary>
    /// <param name="expr">Expression to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>Updated accumulator with messages from this expression and all sub-expressions</returns>
    and analyzeExpression (expr: SynExpr) (acc: Message list) : Message list =
        let analyzeOptionalExpression = createOptionAnalyzer analyzeExpression
        let analyzeExpressions = createListAnalyzer analyzeExpression
        let analyzeTypes = createListAnalyzer analyzeType
        let analyzeBindings = createListAnalyzer analyzeBinding
        let analyzeInterfaceImpls = createListAnalyzer analyzeSynInterfaceImpl
        let analyzePatterns = createListAnalyzer analyzePattern
        
        match expr with
        
        // === BASIC CASES ===
        | SynExpr.Paren(expr: SynExpr, leftParenRange: range, rightParenRange: range option, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.Quote(operator: SynExpr, isRaw: bool, quotedExpr: SynExpr, isFromQueryExpression: bool, range: range) ->
            acc
            |> analyzeExpression operator
            |> analyzeExpression quotedExpr
        
        | SynExpr.Const(constant: SynConst, range: range) ->
            acc
        
        | SynExpr.Typed(expr: SynExpr, targetType: SynType, range: range) ->
            acc
            |> analyzeExpression expr
            |> analyzeType targetType
        
        | SynExpr.Tuple(isStruct: bool, exprs: SynExpr list, commaRanges: range list, range: range) ->
            let analyzeExprs = createListAnalyzer analyzeExpression
            
            acc
            |> analyzeExprs exprs
        
        | SynExpr.AnonRecd(isStruct: bool, copyInfo: (SynExpr * BlockSeparator) option, recordFields: (SynLongIdent * range option * SynExpr) list, range: range, trivia) ->            
            acc
            |> analyzeOptionalExpression (copyInfo |> Option.map fst)
            |> analyzeExpressions (recordFields |> List.map trd)
        
        | SynExpr.ArrayOrList(isArray: bool, exprs: SynExpr list, range: range) ->
            acc
            |> analyzeExpressions exprs
        
        | SynExpr.Record(baseInfo: (SynType * SynExpr * range * BlockSeparator option * range) option, copyInfo: (SynExpr * BlockSeparator) option, recordFields: SynExprRecordField list, range: range) ->
            acc
            |> analyzeOptionalExpression (baseInfo |> Option.map (fun (_, expr, _, _, _) -> expr))
            |> analyzeOptionalExpression (copyInfo |> Option.map fst)
            |> analyzeExpressions (recordFields |> List.choose (function SynExprRecordField(_, _, expr, _) -> expr))
        
        | SynExpr.New(isProtected: bool, targetType: SynType, expr: SynExpr, range: range) ->
            acc
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.ObjExpr(objType: SynType, argOptions: (SynExpr * Ident option) option, withKeyword: range option, bindings: SynBinding list, members: SynMemberDefns, extraImpls: SynInterfaceImpl list, newExprRange: range, range: range) ->            
            acc
            |> analyzeType objType
            |> analyzeOptionalExpression (argOptions |> Option.map fst)
            |> analyzeBindings bindings
            |> analyzeInterfaceImpls extraImpls
        
        // === CONTROL FLOW ===
        | SynExpr.While(whileDebugPoint: DebugPointAtWhile, whileExpr: SynExpr, doExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression whileExpr
            |> analyzeExpression doExpr
        
        | SynExpr.For(forDebugPoint: DebugPointAtFor, toDebugPoint: DebugPointAtInOrTo, ident: Ident, equalsRange: range option, identBody: SynExpr, direction: bool, toBody: SynExpr, doBody: SynExpr, range: range) ->
            acc
            |> analyzeExpression identBody
            |> analyzeExpression toBody
            |> analyzeExpression doBody
        
        | SynExpr.ForEach(forDebugPoint: DebugPointAtFor, inDebugPoint: DebugPointAtInOrTo, seqExprOnly: SeqExprOnly, isFromSource: bool, pat: SynPat, enumExpr: SynExpr, bodyExpr: SynExpr, range: range) ->
            acc
            |> analyzePattern pat
            |> analyzeExpression enumExpr
            |> analyzeExpression bodyExpr
        
        | SynExpr.ArrayOrListComputed(isArray: bool, expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.IndexRange(expr1: SynExpr option, opm: range, expr2: SynExpr option, range1: range, range2: range, range: range) ->
            acc
            |> analyzeOptionalExpression expr1
            |> analyzeOptionalExpression expr2
        
        | SynExpr.IndexFromEnd(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.ComputationExpr(hasSeqBuilder: bool, expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.Lambda(fromMethod: bool, inLambdaSeq: bool, args: SynSimplePats, body: SynExpr, parsedData: (SynPat list * SynExpr) option, range: range, trivia: SynExprLambdaTrivia) ->
            let accWithParsedData = 
                match parsedData with
                | Some (patterns, expr) ->
                    acc
                    |> analyzePatterns patterns
                    |> analyzeExpression expr
                | None -> acc
            
            accWithParsedData
            |> analyzeExpression body
        
        | SynExpr.MatchLambda(isExnMatch: bool, keywordRange: range, matchClauses: SynMatchClause list, matchDebugPoint: DebugPointAtBinding, range: range) ->
            acc
            |> analyzePatterns (matchClauses |> List.map (function SynMatchClause(pat, _, _, _, _, _) -> pat))
            |> analyzeExpressions (matchClauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.Match(matchDebugPoint: DebugPointAtBinding, expr: SynExpr, clauses: SynMatchClause list, range: range, trivia: SynExprMatchTrivia) ->
            acc
            |> analyzeExpression expr
            |> analyzePatterns (clauses |> List.map (function SynMatchClause(pat, _, _, _, _, _) -> pat))
            |> analyzeExpressions (clauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.Do(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.Assert(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        // === FUNCTION APPLICATION ===
        | SynExpr.App(flag: ExprAtomicFlag, isInfix: bool, funcExpr: SynExpr, argExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression funcExpr
            |> analyzeExpression argExpr
        
        | SynExpr.TypeApp(expr: SynExpr, lessRange: range, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, typeArgsRange: range, range: range) ->            
            acc
            |> analyzeExpression expr
            |> analyzeTypes typeArgs
        
        // === BINDINGS ===
        | SynExpr.LetOrUse(isRecursive: bool, isUse: bool, bindings: SynBinding list, body: SynExpr, range: range, trivia) ->
            acc
            |> analyzeBindings bindings
            |> analyzeExpression body
        
        // === ERROR HANDLING ===
        | SynExpr.TryWith(tryExpr: SynExpr, withCases: SynMatchClause list, range: range, tryDebugPoint: DebugPointAtTry, withDebugPoint: DebugPointAtWith, trivia) ->
            acc
            |> analyzeExpression tryExpr
            |> analyzeExpressions (withCases |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.TryFinally(tryExpr: SynExpr, finallyExpr: SynExpr, range: range, tryDebugPoint: DebugPointAtTry, finallyDebugPoint: DebugPointAtFinally, trivia) ->
            acc
            |> analyzeExpression tryExpr
            |> analyzeExpression finallyExpr
        
        | SynExpr.Lazy(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.Sequential(debugPoint: DebugPointAtSequential, isTrueSeq: bool, expr1: SynExpr, expr2: SynExpr, range: range, trivia) ->
            acc
            |> analyzeExpression expr1
            |> analyzeExpression expr2
        
        | SynExpr.IfThenElse(ifExpr: SynExpr, thenExpr: SynExpr, elseExpr: SynExpr option, spIfToThen: DebugPointAtBinding, isFromErrorRecovery: bool, range: range, trivia) ->
            acc
            |> analyzeExpression ifExpr
            |> analyzeExpression thenExpr
            |> analyzeOptionalExpression elseExpr
        
        // === IDENTIFIERS ===
        | SynExpr.Typar(typar: SynTypar, range: range) ->
            acc
        
        | SynExpr.Ident(ident: Ident) ->
            acc
        
        | SynExpr.LongIdent(isOptional: bool, longDotId: SynLongIdent, altNameRefCell: SynSimplePatAlternativeIdInfo ref option, range: range) ->
            acc
        
        | SynExpr.LongIdentSet(longDotId: SynLongIdent, expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        // === MEMBER ACCESS ===
        | SynExpr.DotGet(expr: SynExpr, rangeOfDot: range, longDotId: SynLongIdent, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.DotLambda(expr: SynExpr, range: range, trivia) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.DotSet(targetExpr: SynExpr, longDotId: SynLongIdent, rhsExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression targetExpr
            |> analyzeExpression rhsExpr
        
        | SynExpr.Set(targetExpr: SynExpr, rhsExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression targetExpr
            |> analyzeExpression rhsExpr
        
        | SynExpr.DotIndexedGet(objectExpr: SynExpr, indexArgs: SynExpr, dotRange: range, range: range) ->
            acc
            |> analyzeExpression objectExpr
            |> analyzeExpression indexArgs
        
        | SynExpr.DotIndexedSet(objectExpr: SynExpr, indexArgs: SynExpr, valueExpr: SynExpr, leftOfSetRange: range, dotRange: range, range: range) ->
            acc
            |> analyzeExpression objectExpr
            |> analyzeExpression indexArgs
            |> analyzeExpression valueExpr
        
        | SynExpr.NamedIndexedPropertySet(longDotId: SynLongIdent, expr1: SynExpr, expr2: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr1
            |> analyzeExpression expr2
        
        | SynExpr.DotNamedIndexedPropertySet(targetExpr: SynExpr, longDotId: SynLongIdent, argExpr: SynExpr, rhsExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression targetExpr
            |> analyzeExpression argExpr
            |> analyzeExpression rhsExpr
        
        // === TYPE OPERATIONS ===
        | SynExpr.TypeTest(expr: SynExpr, targetType: SynType, range: range) ->
            acc
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.Upcast(expr: SynExpr, targetType: SynType, range: range) ->
            acc
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.Downcast(expr: SynExpr, targetType: SynType, range: range) ->
            acc
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.InferredUpcast(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.InferredDowncast(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.Null(range: range) ->
            acc
        
        | SynExpr.AddressOf(isByref: bool, expr: SynExpr, opRange: range, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.TraitCall(supportTys: SynType, traitSig: SynMemberSig, argExpr: SynExpr, range: range) ->
            acc
            |> analyzeType supportTys
            |> analyzeExpression argExpr
        
        | SynExpr.JoinIn(lhsExpr: SynExpr, lhsRange: range, rhsExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression lhsExpr
            |> analyzeExpression rhsExpr
        
        | SynExpr.ImplicitZero(range: range) ->
            acc
        
        // === COMPUTATION EXPRESSIONS ===
        | SynExpr.SequentialOrImplicitYield(debugPoint: DebugPointAtSequential, expr1: SynExpr, expr2: SynExpr, ifNotStmt: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr1
            |> analyzeExpression expr2
            |> analyzeExpression ifNotStmt
        
        | SynExpr.YieldOrReturn((flags1: bool, flags2: bool), expr: SynExpr, range: range, trivia) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.YieldOrReturnFrom((flags1: bool, flags2: bool), expr: SynExpr, range: range, trivia) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.LetOrUseBang(bindDebugPoint: DebugPointAtBinding, isUse: bool, isFromSource: bool, pat: SynPat, rhs: SynExpr, andBangs: SynExprAndBang list, body: SynExpr, range: range, trivia) ->
            acc
            |> analyzePattern pat
            |> analyzeExpression rhs
            |> analyzeExpressions (andBangs |> List.map (function SynExprAndBang(_, _, _, _, expr, _, _) -> expr))
            |> analyzeExpression body
        
        | SynExpr.MatchBang(matchDebugPoint: DebugPointAtBinding, expr: SynExpr, clauses: SynMatchClause list, range: range, trivia) ->
            acc
            |> analyzeExpression expr
            |> analyzeExpressions (clauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.DoBang(expr: SynExpr, range: range, trivia) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.WhileBang(whileDebugPoint: DebugPointAtWhile, whileExpr: SynExpr, doExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression whileExpr
            |> analyzeExpression doExpr
        
        // === LIBRARY/COMPILER INTERNALS ===
        | SynExpr.LibraryOnlyILAssembly(ilCode: obj, typeArgs: SynType list, args: SynExpr list, retTy: SynType list, range: range) ->
            acc
            |> analyzeTypes typeArgs
            |> analyzeExpressions args
            |> analyzeTypes retTy
        
        | SynExpr.LibraryOnlyStaticOptimization(constraints: SynStaticOptimizationConstraint list, expr: SynExpr, optimizedExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
            |> analyzeExpression optimizedExpr
        
        | SynExpr.LibraryOnlyUnionCaseFieldGet(expr: SynExpr, longId: LongIdent, fieldNum: int, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.LibraryOnlyUnionCaseFieldSet(expr: SynExpr, longId: LongIdent, fieldNum: int, rhsExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
            |> analyzeExpression rhsExpr
        
        // === ERROR RECOVERY ===
        | SynExpr.ArbitraryAfterError(debugStr: string, range: range) ->
            acc
        
        | SynExpr.FromParseError(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.DiscardAfterMissingQualificationAfterDot(expr: SynExpr, dotRange: range, range: range) ->
            acc
            |> analyzeExpression expr
        
        | SynExpr.Fixed(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
        
        // === STRING INTERPOLATION ===
        | SynExpr.InterpolatedString(contents: SynInterpolatedStringPart list, synStringKind: SynStringKind, range: range) ->
            acc
            |> analyzeExpressions (contents |> List.choose (function 
                | SynInterpolatedStringPart.String(_, _) -> None
                | SynInterpolatedStringPart.FillExpr(expr, _) -> Some expr))
        
        // === DEBUG SUPPORT ===
        | SynExpr.DebugPoint(debugPoint: DebugPointAtLeafExpr, isControlFlow: bool, innerExpr: SynExpr) ->
            acc
            |> analyzeExpression innerExpr
        
        | SynExpr.Dynamic(funcExpr: SynExpr, qmark: range, argExpr: SynExpr, range: range) ->
            acc
            |> analyzeExpression funcExpr
            |> analyzeExpression argExpr
    
    /// <summary>
    /// Analyzes a SynModuleDecl with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynModuleDecl cases - use this as your template
    /// </summary>
    /// <param name="decl">The F# module declaration syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeModuleDecl (decl: SynModuleDecl) (acc: Message list) : Message list =
        let analyzeModuleDecls = createListAnalyzer analyzeModuleDecl
        let analyzeBindings = createListAnalyzer analyzeBinding
        let analyzeExpressions = createListAnalyzer analyzeExpression
        
        match decl with
        
        // === LET BINDINGS ===
        | SynModuleDecl.Let(isRecursive: bool, bindings: SynBinding list, range: range) ->
            acc
            |> analyzeBindings bindings
        
        // === TYPE DEFINITIONS ===
        | SynModuleDecl.Types(typeDefns: SynTypeDefn list, range: range) ->
            acc
        
        // === EXCEPTION DEFINITIONS ===
        | SynModuleDecl.Exception(exnDefn: SynExceptionDefn, range: range) ->
            acc
        
        // === OPEN STATEMENTS ===
        | SynModuleDecl.Open(target: SynOpenDeclTarget, range: range) ->
            acc
        
        // === MODULE DECLARATIONS ===
        | SynModuleDecl.ModuleAbbrev(ident: Ident, longId: LongIdent, range: range) ->
            acc
        
        | SynModuleDecl.NestedModule(componentInfo: SynComponentInfo, isRecursive: bool, decls: SynModuleDecl list, isContinuing: bool, range: range, trivia) ->
            acc
            |> analyzeModuleDecls decls
        
        // === ATTRIBUTES ===
        | SynModuleDecl.Attributes(attributes: SynAttributes, range: range) ->
            acc
            |> analyzeExpressions (extractAttributes attributes |> List.map (fun attr -> attr.ArgExpr))
        
        // === HASH DIRECTIVES ===
        | SynModuleDecl.HashDirective(hashDirective: ParsedHashDirective, range: range) ->
            acc
        
        // === NAMESPACE FRAGMENT ===
        | SynModuleDecl.NamespaceFragment(moduleOrNamespace: SynModuleOrNamespace) ->
            acc
            |> analyzeSynModuleOrNamespace moduleOrNamespace
        
        // === STANDALONE EXPRESSIONS ===
        | SynModuleDecl.Expr(expr: SynExpr, range: range) ->
            acc
            |> analyzeExpression expr
    
    /// <summary>
    /// Analyzes a SynModuleOrNamespace with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers SynModuleOrNamespace cases - use this as your template
    /// </summary>
    /// <param name="moduleOrNs">The F# module or namespace syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeSynModuleOrNamespace (moduleOrNs: SynModuleOrNamespace) (acc: Message list) : Message list =
        let analyzeModuleDecls = createListAnalyzer analyzeModuleDecl
        
        match moduleOrNs with
        | SynModuleOrNamespace(longId: LongIdent, isRecursive: bool, kind: SynModuleOrNamespaceKind, decls: SynModuleDecl list, xmlDoc: PreXmlDoc, attribs: SynAttributes, accessibility: SynAccess option, range: range, trivia: SynModuleOrNamespaceTrivia) ->
            let kindStr = function
            | SynModuleOrNamespaceKind.NamedModule -> "named module"
            | SynModuleOrNamespaceKind.AnonModule -> "anonymous module" 
            | SynModuleOrNamespaceKind.DeclaredNamespace -> "declared namespace"
            | SynModuleOrNamespaceKind.GlobalNamespace -> "global namespace"
            acc
            |> analyzeModuleDecls decls
    
    /// <summary>
    /// Analyzes a SynBinding with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynBinding which contains both SynPat and SynExpr
    /// </summary>
    /// <param name="binding">The F# binding syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeBinding (binding: SynBinding) (acc: Message list) : Message list =
        let analyzeExpressions = createListAnalyzer analyzeExpression
        let analyzeOptionalReturnInfo = createOptionAnalyzer analyzeReturnInfo
            
        match binding with
        | SynBinding(accessibility: SynAccess option, kind: SynBindingKind, isInline: bool, isMutable: bool, attrs: SynAttributes, xmlDoc: PreXmlDoc, valData: SynValData, headPat: SynPat, returnInfo: SynBindingReturnInfo option, expr: SynExpr, range: range, debugPoint: DebugPointAtBinding, trivia: SynBindingTrivia) ->
            acc
            |> analyzeExpressions (extractAttributes attrs |> List.map (fun attr -> attr.ArgExpr))
            |> analyzePattern headPat
            |> analyzeOptionalReturnInfo returnInfo
            |> analyzeExpression expr
    
    /// <summary>
    /// Analyzes a SynInterfaceImpl with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynInterfaceImpl containing SynType and SynBinding list
    /// </summary>
    /// <param name="interfaceImpl">The F# interface implementation syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeSynInterfaceImpl (interfaceImpl: SynInterfaceImpl) (acc: Message list) : Message list =
        let analyzeBindings = createListAnalyzer analyzeBinding
        
        match interfaceImpl with
        | SynInterfaceImpl(interfaceTy: SynType, withKeyword: range option, bindings: SynBinding list, members: SynMemberDefns, range: range) ->
            acc
            |> analyzeType interfaceTy
            |> analyzeBindings bindings
    
    /// <summary>
    /// Analyzes a SynBindingReturnInfo with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynBindingReturnInfo containing SynType and SynAttributes
    /// </summary>
    /// <param name="returnInfo">The F# binding return info syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeReturnInfo (returnInfo: SynBindingReturnInfo) (acc: Message list) : Message list =
        let analyzeExpressions = createListAnalyzer analyzeExpression
            
        match returnInfo with
        | SynBindingReturnInfo(typeName: SynType, range: range, attributes: SynAttributes, trivia: SynBindingReturnInfoTrivia) ->
            acc
            |> analyzeType typeName
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
                        let analyzeContents = createListAnalyzer analyzeSynModuleOrNamespace
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