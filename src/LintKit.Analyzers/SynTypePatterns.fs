namespace LintKit.Analyzers

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text

/// <summary>
/// Complete SynType pattern matching reference for AI agents
/// 
/// This module demonstrates how to handle SynType cases with:
/// - Detailed type annotations for every parameter
/// - Explicit pattern matching for AI learning
/// - Proper analysis of F# type constructs
/// - Appropriate message creation with different severities
/// 
/// Learning points for AI agents:
/// - How to destructure each SynType variant
/// - When to recurse into nested type expressions
/// - How to extract meaningful information from type structures
/// - Proper range and location handling
/// 
/// SynType represents type expressions in F# code, such as:
/// - Basic types (int, string, custom types)
/// - Generic types (List<int>, Option<string>)
/// - Function types (int -> string)
/// - Tuple types (int * string * bool)
/// - Array types (int[], string[][])
/// - Type variables ('a, 'T)
/// 
/// This covers all 23 SynType patterns available in F# AST.
/// </summary>
module SynTypePatterns =
    
    /// <summary>
    /// Analyzes a SynType and returns any issues found
    /// </summary>
    /// <param name="synType">The F# type syntax tree node to analyze</param>
    /// <returns>List of analysis messages for any issues found</returns>
    let rec analyzeType (synType: SynType) : Message list =
        match synType with
        
        // === LONG IDENTIFIER TYPES ===
        | SynType.LongIdent(longDotId: SynLongIdent) ->
            // Handle long identifier types: int, string, MyNamespace.MyType
            // Type annotations:
            // - longDotId: the qualified type identifier
            
            let typePath = 
                match longDotId with
                | SynLongIdent(id, _, _) -> 
                    id |> List.map (fun ident -> ident.idText) |> String.concat "."
            
            // Example analysis: detect potentially deprecated types
            let deprecatedTypeMessage: Message option =
                if typePath.Contains("Deprecated") || typePath.Contains("Obsolete") then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Type '{typePath}' appears to be deprecated. Consider using alternative types."
                        Code = "STP001"
                        Severity = Severity.Warning
                        Range = longDotId.Range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest F# alternatives for .NET types
            let dotNetTypeMessage: Message option =
                match typePath with
                | "System.String" ->
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Consider using 'string' instead of 'System.String' for F# idiomatic code"
                        Code = "STP002"
                        Severity = Severity.Info
                        Range = longDotId.Range
                        Fixes = []
                    }
                | "System.Int32" ->
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Consider using 'int' instead of 'System.Int32' for F# idiomatic code"
                        Code = "STP003"
                        Severity = Severity.Info
                        Range = longDotId.Range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [deprecatedTypeMessage; dotNetTypeMessage]
                |> List.choose id
            
            analysisMessages
        
        // === TYPE APPLICATIONS ===
        | SynType.App(typeName: SynType,
                     lessRange: range option,
                     typeArgs: SynType list,
                     commaRanges: range list,
                     greaterRange: range option,
                     isPostfix: bool,
                     range: range) ->
            // Handle generic type applications: List<int>, Option<string>, Map<string, int>
            // Type annotations:
            // - typeName: the base type being applied
            // - typeArgs: the type arguments
            // - commaRanges: locations of commas between type arguments
            // - lessRange: location of opening '<'
            // - greaterRange: location of closing '>'
            // - isPostfix: true for postfix syntax (int list vs List<int>)
            // - range: location of the entire type application
            
            // Recursively analyze the base type and type arguments
            let baseTypeMessages = analyzeType typeName
            let typeArgMessages = 
                typeArgs
                |> List.collect analyzeType
            
            // Example analysis: detect complex generic types
            let complexGenericMessage: Message option =
                if typeArgs.Length > 3 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Generic type with many type arguments ({typeArgs.Length}). Consider using records or discriminated unions."
                        Code = "STP004"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest F# collections over .NET collections
            let collectionMessage: Message option =
                match typeName with
                | SynType.LongIdent(SynLongIdent([ident], _, _)) when ident.idText = "List" && not isPostfix ->
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Consider using postfix syntax 'int list' instead of 'List<int>' for F# idiomatic code"
                        Code = "STP005"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [complexGenericMessage; collectionMessage]
                |> List.choose id
            
            baseTypeMessages @ typeArgMessages @ analysisMessages
        
        // === FUNCTION TYPES ===
        | SynType.Fun(argType: SynType,
                     returnType: SynType,
                     range: range,
                     trivia: SynTypeFunTrivia) ->
            // Handle function types: int -> string, (int * string) -> bool
            // Type annotations:
            // - argType: the argument type
            // - returnType: the return type
            // - range: location of the function type
            // - trivia: additional syntax information
            
            let argMessages = analyzeType argType
            let returnMessages = analyzeType returnType
            
            // Example analysis: detect complex function signatures
            let rec countFunctionArrows (synType: SynType) : int =
                match synType with
                | SynType.Fun(_, returnType, _, _) -> 1 + (countFunctionArrows returnType)
                | _ -> 0
            
            let arrowCount = 1 + (countFunctionArrows returnType)
            let complexFunctionMessage: Message option =
                if arrowCount > 4 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Function type with many parameters ({arrowCount} arrows). Consider using a record or tuple for parameters."
                        Code = "STP006"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest currying alternatives
            let curryingMessage: Message option =
                match argType with
                | SynType.Tuple(_, _, _) ->
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Function takes tuple argument. Consider currying for better composability: a -> b -> c"
                        Code = "STP007"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            // Combine messages
            let analysisMessages = 
                [complexFunctionMessage; curryingMessage]
                |> List.choose id
            
            argMessages @ returnMessages @ analysisMessages
        
        // === TUPLE TYPES ===
        | SynType.Tuple(isStruct: bool,
                       path: SynTupleTypeSegment list,
                       range: range) ->
            // Handle tuple types: int * string, struct (int * string * bool)
            // Type annotations:
            // - isStruct: true for struct tuples
            // - elementTypes: list of tuple element types
            // - range: location of the tuple type
            
            let elementMessages = 
                path
                |> List.collect (fun segment ->
                    match segment with
                    | SynTupleTypeSegment.Type(synType) -> analyzeType synType
                    | SynTupleTypeSegment.Star(_) -> []
                    | SynTupleTypeSegment.Slash(_) -> [])
            
            // Example analysis: detect large tuples
            let largeTupleMessage: Message option =
                if path.Length > 5 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Tuple type has many elements ({path.Length}). Consider using a record type for better readability."
                        Code = "STP008"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest struct tuples for performance
            let structTupleMessage: Message option =
                if not isStruct && path.Length <= 3 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Consider using struct tuple for better performance: struct (int * string)"
                        Code = "STP009"
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
            
            elementMessages @ analysisMessages
        
        // === ARRAY TYPES ===
        | SynType.Array(rank: int,
                       elementType: SynType,
                       range: range) ->
            // Handle array types: int[], string[][], int[,,]
            // Type annotations:
            // - rank: array rank (1 for [], 2 for [][], etc.)
            // - elementType: type of array elements
            // - range: location of the array type
            
            let elementMessages = analyzeType elementType
            
            // Example analysis: detect multi-dimensional arrays
            let multiDimArrayMessage: Message option =
                if rank > 2 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Multi-dimensional array (rank {rank}). Consider using nested arrays or alternative data structures."
                        Code = "STP010"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest F# alternatives to arrays
            let arrayAlternativeMessage: Message option =
                if rank = 1 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Consider using F# list or seq instead of array for functional programming patterns"
                        Code = "STP011"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [multiDimArrayMessage; arrayAlternativeMessage]
                |> List.choose id
            
            elementMessages @ analysisMessages
        
        // === TYPE VARIABLES ===
        | SynType.Var(typar: SynTypar,
                     range: range) ->
            // Handle type variables: 'a, 'T, 'TResult
            // Type annotations:
            // - typar: the type parameter
            // - range: location of the type variable
            
            let typarName = match typar with | SynTypar(ident, _, _) -> ident.idText
            
            // Example analysis: suggest meaningful type parameter names
            let typarNamingMessage: Message option =
                if typarName.Length = 1 && typarName <> "a" && typarName <> "T" then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Single-character type parameter '{typarName}'. Consider using 'T or more descriptive names."
                        Code = "STP012"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                elif typarName.StartsWith("T") && typarName.Length > 1 && not (System.Char.IsUpper(typarName.[1])) then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Type parameter '{typarName}' should follow PascalCase convention: 'TResult"
                        Code = "STP013"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            let analysisMessages = 
                [typarNamingMessage]
                |> List.choose id
            
            analysisMessages
        
        // === ANONYMOUS RECORD TYPES ===
        | SynType.AnonRecd(isStruct: bool,
                          fields: (Ident * SynType) list,
                          range: range) ->
            // Handle anonymous record types: {| Name: string; Age: int |}
            // Type annotations:
            // - isStruct: true for struct anonymous records
            // - fields: list of field names and types
            // - range: location of the anonymous record type
            
            let fieldMessages = 
                fields
                |> List.collect (fun (_, fieldType) -> analyzeType fieldType)
            
            // Example analysis: detect large anonymous records
            let largeAnonRecordMessage: Message option =
                if fields.Length > 7 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Anonymous record with many fields ({fields.Length}). Consider defining a named record type."
                        Code = "STP014"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest when to use struct anonymous records
            let structAnonRecordMessage: Message option =
                if not isStruct && fields.Length <= 3 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Consider using struct anonymous record for better performance: struct {| Name: string |}"
                        Code = "STP015"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Combine messages
            let analysisMessages = 
                [largeAnonRecordMessage; structAnonRecordMessage]
                |> List.choose id
            
            fieldMessages @ analysisMessages
        
        // === LONG IDENTIFIER APP ===
        | SynType.LongIdentApp(typeName: SynType,
                              longDotId: SynLongIdent,
                              lessRange: range option,
                              typeArgs: SynType list,
                              commaRanges: range list,
                              greaterRange: range option,
                              range: range) ->
            // Handle long identifier applications: MyNamespace.MyType<int>
            // Type annotations:
            // - typeName: the base type
            // - longDotId: the qualified identifier
            // - lessRange: location of opening '<'
            // - typeArgs: the type arguments
            // - commaRanges: locations of commas
            // - greaterRange: location of closing '>'
            // - range: location of the entire type application
            
            let baseTypeMessages = analyzeType typeName
            let typeArgMessages = 
                typeArgs
                |> List.collect analyzeType
            
            // Example analysis: detect complex qualified generic types
            let complexQualifiedMessage: Message option =
                if typeArgs.Length > 2 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Complex qualified generic type. Consider type aliases for readability."
                        Code = "STP016"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            let analysisMessages = 
                [complexQualifiedMessage]
                |> List.choose id
            
            baseTypeMessages @ typeArgMessages @ analysisMessages
        
        // === ANONYMOUS TYPES ===
        | SynType.Anon(range: range) ->
            // Handle anonymous types: obj (compiler-generated)
            // Type annotations:
            // - range: location of the anonymous type
            
            // Example analysis: detect anonymous type usage
            let anonymousTypeMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Anonymous type detected - consider using explicit types for better readability"
                    Code = "STP017"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [anonymousTypeMessage]
                |> List.choose id
            
            analysisMessages
        
        // === STATIC CONSTANTS ===
        | SynType.StaticConstant(constant: SynConst,
                                range: range) ->
            // Handle static constants in types (used in type-level programming)
            // Type annotations:
            // - constant: the constant value
            // - range: location of the static constant
            
            // Example analysis: detect complex type-level constants
            let staticConstantMessage: Message option =
                match constant with
                | SynConst.String(text, _, _) when text.Length > 50 ->
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Very long string constant in type. Consider using shorter identifiers."
                        Code = "STP017"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ ->
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Static constant in type detected - ensure this is necessary for type-level programming"
                        Code = "STP018"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
            
            let analysisMessages = 
                [staticConstantMessage]
                |> List.choose id
            
            analysisMessages
        
        // === STATIC CONSTANT EXPRESSIONS ===
        | SynType.StaticConstantExpr(expr: SynExpr,
                                    range: range) ->
            // Handle static constant expressions in types
            // Type annotations:
            // - expr: the constant expression
            // - range: location of the static constant expression
            
            // Example analysis: warn about complex type-level expressions
            let staticExprMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Static constant expression in type - this is advanced type-level programming"
                    Code = "STP019"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [staticExprMessage]
                |> List.choose id
            
            analysisMessages
        
        // === STATIC CONSTANT NAMED ===
        | SynType.StaticConstantNamed(ident: SynType,
                                     value: SynType,
                                     range: range) ->
            // Handle named static constants in types
            // Type annotations:
            // - ident: the identifier type
            // - value: the value type
            // - range: location of the named static constant
            
            let identMessages = analyzeType ident
            let valueMessages = analyzeType value
            
            // Example analysis: detect complex named type constants
            let namedConstantMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Named static constant in type - verify this is required for type constraints"
                    Code = "STP020"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [namedConstantMessage]
                |> List.choose id
            
            identMessages @ valueMessages @ analysisMessages
        
        // === CONSTRAINTS ===
        | SynType.WithGlobalConstraints(typeName: SynType,
                                       constraints: SynTypeConstraint list,
                                       range: range) ->
            // Handle types with global constraints: 'T when 'T : comparison
            // Type annotations:
            // - typeName: the type being constrained
            // - constraints: list of constraints
            // - range: location of the constrained type
            
            let typeMessages = analyzeType typeName
            
            // Example analysis: detect complex constraint combinations
            let constraintComplexityMessage: Message option =
                if constraints.Length > 3 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Type with many constraints ({constraints.Length}). Consider simplifying or using interfaces."
                        Code = "STP021"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            // Example analysis: suggest common constraint patterns
            let constraintPatternMessage: Message option =
                if constraints.Length > 0 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Type constraints detected - ensure they are necessary and not overly restrictive"
                        Code = "STP022"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            let analysisMessages = 
                [constraintComplexityMessage; constraintPatternMessage]
                |> List.choose id
            
            typeMessages @ analysisMessages
        
        // === HASH CONSTRAINTS ===
        | SynType.HashConstraint(innerType: SynType,
                                range: range) ->
            // Handle hash constraints (flexible types): #seq<int>
            // Type annotations:
            // - innerType: the constrained type
            // - range: location of the hash constraint
            
            let innerMessages = analyzeType innerType
            
            // Example analysis: suggest when flexible types are appropriate
            let flexibleTypeMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Flexible type (#) detected - ensure this provides the needed flexibility without sacrificing type safety"
                    Code = "STP023"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [flexibleTypeMessage]
                |> List.choose id
            
            innerMessages @ analysisMessages
        
        // === MEASURE POWER ===
        | SynType.MeasurePower(baseMeasure: SynType,
                              exponent: SynRationalConst,
                              range: range) ->
            // Handle measure powers: m^2, kg^-1
            // Type annotations:
            // - baseMeasure: the base measure type
            // - exponent: the power exponent
            // - range: location of the measure power
            
            let baseMessages = analyzeType baseMeasure
            
            // Example analysis: detect complex measure expressions
            let measureComplexityMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Measure power detected - ensure units of measure are consistently used throughout the codebase"
                    Code = "STP024"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [measureComplexityMessage]
                |> List.choose id
            
            baseMessages @ analysisMessages
        
        // === STATIC CONSTANT NULL ===
        | SynType.StaticConstantNull(range: range) ->
            // Handle static constant null in types
            // Type annotations:
            // - range: location of the null constant
            
            // Example analysis: warn about null in type system
            let nullConstantMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Static null constant in type - consider using Option type for better F# idioms"
                    Code = "STP025"
                    Severity = Severity.Warning
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [nullConstantMessage]
                |> List.choose id
            
            analysisMessages
        
        // === PARENTHESIZED TYPES ===
        | SynType.Paren(innerType: SynType,
                       range: range) ->
            // Handle parenthesized types: (int -> string)
            // Type annotations:
            // - innerType: the type inside parentheses
            // - range: location of the parenthesized type
            
            let innerMessages = analyzeType innerType
            
            // Example analysis: detect unnecessary parentheses
            let unnecessaryParensMessage: Message option =
                match innerType with
                | SynType.LongIdent(_) | SynType.Var(_, _) ->
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Parentheses around simple type may be unnecessary"
                        Code = "STP026"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                | _ -> None
            
            let analysisMessages = 
                [unnecessaryParensMessage]
                |> List.choose id
            
            innerMessages @ analysisMessages
        
        // === WITH NULL TYPES ===
        | SynType.WithNull(innerType: SynType,
                          ambivalent: bool,
                          range: range,
                          trivia: SynTypeWithNullTrivia) ->
            // Handle types with null annotations
            // Type annotations:
            // - innerType: the base type
            // - ambivalent: whether null is ambivalent
            // - range: location of the type
            // - trivia: additional syntax information
            
            let innerMessages = analyzeType innerType
            
            // Example analysis: suggest F# alternatives to nullable types
            let nullableMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Nullable type annotation detected - consider using Option type for F# idioms"
                    Code = "STP026"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [nullableMessage]
                |> List.choose id
            
            innerMessages @ analysisMessages
        
        // === SIGNATURE PARAMETER TYPES ===
        | SynType.SignatureParameter(attributes: SynAttributes,
                                    optional: bool,
                                    paramId: Ident option,
                                    usedType: SynType,
                                    range: range) ->
            // Handle signature parameter types
            // Type annotations:
            // - attributes: parameter attributes
            // - optional: whether parameter is optional
            // - id: parameter identifier
            // - usedType: the parameter type
            // - range: location of the parameter
            
            let typeMessages = analyzeType usedType
            
            // Example analysis: detect complex signature parameters
            let signatureParamMessage: Message option =
                if optional then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = "Optional parameter in signature - ensure this is necessary for API design"
                        Code = "STP027"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            let analysisMessages = 
                [signatureParamMessage]
                |> List.choose id
            
            typeMessages @ analysisMessages
        
        // === OR TYPES ===
        | SynType.Or(lhsType: SynType,
                    rhsType: SynType,
                    range: range,
                    trivia: SynTypeOrTrivia) ->
            // Handle or types (union types in type expressions)
            // Type annotations:
            // - lhsType: left side type
            // - rhsType: right side type
            // - range: location of the or type
            // - trivia: additional syntax information
            
            let lhsMessages = analyzeType lhsType
            let rhsMessages = analyzeType rhsType
            
            // Example analysis: suggest discriminated unions over or types
            let orTypeMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Or type detected - consider using discriminated union for better type safety"
                    Code = "STP028"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [orTypeMessage]
                |> List.choose id
            
            lhsMessages @ rhsMessages @ analysisMessages
        
        // === INTERSECTION TYPES ===
        | SynType.Intersection(typar: SynTypar option,
                              types: SynType list,
                              range: range,
                              trivia: SynTyparDeclTrivia) ->
            // Handle intersection types
            // Type annotations:
            // - typar: optional type parameter
            // - types: list of intersected types
            // - range: location of the intersection
            // - trivia: additional syntax information
            
            let typeMessages = 
                types
                |> List.collect analyzeType
            
            // Example analysis: detect complex intersection types
            let intersectionMessage: Message option =
                if types.Length > 3 then
                    Some {
                        Type = "SynType Pattern Analyzer"
                        Message = $"Complex intersection type with {types.Length} types. Consider simplifying."
                        Code = "STP029"
                        Severity = Severity.Info
                        Range = range
                        Fixes = []
                    }
                else
                    None
            
            let analysisMessages = 
                [intersectionMessage]
                |> List.choose id
            
            typeMessages @ analysisMessages
        
        // === PARSE ERROR TYPES ===
        | SynType.FromParseError(range: range) ->
            // Handle types created during error recovery (SPECIAL CASE)
            // This is a compiler-internal type used when the parser encounters
            // syntax errors but tries to continue parsing. Rarely seen in normal code.
            // Type annotations:
            // - range: location of the error recovery type
            
            // Example analysis: detect syntax errors in types
            let parseErrorMessage: Message option =
                Some {
                    Type = "SynType Pattern Analyzer"
                    Message = "Type parse error detected - check syntax for correctness"
                    Code = "STP030"
                    Severity = Severity.Warning
                    Range = range
                    Fixes = []
                }
            
            let analysisMessages = 
                [parseErrorMessage]
                |> List.choose id
            
            analysisMessages
        
    
    /// <summary>
    /// Sample analyzer that uses the SynType pattern matching
    /// This demonstrates how to integrate the type analysis into a complete analyzer
    /// </summary>
    [<CliAnalyzer>]
    let synTypePatternAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    // Helper function to recursively find and analyze types in the AST
                    let rec analyzeDeclarations (decls: SynModuleDecl list) : unit =
                        for decl in decls do
                            match decl with
                            | SynModuleDecl.Let(isRecursive: bool, bindings: SynBinding list, range: range) ->
                                // Process let bindings - extract type annotations from bindings
                                for binding in bindings do
                                    match binding with
                                    | SynBinding(_, _, _, _, _, _, _, _, returnInfo, _, _, _, _) ->
                                        // Analyze return type annotations
                                        match returnInfo with
                                        | Some (SynBindingReturnInfo(typeName, _, _, _)) ->
                                            let typeMessages = analyzeType typeName
                                            messages.AddRange(typeMessages)
                                        | None -> ()
                            | SynModuleDecl.Types(typeDefns: SynTypeDefn list, range: range) ->
                                // Process type definitions - analyze type representations
                                for typeDefn in typeDefns do
                                    match typeDefn with
                                    | SynTypeDefn(_, repr, _, _, _, _) ->
                                        match repr with
                                        | SynTypeDefnRepr.Simple(simpleRepr, _) ->
                                            match simpleRepr with
                                            | SynTypeDefnSimpleRepr.Record(_, fields, _) ->
                                                // Analyze record field types
                                                for field in fields do
                                                    match field with
                                                    | SynField(_, _, _, fieldType, _, _, _, _, _) ->
                                                        let typeMessages = analyzeType fieldType
                                                        messages.AddRange(typeMessages)
                                            | SynTypeDefnSimpleRepr.Union(_, cases, _) ->
                                                // Analyze union case types
                                                for case in cases do
                                                    match case with
                                                    | SynUnionCase(_, _, caseType, _, _, _, _) ->
                                                        match caseType with
                                                        | SynUnionCaseKind.Fields(fields) ->
                                                            for field in fields do
                                                                match field with
                                                                | SynField(_, _, _, fieldType, _, _, _, _, _) ->
                                                                    let typeMessages = analyzeType fieldType
                                                                    messages.AddRange(typeMessages)
                                                        | _ -> ()
                                            | _ -> ()
                                        | _ -> ()
                            | _ ->
                                // Other declaration types - not processing types for now
                                ()
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                        for moduleOrNs in modules do
                            match moduleOrNs with
                            | SynModuleOrNamespace(_, _, _, decls, _, _, _, _, _) ->
                                analyzeDeclarations decls
                    | ParsedInput.SigFile(_) ->
                        // Signature files contain many type expressions to analyze
                        ()
                        
                with
                | ex ->
                    messages.Add({
                        Type = "SynType Pattern Analyzer"
                        Message = $"Error analyzing types: {ex.Message}"
                        Code = "STP999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }