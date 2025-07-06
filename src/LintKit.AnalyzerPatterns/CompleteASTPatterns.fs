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

    /// <summary>
    /// State type for stateful AST analysis with generic user state
    /// </summary>
    type State<'TState> = {
        Messages: Message list
        UserState: 'TState
    }

    /// <summary>
    /// Creates initial state with empty messages and provided user state
    /// </summary>
    let createState<'TState> (initialUserState: 'TState) : State<'TState> =
        { Messages = []; UserState = initialUserState }

    /// <summary>
    /// Adds a message to the current state
    /// </summary>
    let addMessage<'TState> (message: Message) (state: State<'TState>) : State<'TState> =
        { state with Messages = message :: state.Messages }

    let flip f x y = f y x
    let trd (_, _, a) = a
    let createListAnalyzer<'a, 'TState> (analyzer: 'a -> State<'TState> -> State<'TState>) = flip (List.fold (flip analyzer))
    let createOptionAnalyzer<'a, 'TState> (analyzer: 'a -> State<'TState> -> State<'TState>) = Option.map analyzer >> Option.defaultValue id

    /// <summary>
    /// Analyzes a SynType with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynType cases - use this as your template
    /// </summary>
    /// <param name="synType">The F# type syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzeType<'TState> (synType: SynType) (state: State<'TState>) : State<'TState> =
        let analyzeTypes = createListAnalyzer analyzeType
        let analyzeExpressions = createListAnalyzer analyzeExpression
        
        match synType with
        
        // === LONG IDENTIFIER TYPES ===
        | SynType.LongIdent(longDotId: SynLongIdent) ->
            state
        
        // === TYPE APPLICATIONS ===
        | SynType.App(typeName: SynType, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, isPostfix: bool, range: range) ->            
            state
            |> analyzeType typeName
            |> analyzeTypes typeArgs
        
        // === FUNCTION TYPES ===
        | SynType.Fun(argType: SynType, returnType: SynType, range: range, trivia: SynTypeFunTrivia) ->
            state
            |> analyzeType argType
            |> analyzeType returnType
        
        // === TUPLE TYPES ===
        | SynType.Tuple(isStruct: bool, path: SynTupleTypeSegment list, range: range) ->
            state
            |> analyzeTypes (path |> List.choose (function
                | SynTupleTypeSegment.Type(typeName: SynType) -> Some typeName
                | SynTupleTypeSegment.Star(_) -> None
                | SynTupleTypeSegment.Slash(_) -> None))
        
        // === ARRAY TYPES ===
        | SynType.Array(rank: int, elementType: SynType, range: range) ->
            state
            |> analyzeType elementType
        
        // === TYPE VARIABLES ===
        | SynType.Var(typar: SynTypar, range: range) ->
            state
        
        // === ANONYMOUS RECORD TYPES ===
        | SynType.AnonRecd(isStruct: bool, fields: (Ident * SynType) list, range: range) ->            
            // IDENTIFIER EXTRACTION: Anonymous record field names from fields |> List.map fst to get Ident list, use idText for field names
            // CUSTOM RULE EXAMPLES: Field naming conventions, required field validation, type consistency checks
            // ACCESS PATTERN: fields |> List.map (fun (fieldName, fieldType) -> fieldName.idText, fieldType)
            state
            |> analyzeTypes (fields |> List.map snd)
        
        // === LONG IDENTIFIER APP ===
        | SynType.LongIdentApp(typeName: SynType, longDotId: SynLongIdent, lessRange: range option, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, range: range) ->            
            state
            |> analyzeType typeName
            |> analyzeTypes typeArgs
        
        // === OTHER TYPE PATTERNS ===
        | SynType.Anon(range: range) ->
            state
        
        | SynType.StaticConstant(constant: SynConst, range: range) ->
            state
        
        | SynType.StaticConstantExpr(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynType.StaticConstantNamed(ident: SynType, value: SynType, range: range) ->
            state
            |> analyzeType ident
            |> analyzeType value
        
        | SynType.WithGlobalConstraints(typeName: SynType, constraints: SynTypeConstraint list, range: range) ->
            state
            |> analyzeType typeName
        
        | SynType.HashConstraint(innerType: SynType, range: range) ->
            state
            |> analyzeType innerType
        
        | SynType.MeasurePower(baseMeasure: SynType, exponent: SynRationalConst, range: range) ->
            state
            |> analyzeType baseMeasure
        
        | SynType.StaticConstantNull(range: range) ->
            state
        
        | SynType.Paren(innerType: SynType, range: range) ->
            state
            |> analyzeType innerType
        
        | SynType.WithNull(innerType: SynType, ambivalent: bool, range: range, trivia: SynTypeWithNullTrivia) ->
            state
            |> analyzeType innerType
        
        | SynType.SignatureParameter(attributes: SynAttributes, optional: bool, paramId: Ident option, usedType: SynType, range: range) ->
            state
            |> analyzeExpressions (attributes |> List.collect _.Attributes |> List.map _.ArgExpr)
            |> analyzeType usedType
        
        | SynType.Or(lhsType: SynType, rhsType: SynType, range: range, trivia: SynTypeOrTrivia) ->
            state
            |> analyzeType lhsType
            |> analyzeType rhsType
        
        | SynType.Intersection(typar: SynTypar option, types: SynType list, range: range, trivia: SynTyparDeclTrivia) ->            
            state
            |> analyzeTypes types
        
        | SynType.FromParseError(range: range) ->
            state
    
    /// <summary>
    /// Analyzes a SynPat with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynPat cases - use this as your template
    /// </summary>
    /// <param name="pat">The F# pattern syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzePattern<'TState> (pat: SynPat) (state: State<'TState>) : State<'TState> =
        let analyzePatterns = createListAnalyzer analyzePattern
        let analyzeArgPats (argPats: SynArgPats) (state: State<'TState>) : State<'TState> = 
            match argPats with
            | SynArgPats.Pats(pats: SynPat list) ->
                state
                |> analyzePatterns pats
            | SynArgPats.NamePatPairs(pats: (Ident * range option * SynPat) list, range: range, trivia: SynArgPatsNamePatPairsTrivia) ->
                state
                |> analyzePatterns (pats |> List.map trd)
        
        match pat with
        
        // === BASIC PATTERNS ===
        | SynPat.Const(constant: SynConst, range: range) ->
            state
        
        | SynPat.Wild(range: range) ->
            state
        
        | SynPat.Named(ident: SynIdent, isThisVal: bool, accessibility: SynAccess option, range: range) ->
            // IDENTIFIER EXTRACTION: Variable/function name from ident.idText, position from ident.idRange
            // CUSTOM RULE EXAMPLES: Naming convention checks, forbidden name validation, camelCase/PascalCase verification
            // ACCESS PATTERN: let name = ident.idText; let range = ident.idRange
            state
        
        | SynPat.Typed(pat: SynPat, targetType: SynType, range: range) ->
            state
            |> analyzeType targetType
            |> analyzePattern pat
        
        // === COLLECTION PATTERNS ===
        | SynPat.Tuple(isStruct: bool, elementPats: SynPat list, commaRanges: range list, range: range) ->
            state
            |> analyzePatterns elementPats
        
        | SynPat.ArrayOrList(isArray: bool, elementPats: SynPat list, range: range) ->
            let patType = if isArray then "array" else "list"
            state
            |> analyzePatterns elementPats
        
        | SynPat.Record(fieldPats: ((LongIdent * Ident) * range option * SynPat) list, range: range) ->            
            // IDENTIFIER EXTRACTION: Record field names during pattern matching from fieldPats |> List.map (fun ((longId, fieldName), _, _) -> longId, fieldName.idText)
            // CUSTOM RULE EXAMPLES: Record destructuring patterns, field access validation, required field checks
            // ACCESS PATTERN: fieldPats |> List.map (fun ((qualifiedName, field), _, pattern) -> qualifiedName |> List.map (fun i -> i.idText), field.idText, pattern)
            state
            |> analyzePatterns (fieldPats |> List.map trd)

        // === IDENTIFIER PATTERNS ===
        | SynPat.LongIdent(longDotId: SynLongIdent, extraId: Ident option, typarDecls: SynValTyparDecls option, argPats: SynArgPats, accessibility: SynAccess option, range: range) ->
            // IDENTIFIER EXTRACTION: Qualified/function names from longDotId.LongIdent as Ident list, use idText for each element
            // CUSTOM RULE EXAMPLES: Module qualified name checks, function naming rule validation
            // ACCESS PATTERN: longDotId.LongIdent |> List.map (fun id -> id.idText)
            state
            |> analyzeArgPats argPats
        
        | SynPat.Paren(pat: SynPat, range: range) ->
            state
            |> analyzePattern pat
        
        // === ADVANCED PATTERNS ===
        | SynPat.Attrib(pat: SynPat, attributes: SynAttributes, range: range) ->
            state
            |> analyzePattern pat
        
        | SynPat.Or(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia: SynPatOrTrivia) ->
            state
            |> analyzePattern lhsPat
            |> analyzePattern rhsPat
        
        | SynPat.ListCons(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia: SynPatListConsTrivia) ->
            state
            |> analyzePattern lhsPat
            |> analyzePattern rhsPat
        
        | SynPat.Ands(pats: SynPat list, range: range) ->
            state
            |> analyzePatterns pats
        
        | SynPat.As(lhsPat: SynPat, rhsPat: SynPat, range: range) ->
            state
            |> analyzePattern lhsPat
            |> analyzePattern rhsPat
        
        | SynPat.Null(range: range) ->
            state
        
        | SynPat.OptionalVal(ident: Ident, range: range) ->
            // IDENTIFIER EXTRACTION: Optional value name from ident.idText for optional variable names
            // CUSTOM RULE EXAMPLES: Optional value naming conventions, optional pattern usage validation
            // ACCESS PATTERN: let optionalVarName = ident.idText; let range = ident.idRange
            state
        
        | SynPat.IsInst(targetType: SynType, range: range) ->
            state
            |> analyzeType targetType
        
        | SynPat.QuoteExpr(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynPat.InstanceMember(thisId: Ident, memberId: Ident, toolId: Ident option, accessibility: SynAccess option, range: range) ->
            // IDENTIFIER EXTRACTION: Instance member names from thisId.idText for this name, memberId.idText for member name
            // CUSTOM RULE EXAMPLES: Instance member naming conventions, this reference patterns, accessibility checks
            // ACCESS PATTERN: let thisName = thisId.idText; let memberName = memberId.idText
            state
        
        | SynPat.FromParseError(pat: SynPat, range: range) ->
            state
            |> analyzePattern pat
    
    /// <summary>
    /// Recursively analyzes SynExpr with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL 73 SynExpr cases - use this as your template
    /// </summary>
    /// <param name="expr">Expression to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>Updated accumulator with messages from this expression and all sub-expressions</returns>
    and analyzeExpression<'TState> (expr: SynExpr) (state: State<'TState>) : State<'TState> =
        let analyzeOptionalExpression = createOptionAnalyzer analyzeExpression
        let analyzeExpressions = createListAnalyzer analyzeExpression
        let analyzeTypes = createListAnalyzer analyzeType
        let analyzeBindings = createListAnalyzer analyzeBinding
        let analyzeInterfaceImpls = createListAnalyzer analyzeSynInterfaceImpl
        let analyzePatterns = createListAnalyzer analyzePattern
        
        match expr with
        
        // === BASIC CASES ===
        | SynExpr.Paren(expr: SynExpr, leftParenRange: range, rightParenRange: range option, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.Quote(operator: SynExpr, isRaw: bool, quotedExpr: SynExpr, isFromQueryExpression: bool, range: range) ->
            state
            |> analyzeExpression operator
            |> analyzeExpression quotedExpr
        
        | SynExpr.Const(constant: SynConst, range: range) ->
            state
        
        | SynExpr.Typed(expr: SynExpr, targetType: SynType, range: range) ->
            state
            |> analyzeExpression expr
            |> analyzeType targetType
        
        | SynExpr.Tuple(isStruct: bool, exprs: SynExpr list, commaRanges: range list, range: range) ->
            let analyzeExprs = createListAnalyzer analyzeExpression
            
            state
            |> analyzeExprs exprs
        
        | SynExpr.AnonRecd(isStruct: bool, copyInfo: (SynExpr * BlockSeparator) option, recordFields: (SynLongIdent * range option * SynExpr) list, range: range, trivia: SynExprAnonRecdTrivia) ->            
            // IDENTIFIER EXTRACTION: Anonymous record field names during creation from recordFields |> List.map (fun (field, _, _) -> field.LongIdent)
            // CUSTOM RULE EXAMPLES: Record creation patterns, field initialization validation, invalid value checks
            // ACCESS PATTERN: recordFields |> List.map (fun (fieldName, _, value) -> fieldName.LongIdent |> List.map (fun i -> i.idText), value)
            state
            |> analyzeOptionalExpression (copyInfo |> Option.map fst)
            |> analyzeExpressions (recordFields |> List.map trd)
        
        | SynExpr.ArrayOrList(isArray: bool, exprs: SynExpr list, range: range) ->
            state
            |> analyzeExpressions exprs
        
        | SynExpr.Record(baseInfo: (SynType * SynExpr * range * BlockSeparator option * range) option, copyInfo: (SynExpr * BlockSeparator) option, recordFields: SynExprRecordField list, range: range) ->
            // IDENTIFIER EXTRACTION: Record field names during creation from recordFields SynExprRecordField(field, _, _, _)
            // CUSTOM RULE EXAMPLES: Record update patterns, field assignment validation, copy syntax checks
            // ACCESS PATTERN: recordFields |> List.map (function SynExprRecordField((fieldName, _), _, value, _) -> fieldName.LongIdent |> List.map (fun i -> i.idText), value)
            state
            |> analyzeOptionalExpression (baseInfo |> Option.map (fun (_, expr, _, _, _) -> expr))
            |> analyzeOptionalExpression (copyInfo |> Option.map fst)
            |> analyzeExpressions (recordFields |> List.choose (function SynExprRecordField(_, _, expr, _) -> expr))
        
        | SynExpr.New(isProtected: bool, targetType: SynType, expr: SynExpr, range: range) ->
            state
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.ObjExpr(objType: SynType, argOptions: (SynExpr * Ident option) option, withKeyword: range option, bindings: SynBinding list, members: SynMemberDefns, extraImpls: SynInterfaceImpl list, newExprRange: range, range: range) ->            
            state
            |> analyzeType objType
            |> analyzeOptionalExpression (argOptions |> Option.map fst)
            |> analyzeBindings bindings
            |> analyzeInterfaceImpls extraImpls
        
        // === CONTROL FLOW ===
        | SynExpr.While(whileDebugPoint: DebugPointAtWhile, whileExpr: SynExpr, doExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression whileExpr
            |> analyzeExpression doExpr
        
        | SynExpr.For(forDebugPoint: DebugPointAtFor, toDebugPoint: DebugPointAtInOrTo, ident: Ident, equalsRange: range option, identBody: SynExpr, direction: bool, toBody: SynExpr, doBody: SynExpr, range: range) ->
            // IDENTIFIER EXTRACTION: For loop variable name from ident.idText, position from ident.idRange
            // CUSTOM RULE EXAMPLES: Loop variable naming conventions, variable scope checks, unused variable detection
            // ACCESS PATTERN: let loopVar = ident.idText; let range = ident.idRange
            state
            |> analyzeExpression identBody
            |> analyzeExpression toBody
            |> analyzeExpression doBody
        
        | SynExpr.ForEach(forDebugPoint: DebugPointAtFor, inDebugPoint: DebugPointAtInOrTo, seqExprOnly: SeqExprOnly, isFromSource: bool, pat: SynPat, enumExpr: SynExpr, bodyExpr: SynExpr, range: range) ->
            state
            |> analyzePattern pat
            |> analyzeExpression enumExpr
            |> analyzeExpression bodyExpr
        
        | SynExpr.ArrayOrListComputed(isArray: bool, expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.IndexRange(expr1: SynExpr option, opm: range, expr2: SynExpr option, range1: range, range2: range, range: range) ->
            state
            |> analyzeOptionalExpression expr1
            |> analyzeOptionalExpression expr2
        
        | SynExpr.IndexFromEnd(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.ComputationExpr(hasSeqBuilder: bool, expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.Lambda(fromMethod: bool, inLambdaSeq: bool, args: SynSimplePats, body: SynExpr, parsedData: (SynPat list * SynExpr) option, range: range, trivia: SynExprLambdaTrivia) ->
            let analyzeParsedData =
                let analyzeParsedData (patterns, expr) state =
                    state
                    |> analyzePatterns patterns
                    |> analyzeExpression expr
                createOptionAnalyzer analyzeParsedData
            
            state
            |> analyzeParsedData parsedData
            |> analyzeExpression body
        
        | SynExpr.MatchLambda(isExnMatch: bool, keywordRange: range, matchClauses: SynMatchClause list, matchDebugPoint: DebugPointAtBinding, range: range) ->
            state
            |> analyzePatterns (matchClauses |> List.map (function SynMatchClause(pat, _, _, _, _, _) -> pat))
            |> analyzeExpressions (matchClauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.Match(matchDebugPoint: DebugPointAtBinding, expr: SynExpr, clauses: SynMatchClause list, range: range, trivia: SynExprMatchTrivia) ->
            state
            |> analyzeExpression expr
            |> analyzePatterns (clauses |> List.map (function SynMatchClause(pat, _, _, _, _, _) -> pat))
            |> analyzeExpressions (clauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.Do(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.Assert(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        // === FUNCTION APPLICATION ===
        | SynExpr.App(flag: ExprAtomicFlag, isInfix: bool, funcExpr: SynExpr, argExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression funcExpr
            |> analyzeExpression argExpr
        
        | SynExpr.TypeApp(expr: SynExpr, lessRange: range, typeArgs: SynType list, commaRanges: range list, greaterRange: range option, typeArgsRange: range, range: range) ->            
            state
            |> analyzeExpression expr
            |> analyzeTypes typeArgs
        
        // === BINDINGS ===
        | SynExpr.LetOrUse(isRecursive: bool, isUse: bool, bindings: SynBinding list, body: SynExpr, range: range, trivia: SynExprLetOrUseTrivia) ->
            state
            |> analyzeBindings bindings
            |> analyzeExpression body
        
        // === ERROR HANDLING ===
        | SynExpr.TryWith(tryExpr: SynExpr, withCases: SynMatchClause list, range: range, tryDebugPoint: DebugPointAtTry, withDebugPoint: DebugPointAtWith, trivia: SynExprTryWithTrivia) ->
            state
            |> analyzeExpression tryExpr
            |> analyzeExpressions (withCases |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.TryFinally(tryExpr: SynExpr, finallyExpr: SynExpr, range: range, tryDebugPoint: DebugPointAtTry, finallyDebugPoint: DebugPointAtFinally, trivia: SynExprTryFinallyTrivia) ->
            state
            |> analyzeExpression tryExpr
            |> analyzeExpression finallyExpr
        
        | SynExpr.Lazy(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.Sequential(debugPoint: DebugPointAtSequential, isTrueSeq: bool, expr1: SynExpr, expr2: SynExpr, range: range, trivia: SynExprSequentialTrivia) ->
            state
            |> analyzeExpression expr1
            |> analyzeExpression expr2
        
        | SynExpr.IfThenElse(ifExpr: SynExpr, thenExpr: SynExpr, elseExpr: SynExpr option, spIfToThen: DebugPointAtBinding, isFromErrorRecovery: bool, range: range, trivia: SynExprIfThenElseTrivia) ->
            state
            |> analyzeExpression ifExpr
            |> analyzeExpression thenExpr
            |> analyzeOptionalExpression elseExpr
        
        // === IDENTIFIERS ===
        | SynExpr.Typar(typar: SynTypar, range: range) ->
            state
        
        | SynExpr.Ident(ident: Ident) ->
            // IDENTIFIER EXTRACTION: Identifier name from ident.idText, position from ident.idRange
            // CUSTOM RULE EXAMPLES: Variable/function name rule checks, forbidden name validation
            // ACCESS PATTERN: let name = ident.idText; let range = ident.idRange
            state
        
        | SynExpr.LongIdent(isOptional: bool, longDotId: SynLongIdent, altNameRefCell: SynSimplePatAlternativeIdInfo ref option, range: range) ->
            // IDENTIFIER EXTRACTION: Qualified identifier from longDotId.LongIdent as Ident list, use idText for qualified names
            // CUSTOM RULE EXAMPLES: Module reference checks, namespace usage validation, qualified naming rules
            // ACCESS PATTERN: let qualifiedName = longDotId.LongIdent |> List.map (fun i -> i.idText) |> String.concat "."
            state
        
        | SynExpr.LongIdentSet(longDotId: SynLongIdent, expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        // === MEMBER ACCESS ===
        | SynExpr.DotGet(expr: SynExpr, rangeOfDot: range, longDotId: SynLongIdent, range: range) ->
            // IDENTIFIER EXTRACTION: Member access names from longDotId.LongIdent for accessed member names
            // CUSTOM RULE EXAMPLES: Member access patterns, property usage validation, API design guidelines
            // ACCESS PATTERN: let memberName = longDotId.LongIdent |> List.map (fun i -> i.idText) |> String.concat "."
            state
            |> analyzeExpression expr
        
        | SynExpr.DotLambda(expr: SynExpr, range: range, trivia: SynExprDotLambdaTrivia) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.DotSet(targetExpr: SynExpr, longDotId: SynLongIdent, rhsExpr: SynExpr, range: range) ->
            // IDENTIFIER EXTRACTION: Member assignment names from longDotId.LongIdent for target member names
            // CUSTOM RULE EXAMPLES: Immutable field assignment detection, mutability checks, property setting patterns
            // ACCESS PATTERN: let memberName = longDotId.LongIdent |> List.map (fun i -> i.idText) |> String.concat "."
            state
            |> analyzeExpression targetExpr
            |> analyzeExpression rhsExpr
        
        | SynExpr.Set(targetExpr: SynExpr, rhsExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression targetExpr
            |> analyzeExpression rhsExpr
        
        | SynExpr.DotIndexedGet(objectExpr: SynExpr, indexArgs: SynExpr, dotRange: range, range: range) ->
            state
            |> analyzeExpression objectExpr
            |> analyzeExpression indexArgs
        
        | SynExpr.DotIndexedSet(objectExpr: SynExpr, indexArgs: SynExpr, valueExpr: SynExpr, leftOfSetRange: range, dotRange: range, range: range) ->
            state
            |> analyzeExpression objectExpr
            |> analyzeExpression indexArgs
            |> analyzeExpression valueExpr
        
        | SynExpr.NamedIndexedPropertySet(longDotId: SynLongIdent, expr1: SynExpr, expr2: SynExpr, range: range) ->
            state
            |> analyzeExpression expr1
            |> analyzeExpression expr2
        
        | SynExpr.DotNamedIndexedPropertySet(targetExpr: SynExpr, longDotId: SynLongIdent, argExpr: SynExpr, rhsExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression targetExpr
            |> analyzeExpression argExpr
            |> analyzeExpression rhsExpr
        
        // === TYPE OPERATIONS ===
        | SynExpr.TypeTest(expr: SynExpr, targetType: SynType, range: range) ->
            state
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.Upcast(expr: SynExpr, targetType: SynType, range: range) ->
            state
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.Downcast(expr: SynExpr, targetType: SynType, range: range) ->
            state
            |> analyzeType targetType
            |> analyzeExpression expr
        
        | SynExpr.InferredUpcast(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.InferredDowncast(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.Null(range: range) ->
            state
        
        | SynExpr.AddressOf(isByref: bool, expr: SynExpr, opRange: range, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.TraitCall(supportTys: SynType, traitSig: SynMemberSig, argExpr: SynExpr, range: range) ->
            state
            |> analyzeType supportTys
            |> analyzeExpression argExpr
        
        | SynExpr.JoinIn(lhsExpr: SynExpr, lhsRange: range, rhsExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression lhsExpr
            |> analyzeExpression rhsExpr
        
        | SynExpr.ImplicitZero(range: range) ->
            state
        
        // === COMPUTATION EXPRESSIONS ===
        | SynExpr.SequentialOrImplicitYield(debugPoint: DebugPointAtSequential, expr1: SynExpr, expr2: SynExpr, ifNotStmt: SynExpr, range: range) ->
            state
            |> analyzeExpression expr1
            |> analyzeExpression expr2
            |> analyzeExpression ifNotStmt
        
        | SynExpr.YieldOrReturn((flags1: bool, flags2: bool), expr: SynExpr, range: range, trivia: SynExprYieldOrReturnTrivia) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.YieldOrReturnFrom((flags1: bool, flags2: bool), expr: SynExpr, range: range, trivia: SynExprYieldOrReturnFromTrivia) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.LetOrUseBang(bindDebugPoint: DebugPointAtBinding, isUse: bool, isFromSource: bool, pat: SynPat, rhs: SynExpr, andBangs: SynExprAndBang list, body: SynExpr, range: range, trivia: SynExprLetOrUseBangTrivia) ->
            state
            |> analyzePattern pat
            |> analyzeExpression rhs
            |> analyzeExpressions (andBangs |> List.map (function SynExprAndBang(_, _, _, _, expr, _, _) -> expr))
            |> analyzeExpression body
        
        | SynExpr.MatchBang(matchDebugPoint: DebugPointAtBinding, expr: SynExpr, clauses: SynMatchClause list, range: range, trivia: SynExprMatchBangTrivia) ->
            state
            |> analyzeExpression expr
            |> analyzeExpressions (clauses |> List.map (function SynMatchClause(_, _, expr, _, _, _) -> expr))
        
        | SynExpr.DoBang(expr: SynExpr, range: range, trivia: SynExprDoBangTrivia) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.WhileBang(whileDebugPoint: DebugPointAtWhile, whileExpr: SynExpr, doExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression whileExpr
            |> analyzeExpression doExpr
        
        // === LIBRARY/COMPILER INTERNALS ===
        | SynExpr.LibraryOnlyILAssembly(ilCode: obj, typeArgs: SynType list, args: SynExpr list, retTy: SynType list, range: range) ->
            state
            |> analyzeTypes typeArgs
            |> analyzeExpressions args
            |> analyzeTypes retTy
        
        | SynExpr.LibraryOnlyStaticOptimization(constraints: SynStaticOptimizationConstraint list, expr: SynExpr, optimizedExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
            |> analyzeExpression optimizedExpr
        
        | SynExpr.LibraryOnlyUnionCaseFieldGet(expr: SynExpr, longId: LongIdent, fieldNum: int, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.LibraryOnlyUnionCaseFieldSet(expr: SynExpr, longId: LongIdent, fieldNum: int, rhsExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
            |> analyzeExpression rhsExpr
        
        // === ERROR RECOVERY ===
        | SynExpr.ArbitraryAfterError(debugStr: string, range: range) ->
            state
        
        | SynExpr.FromParseError(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.DiscardAfterMissingQualificationAfterDot(expr: SynExpr, dotRange: range, range: range) ->
            state
            |> analyzeExpression expr
        
        | SynExpr.Fixed(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
        
        // === STRING INTERPOLATION ===
        | SynExpr.InterpolatedString(contents: SynInterpolatedStringPart list, synStringKind: SynStringKind, range: range) ->
            state
            |> analyzeExpressions (contents |> List.choose (function 
                | SynInterpolatedStringPart.String(_, _) -> None
                | SynInterpolatedStringPart.FillExpr(expr, _) -> Some expr))
        
        // === DEBUG SUPPORT ===
        | SynExpr.DebugPoint(debugPoint: DebugPointAtLeafExpr, isControlFlow: bool, innerExpr: SynExpr) ->
            state
            |> analyzeExpression innerExpr
        
        | SynExpr.Dynamic(funcExpr: SynExpr, qmark: range, argExpr: SynExpr, range: range) ->
            state
            |> analyzeExpression funcExpr
            |> analyzeExpression argExpr
    
    /// <summary>
    /// Analyzes a SynModuleDecl with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers ALL SynModuleDecl cases - use this as your template
    /// </summary>
    /// <param name="decl">The F# module declaration syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeModuleDecl<'TState> (decl: SynModuleDecl) (state: State<'TState>) : State<'TState> =
        let analyzeModuleDecls = createListAnalyzer analyzeModuleDecl
        let analyzeBindings = createListAnalyzer analyzeBinding
        let analyzeExpressions = createListAnalyzer analyzeExpression
        
        match decl with
        
        // === LET BINDINGS ===
        | SynModuleDecl.Let(isRecursive: bool, bindings: SynBinding list, range: range) ->
            state
            |> analyzeBindings bindings
        
        // === TYPE DEFINITIONS ===
        | SynModuleDecl.Types(typeDefns: SynTypeDefn list, range: range) ->
            // IDENTIFIER EXTRACTION: Type names from typeDefns list, get names from SynComponentInfo.longId
            // TYPE KIND EXTRACTION: Use SynTypeDefnRepr to determine Record/Union/Class/Interface/Delegate
            // CUSTOM RULE EXAMPLES: Type naming conventions, type hierarchy checks, inheritance constraint validation, record/union structure validation
            // ACCESS PATTERN: typeDefns |> List.collect (fun (SynTypeDefn(ci, _, _, _, _, _)) -> match ci with SynComponentInfo(_, _, _, longId, _, _, _, _) -> longId)
            // TYPE DEFINITION DETAILS: SynTypeDefn(componentInfo, typeRepr, members, implicitCtor, range, trivia)
            state
        
        // === EXCEPTION DEFINITIONS ===
        | SynModuleDecl.Exception(exnDefn: SynExceptionDefn, range: range) ->
            // IDENTIFIER EXTRACTION: Exception names from exnDefn.Exception.SynComponentInfo.longId
            // EXCEPTION FIELD EXTRACTION: Exception fields from exnDefn.Exception.SynUnionCase.fields
            // CUSTOM RULE EXAMPLES: Exception naming conventions, exception hierarchy checks, custom exception structure validation
            // ACCESS PATTERN: match exnDefn with SynExceptionDefn(SynExceptionDefnRepr(_, union, _, _, _, _), _, _, _, _, _) -> union
            state
        
        // === OPEN STATEMENTS ===
        | SynModuleDecl.Open(target: SynOpenDeclTarget, range: range) ->
            // IDENTIFIER EXTRACTION: Import names from target for opened module/namespace names
            // CUSTOM RULE EXAMPLES: Unnecessary import detection, import ordering checks, namespace usage rules
            // ACCESS PATTERN: match target with SynOpenDeclTarget.ModuleOrNamespace(longId, _) -> longId |> List.map (fun i -> i.idText)
            state
        
        // === MODULE DECLARATIONS ===
        | SynModuleDecl.ModuleAbbrev(ident: Ident, longId: LongIdent, range: range) ->
            // IDENTIFIER EXTRACTION: Module alias from ident.idText for alias, longId for full path
            // CUSTOM RULE EXAMPLES: Module alias naming conventions, alias usage pattern validation
            // ACCESS PATTERN: let alias = ident.idText; let fullPath = longId |> List.map (fun i -> i.idText)
            state
        
        | SynModuleDecl.NestedModule(componentInfo: SynComponentInfo, isRecursive: bool, decls: SynModuleDecl list, isContinuing: bool, range: range, trivia: SynModuleDeclNestedModuleTrivia) ->
            // IDENTIFIER EXTRACTION: Nested module names from componentInfo.longId
            // RECURSIVE MODULE DETECTION: Use isRecursive to determine mutual recursive modules
            // CUSTOM RULE EXAMPLES: Nested module naming conventions, module hierarchy checks, recursive module validation
            // ACCESS PATTERN: match componentInfo with SynComponentInfo(_, _, _, longId, _, _, _, _) -> longId |> List.map (fun i -> i.idText)
            state
            |> analyzeModuleDecls decls
        
        // === ATTRIBUTES ===
        | SynModuleDecl.Attributes(attributes: SynAttributes, range: range) ->
            state
            |> analyzeExpressions (attributes |> List.collect _.Attributes |> List.map _.ArgExpr)
        
        // === HASH DIRECTIVES ===
        | SynModuleDecl.HashDirective(hashDirective: ParsedHashDirective, range: range) ->
            state
        
        // === NAMESPACE FRAGMENT ===
        | SynModuleDecl.NamespaceFragment(moduleOrNamespace: SynModuleOrNamespace) ->
            state
            |> analyzeSynModuleOrNamespace moduleOrNamespace
        
        // === STANDALONE EXPRESSIONS ===
        | SynModuleDecl.Expr(expr: SynExpr, range: range) ->
            state
            |> analyzeExpression expr
    
    /// <summary>
    /// Analyzes a SynModuleOrNamespace with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This covers SynModuleOrNamespace cases - use this as your template
    /// </summary>
    /// <param name="moduleOrNs">The F# module or namespace syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeSynModuleOrNamespace<'TState> (moduleOrNs: SynModuleOrNamespace) (state: State<'TState>) : State<'TState> =
        let analyzeModuleDecls = createListAnalyzer analyzeModuleDecl
        
        match moduleOrNs with
        | SynModuleOrNamespace(longId: LongIdent, isRecursive: bool, kind: SynModuleOrNamespaceKind, decls: SynModuleDecl list, xmlDoc: PreXmlDoc, attribs: SynAttributes, accessibility: SynAccess option, range: range, trivia: SynModuleOrNamespaceTrivia) ->
            // IDENTIFIER EXTRACTION: Module/namespace names from longId name list
            // MODULE KIND DETECTION: Use kind to determine NamedModule/DeclaredNamespace etc.
            // CUSTOM RULE EXAMPLES: Module naming conventions, namespace structure checks, attribute validation
            // ACCESS PATTERN: let names = longId |> List.map (fun id -> id.idText)
            let kindStr = function
            | SynModuleOrNamespaceKind.NamedModule -> "named module"
            | SynModuleOrNamespaceKind.AnonModule -> "anonymous module" 
            | SynModuleOrNamespaceKind.DeclaredNamespace -> "declared namespace"
            | SynModuleOrNamespaceKind.GlobalNamespace -> "global namespace"
            state
            |> analyzeModuleDecls decls
    
    /// <summary>
    /// Analyzes a SynBinding with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynBinding which contains both SynPat and SynExpr
    /// </summary>
    /// <param name="binding">The F# binding syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeBinding<'TState> (binding: SynBinding) (state: State<'TState>) : State<'TState> =
        let analyzeExpressions = createListAnalyzer analyzeExpression
        let analyzeOptionalReturnInfo = createOptionAnalyzer analyzeBindingReturnInfo
            
        match binding with
        | SynBinding(accessibility: SynAccess option, kind: SynBindingKind, isInline: bool, isMutable: bool, attrs: SynAttributes, xmlDoc: PreXmlDoc, valData: SynValData, headPat: SynPat, returnInfo: SynBindingReturnInfo option, expr: SynExpr, range: range, debugPoint: DebugPointAtBinding, trivia: SynBindingTrivia) ->
            // IDENTIFIER EXTRACTION: Function names from headPat extraction (SynPat.Named or SynPat.LongIdent)
            // ATTRIBUTE EXTRACTION: Attribute information from attrs [<Attribute>] data
            // CUSTOM RULE EXAMPLES: Function naming conventions, required attribute checks, accessibility validation
            // ACCESS PATTERN: let attrs = attrs |> List.collect _.Attributes
            state
            |> analyzeExpressions (attrs |> List.collect _.Attributes |> List.map _.ArgExpr)
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
    and analyzeSynInterfaceImpl<'TState> (interfaceImpl: SynInterfaceImpl) (state: State<'TState>) : State<'TState> =
        let analyzeBindings = createListAnalyzer analyzeBinding
        
        match interfaceImpl with
        | SynInterfaceImpl(interfaceTy: SynType, withKeyword: range option, bindings: SynBinding list, members: SynMemberDefns, range: range) ->
            state
            |> analyzeType interfaceTy
            |> analyzeBindings bindings
    
    /// <summary>
    /// Analyzes a SynBindingReturnInfo with complete pattern matching using accumulator pattern
    /// **AI CRITICAL PATTERN**: This handles SynBindingReturnInfo containing SynType and SynAttributes
    /// </summary>
    /// <param name="returnInfo">The F# binding return info syntax tree node to analyze</param>
    /// <param name="acc">Accumulator for collecting messages</param>
    /// <returns>List of analysis messages for any issues found</returns>
    and analyzeBindingReturnInfo<'TState> (returnInfo: SynBindingReturnInfo) (state: State<'TState>) : State<'TState> =
        let analyzeExpressions = createListAnalyzer analyzeExpression
            
        match returnInfo with
        | SynBindingReturnInfo(typeName: SynType, range: range, attributes: SynAttributes, trivia: SynBindingReturnInfoTrivia) ->
            state
            |> analyzeType typeName
            |> analyzeExpressions (attributes |> List.collect _.Attributes |> List.map _.ArgExpr)
    
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
                        let initialState = createState ()
                        let finalState = analyzeContents contents initialState
                        return finalState.Messages |> List.rev
                    | ParsedInput.SigFile _ ->
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