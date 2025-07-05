namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text

/// <summary>
/// Complete SynPat pattern matching reference with full type annotations
/// 
/// **AI Learning Reference**: This demonstrates how to:
/// - Handle ALL SynPat patterns with complete pattern matching
/// - Use consistent message creation for all pattern constructs
/// - Apply explicit type annotations for AI learning
/// 
/// **For AI Agents**: This is the definitive pattern for SynPat analysis.
/// Copy this approach when you need to analyze F# pattern matching constructs.
/// **For Humans**: Reference for building pattern matching analyzers.
/// 
/// SynPat represents F# patterns used in match expressions, let bindings, etc.
/// </summary>
module SynPatPatterns =
    
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
    /// Analyzes a SynPat with complete pattern matching
    /// **AI CRITICAL PATTERN**: This covers ALL SynPat cases - use this as your template
    /// </summary>
    /// <param name="pat">The F# pattern syntax tree node to analyze</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzePattern (pat: SynPat) : Message list =
        match pat with
        
        // === BASIC PATTERNS ===
        | SynPat.Const(constant: SynConst, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Const" range $"constant pattern: {constant}"
            [nodeMsg]
        
        | SynPat.Wild(range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Wild" range "wildcard pattern (_)"
            [nodeMsg]
        
        | SynPat.Named(ident: SynIdent, isThisVal: bool, accessibility: SynAccess option, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Named" range $"named pattern (isThis: {isThisVal})"
            [nodeMsg]
        
        | SynPat.Typed(pat: SynPat, targetType: SynType, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Typed" range "typed pattern"
            let patMessages = analyzePattern pat
            nodeMsg :: patMessages
        
        // === COLLECTION PATTERNS ===
        | SynPat.Tuple(isStruct: bool, elementPats: SynPat list, commaRanges: range list, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Tuple" range $"tuple pattern (struct: {isStruct}, elements: {elementPats.Length})"
            let patMessages = elementPats |> List.collect analyzePattern
            nodeMsg :: patMessages
        
        | SynPat.ArrayOrList(isArray: bool, elementPats: SynPat list, range: range) ->
            let patType = if isArray then "array" else "list"
            let nodeMsg = createPatternVisitMessage "SynPat.ArrayOrList" range $"{patType} pattern (elements: {elementPats.Length})"
            let patMessages = elementPats |> List.collect analyzePattern
            nodeMsg :: patMessages
        
        | SynPat.Record(fieldPats: ((LongIdent * Ident) * range option * SynPat) list, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Record" range $"record pattern (fields: {fieldPats.Length})"
            let patMessages = fieldPats |> List.collect (fun (_, _, pat) -> analyzePattern pat)
            nodeMsg :: patMessages
        
        // === IDENTIFIER PATTERNS ===
        | SynPat.LongIdent(longDotId: SynLongIdent, extraId: Ident option, typarDecls: SynValTyparDecls option, argPats: SynArgPats, accessibility: SynAccess option, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.LongIdent" range "long identifier pattern"
            [nodeMsg]
        
        | SynPat.Paren(pat: SynPat, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Paren" range "parenthesized pattern"
            let patMessages = analyzePattern pat
            nodeMsg :: patMessages
        
        // === ADVANCED PATTERNS ===
        | SynPat.Attrib(pat: SynPat, attributes: SynAttributes, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Attrib" range $"attributed pattern (attributes: {attributes.Length})"
            let patMessages = analyzePattern pat
            nodeMsg :: patMessages
        
        | SynPat.Or(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Or" range "or pattern (|)"
            let lhsMessages = analyzePattern lhsPat
            let rhsMessages = analyzePattern rhsPat
            nodeMsg :: (lhsMessages @ rhsMessages)
        
        | SynPat.ListCons(lhsPat: SynPat, rhsPat: SynPat, range: range, trivia: SynPatListConsTrivia) ->
            let nodeMsg = createPatternVisitMessage "SynPat.ListCons" range "list cons pattern (::)"
            let lhsMessages = analyzePattern lhsPat
            let rhsMessages = analyzePattern rhsPat
            nodeMsg :: (lhsMessages @ rhsMessages)
        
        | SynPat.Ands(pats: SynPat list, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Ands" range $"and pattern (&) with {pats.Length} patterns"
            let patMessages = pats |> List.collect analyzePattern
            nodeMsg :: patMessages
        
        | SynPat.As(lhsPat: SynPat, rhsPat: SynPat, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.As" range "as pattern"
            let lhsMessages = analyzePattern lhsPat
            let rhsMessages = analyzePattern rhsPat
            nodeMsg :: (lhsMessages @ rhsMessages)
        
        | SynPat.Null(range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.Null" range "null pattern"
            [nodeMsg]
        
        | SynPat.OptionalVal(ident: Ident, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.OptionalVal" range "optional value pattern"
            [nodeMsg]
        
        | SynPat.IsInst(targetType: SynType, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.IsInst" range "type test pattern (:? type)"
            [nodeMsg]
        
        | SynPat.QuoteExpr(expr: SynExpr, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.QuoteExpr" range "quoted expression pattern"
            [nodeMsg]
        
        | SynPat.InstanceMember(thisId: Ident, memberId: Ident, toolId: Ident option, accessibility: SynAccess option, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.InstanceMember" range "instance member pattern"
            [nodeMsg]
        
        | SynPat.FromParseError(pat: SynPat, range: range) ->
            let nodeMsg = createPatternVisitMessage "SynPat.FromParseError" range "pattern from parse error"
            let patMessages = analyzePattern pat
            nodeMsg :: patMessages
    
    /// <summary>
    /// Sample analyzer that uses the SynPat pattern matching
    /// This demonstrates how to integrate the pattern analysis into a complete analyzer
    /// **AI PATTERN**: Use this structure for your own pattern matching analyzers
    /// </summary>
    [<CliAnalyzer>]
    let patternPatternAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    // Note: This is a simplified example. Full pattern analysis would require
                    // traversing the entire AST to find patterns in match expressions, let bindings, etc.
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
                                            | SynBinding(_, _, _, _, _, _, _, pat, _, _, _, _, _) ->
                                                let patMessages = analyzePattern pat
                                                messages.AddRange patMessages
                                    | _ -> ()
                    | ParsedInput.SigFile(_) ->
                        ()
                        
                with
                | ex ->
                    messages.Add({
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Error analyzing patterns: {ex.Message}"
                        Code = "SYNPAT999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }