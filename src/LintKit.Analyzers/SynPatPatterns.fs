namespace LintKit.Analyzers

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text

/// <summary>
/// Complete SynPat pattern matching reference for AI agents
/// 
/// This module demonstrates how to handle SynPat cases with:
/// - Detailed type annotations for every parameter
/// - Explicit pattern matching for AI learning
/// - Proper analysis of F# pattern constructs
/// - Appropriate message creation with different severities
/// 
/// Learning points for AI agents:
/// - How to destructure each SynPat variant
/// - When to recurse into nested patterns
/// - How to extract meaningful information from pattern structures
/// - Proper range and location handling
/// 
/// SynPat represents patterns in F# code, such as:
/// - Variable patterns in let bindings and function parameters
/// - Pattern matching cases in match expressions
/// - Destructuring patterns for tuples, records, lists
/// - Wildcard and constant patterns
/// 
/// This covers all 21 SynPat patterns available in F# AST.
/// </summary>
module SynPatPatterns =
    
    /// <summary>
    /// Analyzes a SynPat and returns any issues found
    /// </summary>
    /// <param name="pattern">The F# pattern syntax tree node to analyze</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzePattern (pattern: SynPat) : Message list =
        match pattern with
        
        // === NAMED PATTERNS ===
        | SynPat.Named(ident: SynIdent,
                      isThisVal: bool,
                      accessibility: SynAccess option,
                      range: range) ->
            // Handle named patterns: x, value, result
            // Type annotations:
            // - ident: the identifier being bound (SynIdent type)
            // - isThisVal: true if this is a 'this' parameter
            // - accessibility: access modifier (public, private, etc.)
            // - range: location of the pattern
            
            let identName = match ident with | SynIdent(id, _) -> id.idText
            
            // Example analysis: detect poor variable naming
            let namingMessage: Message option =
                if identName.Length = 1 && identName <> "_" && not (System.Char.IsUpper(identName.[0])) then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Single-character variable name '{identName}' may be unclear. Consider a more descriptive name."
                        Code = "SPP001"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                elif identName.StartsWith("_") && identName.Length > 1 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Variable '{identName}' starts with underscore but is named. Consider using just '_' if unused."
                        Code = "SPP002"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: detect 'this' parameter usage
            let thisMessage: Message option =
                if isThisVal then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Using 'this' parameter - ensure object-oriented design is appropriate for F#"
                        Code = "SPP003"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [namingMessage; thisMessage]
                |> List.choose id
            
            analysisMessages
        
        // === WILDCARD PATTERNS ===
        | SynPat.Wild(range: range) ->
            // Handle wildcard patterns: _
            // Type annotations:
            // - range: location of the wildcard
            
            // Example analysis: suggest when wildcards might be overused
            let wildcardMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = "Wildcard pattern detected - ensure you're not ignoring important values"
                    Code = "SPP004"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [wildcardMessage]
                |> List.choose id
            
            analysisMessages
        
        // === CONSTANT PATTERNS ===
        | SynPat.Const(constant: SynConst,
                      range: range) ->
            // Handle constant patterns: 42, "text", true
            // Type annotations:
            // - constant: the constant value being matched
            // - range: location of the constant pattern
            
            // Example analysis: detect magic numbers in patterns
            let magicNumberMessage: Message option =
                match constant with
                | SynConst.Int32(value) when value > 100 && value <> 200 && value <> 404 && value <> 500 ->
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Magic number {value} in pattern. Consider using a named constant."
                        Code = "SPP005"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | SynConst.String(text, _, _) when text.Length > 50 ->
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Very long string literal in pattern. Consider using a variable or constant."
                        Code = "SPP006"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            let analysisMessages = 
                [magicNumberMessage]
                |> List.choose id
            
            analysisMessages
        
        // === TUPLE PATTERNS ===
        | SynPat.Tuple(isStruct: bool,
                      elementPats: SynPat list,
                      commaRanges: range list,
                      range: range) ->
            // Handle tuple patterns: (x, y), struct (a, b, c)
            // Type annotations:
            // - isStruct: true for struct tuples
            // - elementPats: patterns for each tuple element
            // - range: location of the entire tuple pattern
            
            // Recursively analyze nested patterns
            let nestedMessages = 
                elementPats
                |> List.collect analyzePattern
            
            // Example analysis: detect large tuples in patterns
            let largeTupleMessage: Message option =
                if elementPats.Length > 5 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Tuple pattern has many elements ({elementPats.Length}). Consider using a record type."
                        Code = "SPP007"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest struct tuples for performance
            let structTupleMessage: Message option =
                if not isStruct && elementPats.Length <= 3 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Consider using struct tuple pattern for better performance with small tuples"
                        Code = "SPP008"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [largeTupleMessage; structTupleMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === RECORD PATTERNS ===
        | SynPat.Record(fieldPats: ((LongIdent * Ident) * range option * SynPat) list,
                       range: range) ->
            // Handle record patterns: { Name = n; Age = a }
            // Type annotations:
            // - fieldPats: list of field patterns (path, field name, pattern)
            // - range: location of the record pattern
            
            // Recursively analyze field patterns
            let nestedMessages = 
                fieldPats
                |> List.collect (fun (_, _, pat) -> analyzePattern pat)
            
            // Example analysis: detect incomplete record patterns
            let incompleteRecordMessage: Message option =
                if fieldPats.Length = 0 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Empty record pattern detected. Ensure this is intentional."
                        Code = "SPP009"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                elif fieldPats.Length = 1 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Single-field record pattern. Consider if all fields should be matched."
                        Code = "SPP010"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: detect potential field name typos
            let fieldNamingMessage: Message option =
                let hasShortFieldNames = 
                    fieldPats
                    |> List.exists (fun ((_, fieldIdent), _, _) -> fieldIdent.idText.Length <= 2)
                
                if hasShortFieldNames then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Very short field names in record pattern. Verify field names are correct."
                        Code = "SPP011"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [incompleteRecordMessage; fieldNamingMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === ARRAY OR LIST PATTERNS ===
        | SynPat.ArrayOrList(isArray: bool,
                            elementPats: SynPat list,
                            range: range) ->
            // Handle array/list patterns: [x; y; z], [|a; b|]
            // Type annotations:
            // - isArray: true for arrays [|...|], false for lists [...]
            // - elementPats: patterns for each element
            // - range: location of the collection pattern
            
            // Recursively analyze element patterns
            let nestedMessages = 
                elementPats
                |> List.collect analyzePattern
            
            // Example analysis: detect large collection patterns
            let largeCollectionMessage: Message option =
                if elementPats.Length > 10 then
                    let collectionType = if isArray then "array" else "list"
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Large {collectionType} pattern ({elementPats.Length} elements). Consider using head/tail pattern or sequence matching."
                        Code = "SPP012"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest head/tail patterns for lists
            let headTailSuggestion: Message option =
                if not isArray && elementPats.Length > 3 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Consider using head::tail pattern instead of matching many list elements explicitly"
                        Code = "SPP013"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [largeCollectionMessage; headTailSuggestion]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === LONG IDENTIFIER PATTERNS ===
        | SynPat.LongIdent(longDotId: SynLongIdent,
                          extraId: Ident option,
                          typarDecls: SynValTyparDecls option,
                          argPats: SynArgPats,
                          accessibility: SynAccess option,
                          range: range) ->
            // Handle constructor patterns: Some x, None, MyType.Case pattern
            // Type annotations:
            // - longDotId: the qualified identifier
            // - extraId: additional identifier (rare)
            // - typarDecls: type parameter declarations
            // - argPats: argument patterns for constructor
            // - accessibility: access modifier
            // - range: location of the pattern
            
            let identPath = 
                match longDotId with
                | SynLongIdent(id, _, _) -> 
                    id |> List.map (fun ident -> ident.idText) |> String.concat "."
            
            // Recursively analyze argument patterns
            let nestedMessages = 
                match argPats with
                | SynArgPats.Pats(patterns) ->
                    patterns |> List.collect analyzePattern
                | SynArgPats.NamePatPairs(namedPats, _, _) ->
                    namedPats |> List.collect (fun (_, _, pat) -> analyzePattern pat)
            
            // Example analysis: detect potentially deprecated pattern usage
            let deprecatedPatternMessage: Message option =
                if identPath.Contains("Deprecated") || identPath.Contains("Obsolete") then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Pattern matches potentially deprecated constructor '{identPath}'"
                        Code = "SPP014"
                        Severity = Severity.Warning
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest using Option.bind instead of nested Some patterns
            let nestedOptionMessage: Message option =
                if identPath = "Some" then
                    match argPats with
                    | SynArgPats.Pats([SynPat.LongIdent(SynLongIdent([someIdent], _, _), _, _, _, _, _)]) 
                        when someIdent.idText = "Some" ->
                        Some {
                            Type = "SynPat Pattern Analyzer"
                            Message = "Nested Some patterns detected. Consider using Option.bind or flatten operations."
                            Code = "SPP015"
                            Severity = Severity.Info
                            Range = range
                            Fixes = []
                        }
                    | _ -> None
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [deprecatedPatternMessage; nestedOptionMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === PARENTHESIZED PATTERNS ===
        | SynPat.Paren(pat: SynPat,
                      range: range) ->
            // Handle parenthesized patterns: (pattern)
            // Type annotations:
            // - pat: the pattern inside parentheses
            // - range: location of the parenthesized pattern
            
            let nestedMessages = analyzePattern pat
            
            // Example analysis: detect unnecessary parentheses
            let unnecessaryParensMessage: Message option =
                match pat with
                | SynPat.Named(_, _, _, _) | SynPat.Wild(_) | SynPat.Const(_, _) ->
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "Parentheses around simple patterns may be unnecessary"
                        Code = "SPP016"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [unnecessaryParensMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === AS PATTERNS ===
        | SynPat.As(lhsPat: SynPat,
                   rhsPat: SynPat,
                   range: range) ->
            // Handle 'as' patterns: pattern as variable
            // Type annotations:
            // - lhsPat: the main pattern being matched
            // - rhsPat: the variable pattern (usually Named)
            // - range: location of the as pattern
            
            let lhsMessages = analyzePattern lhsPat
            let rhsMessages = analyzePattern rhsPat
            
            // Example analysis: suggest when 'as' patterns might be overused
            let asPatternMessage: Message option =
                match lhsPat with
                | SynPat.Wild(_) ->
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = "'as' pattern with wildcard - consider if direct binding is clearer"
                        Code = "SPP017"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [asPatternMessage]
                |> List.choose id
            
            lhsMessages @ rhsMessages @ analysisMessages
        
        // === OR PATTERNS ===
        | SynPat.Or(lhsPat: SynPat,
                   rhsPat: SynPat,
                   range: range,
                   trivia: SynPatOrTrivia) ->
            // Handle 'or' patterns: pattern1 | pattern2
            // Type annotations:
            // - lhsPat: left side pattern
            // - rhsPat: right side pattern
            // - range: location of the or pattern
            // - trivia: additional syntax information
            
            let lhsMessages = analyzePattern lhsPat
            let rhsMessages = analyzePattern rhsPat
            
            // Example analysis: detect complex or patterns
            let complexOrMessage: Message option =
                let rec countOrPatterns (pat: SynPat) : int =
                    match pat with
                    | SynPat.Or(lhs, rhs, _, _) -> 1 + (countOrPatterns lhs) + (countOrPatterns rhs)
                    | _ -> 0
                
                let orCount = 1 + (countOrPatterns lhsPat) + (countOrPatterns rhsPat)
                if orCount > 5 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Complex or pattern with {orCount} alternatives. Consider refactoring for readability."
                        Code = "SPP018"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [complexOrMessage]
                |> List.choose id
            
            lhsMessages @ rhsMessages @ analysisMessages
        
        // === TYPED PATTERNS ===
        | SynPat.Typed(pat: SynPat,
                      targetType: SynType,
                      range: range) ->
            // Handle typed patterns: (pattern : type)
            // Type annotations:
            // - pat: the pattern being typed
            // - targetType: the type annotation
            // - range: location of the typed pattern
            
            let nestedMessages = analyzePattern pat
            
            // Example analysis: detect redundant type annotations in patterns
            let redundantTypeMessage: Message option =
                match pat with
                | SynPat.Named(ident, _, _, _) ->
                    let identName = match ident with | SynIdent(id, _) -> id.idText
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Type annotation on simple pattern '{identName}' may be redundant if type can be inferred"
                        Code = "SPP019"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [redundantTypeMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === NULL PATTERNS ===
        | SynPat.Null(range: range) ->
            // Handle null patterns: null
            // Type annotations:
            // - range: location of the null pattern
            
            // Example analysis: warn about null pattern usage in F#
            let nullPatternMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = "Null pattern detected. Consider using Option type instead of null for better F# idioms."
                    Code = "SPP020"
                    Severity = Severity.Warning
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [nullPatternMessage]
                |> List.choose id
            
            analysisMessages
        
        // === ATTRIBUTED PATTERNS ===
        | SynPat.Attrib(pat: SynPat,
                       attributes: SynAttributes,
                       range: range) ->
            // Handle attributed patterns: [<Attribute>] pattern
            // Type annotations:
            // - pat: the pattern with attributes
            // - attributes: the attributes applied
            // - range: location of the attributed pattern
            
            let nestedMessages = analyzePattern pat
            
            // Example analysis: detect unnecessary attributes on patterns
            let attributeMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = "Pattern with attributes detected - ensure attributes are necessary for pattern matching"
                    Code = "SPP021"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [attributeMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === LIST CONS PATTERNS ===
        | SynPat.ListCons(lhsPat: SynPat,
                         rhsPat: SynPat,
                         range: range,
                         trivia: SynPatListConsTrivia) ->
            // Handle list cons patterns: head :: tail
            // Type annotations:
            // - lhsPat: pattern for the head element
            // - rhsPat: pattern for the tail list
            // - range: location of the cons pattern
            // - trivia: additional syntax information
            
            let lhsMessages = analyzePattern lhsPat
            let rhsMessages = analyzePattern rhsPat
            
            // Example analysis: suggest when cons patterns are appropriate
            let consPatternMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = "List cons pattern detected - good for processing lists element by element"
                    Code = "SPP022"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [consPatternMessage]
                |> List.choose id
            
            lhsMessages @ rhsMessages @ analysisMessages
        
        // === AND PATTERNS ===
        | SynPat.Ands(pats: SynPat list,
                     range: range) ->
            // Handle and patterns: pattern1 & pattern2 & pattern3
            // Type annotations:
            // - pats: list of patterns that must all match
            // - range: location of the and pattern
            
            let nestedMessages = 
                pats
                |> List.collect analyzePattern
            
            // Example analysis: detect complex and patterns
            let complexAndMessage: Message option =
                if pats.Length > 3 then
                    Some {
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Complex and pattern with {pats.Length} sub-patterns. Consider simplifying for readability."
                        Code = "SPP023"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            let analysisMessages = 
                [complexAndMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === OPTIONAL VALUE PATTERNS ===
        | SynPat.OptionalVal(ident: Ident,
                            range: range) ->
            // Handle optional value patterns: ?ident
            // Type annotations:
            // - ident: the optional parameter identifier
            // - range: location of the optional pattern
            
            // Example analysis: suggest when optional patterns are used
            let optionalMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = $"Optional value pattern '?{ident.idText}' detected - ensure this is for optional parameters"
                    Code = "SPP024"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [optionalMessage]
                |> List.choose id
            
            analysisMessages
        
        // === TYPE TEST PATTERNS ===
        | SynPat.IsInst(pat: SynType,
                       range: range) ->
            // Handle type test patterns: :? Type
            // Type annotations:
            // - pat: the type being tested (note: SynType, not SynPat)
            // - range: location of the type test pattern
            
            // Example analysis: suggest alternatives to type tests
            let typeTestMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = "Type test pattern (:?) detected - consider using discriminated unions for better type safety"
                    Code = "SPP025"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [typeTestMessage]
                |> List.choose id
            
            analysisMessages
        
        // === QUOTE EXPRESSION PATTERNS ===
        | SynPat.QuoteExpr(expr: SynExpr,
                          range: range) ->
            // Handle quote expression patterns: <@ expr @>
            // Type annotations:
            // - expr: the quoted expression
            // - range: location of the quote pattern
            
            // Example analysis: warn about complex metaprogramming
            let quoteMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = "Quote expression pattern detected - metaprogramming can be complex to maintain"
                    Code = "SPP026"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [quoteMessage]
                |> List.choose id
            
            analysisMessages
        
        // === INSTANCE MEMBER PATTERNS ===
        | SynPat.InstanceMember(thisId: Ident,
                               memberId: Ident,
                               toolingId: Ident option,
                               accessibility: SynAccess option,
                               range: range) ->
            // Handle instance member patterns (rare, used in special contexts)
            // Type annotations:
            // - thisId: the 'this' identifier
            // - memberId: the member identifier
            // - toolingId: optional tooling identifier
            // - accessibility: access modifier
            // - range: location of the pattern
            
            // Example analysis: suggest alternatives to instance member patterns
            let instanceMemberMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = $"Instance member pattern detected: {thisId.idText}.{memberId.idText} - consider functional alternatives"
                    Code = "SPP027"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [instanceMemberMessage]
                |> List.choose id
            
            analysisMessages
        
        // === PARSE ERROR RECOVERY PATTERNS ===
        | SynPat.FromParseError(pat: SynPat,
                               range: range) ->
            // Handle patterns created during error recovery (SPECIAL CASE)
            // This is a compiler-internal pattern used when the parser encounters
            // syntax errors but tries to continue parsing. Rarely seen in normal code.
            // Type annotations:
            // - pat: the original pattern that had parse errors
            // - range: location of the error recovery pattern
            
            let nestedMessages = analyzePattern pat
            
            // Example analysis: detect syntax errors in patterns
            let parseErrorMessage: Message option =
                Some {
                    Type = "SynPat Pattern Analyzer"
                    Message = "Pattern parse error detected - check syntax for correctness"
                    Code = "SPP028"
                    Severity = Severity.Warning
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [parseErrorMessage]
                |> List.choose id
            
            nestedMessages @ analysisMessages
        
        // === PATTERN MATCHING COMPLETE ===
        // All 21 SynPat pattern cases have been implemented above:
        // 1. Const, 2. Wild, 3. Named, 4. Typed, 5. Attrib, 6. Or, 7. ListCons
        // 8. Ands, 9. As, 10. LongIdent, 11. Tuple, 12. Paren, 13. ArrayOrList
        // 14. Record, 15. Null, 16. OptionalVal, 17. IsInst, 18. QuoteExpr
        // 19. InstanceMember, 20. FromParseError
        // 
        // Pattern matching is exhaustive - no wildcard case needed.
    
    /// <summary>
    /// Sample analyzer that uses the SynPat pattern matching
    /// This demonstrates how to integrate the pattern analysis into a complete analyzer
    /// </summary>
    [<CliAnalyzer>]
    let synPatPatternAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    // Helper function to recursively find and analyze patterns in the AST
                    let rec analyzeDeclarations (decls: SynModuleDecl list) : unit =
                        for decl in decls do
                            match decl with
                            | SynModuleDecl.Let(isRecursive: bool, bindings: SynBinding list, range: range) ->
                                // Process let bindings - extract patterns from the bindings
                                for binding in bindings do
                                    match binding with
                                    | SynBinding(_, _, _, _, _, _, _, pat, _, _, _, _, _) ->
                                        let patMessages = analyzePattern pat
                                        messages.AddRange(patMessages)
                            | SynModuleDecl.Types(typeDefns: SynTypeDefn list, range: range) ->
                                // Process type definitions - could contain patterns in discriminated unions
                                for typeDefn in typeDefns do
                                    match typeDefn with
                                    | SynTypeDefn(_, repr, _, _, _, _) ->
                                        // Could analyze patterns in union case definitions here
                                        ()
                            | _ ->
                                // Other declaration types - not processing patterns for now
                                ()
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                        for moduleOrNs in modules do
                            match moduleOrNs with
                            | SynModuleOrNamespace(_, _, _, decls, _, _, _, _, _) ->
                                analyzeDeclarations decls
                    | ParsedInput.SigFile(_) ->
                        // Signature files might contain patterns in type signatures
                        ()
                        
                with
                | ex ->
                    messages.Add({
                        Type = "SynPat Pattern Analyzer"
                        Message = $"Error analyzing patterns: {ex.Message}"
                        Code = "SPP999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }