namespace LintKit.AnalyzerPatterns

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// <summary>
/// Naming convention analysis for F# code quality and consistency
/// 
/// This analyzer focuses on detecting naming convention violations:
/// - Test function naming patterns (prefix requirements)
/// - Public API naming consistency (PascalCase, camelCase)
/// - Module and namespace naming conventions
/// - Type and member naming standards
/// - Parameter and variable naming guidelines
/// 
/// Educational purpose for AI agents:
/// - How to enforce team-specific naming conventions
/// - Pattern recognition for different code contexts
/// - Consistent API design through naming
/// - Maintainability through clear naming standards
/// 
/// Common naming convention categories:
/// - Test functions: Should* / Test* / When* prefixes
/// - Public APIs: PascalCase for types, camelCase for functions
/// - Internal code: Flexible but consistent patterns
/// - Constants: ALL_CAPS or PascalCase depending on team preference
/// - Modules: PascalCase with descriptive names
/// 
/// Note: Conventions can be team-specific. This provides common patterns
/// that teams can adapt to their coding standards.
/// </summary>
module NamingConventions =
    
    /// <summary>
    /// Analyzes function names for test naming conventions
    /// </summary>
    /// <param name="functionName">The function name to analyze</param>
    /// <param name="range">The range of the function</param>
    /// <returns>List of naming convention messages</returns>
    let private analyzeTestFunctionNaming (functionName: string) (range: range) : Message list =
        let messages = ResizeArray<Message>()
        
        // Common test prefixes
        let testPrefixes = [
            "Should"; "Test"; "When"; "Given"; "Then"
            "Can"; "Will"; "Must"; "Verify"; "Check"
            "Ensure"; "Assert"; "Expect"
        ]
        
        // Check if this looks like a test function but doesn't follow conventions
        let looksLikeTest = 
            functionName.Contains("Test") || 
            functionName.Contains("Should") ||
            functionName.Contains("Verify") ||
            functionName.Contains("Assert") ||
            functionName.Contains("Check")
        
        if looksLikeTest then
            let hasValidPrefix = testPrefixes |> List.exists (fun prefix -> functionName.StartsWith(prefix))
            
            if not hasValidPrefix then
                messages.Add({
                    Type = "Naming Convention Analyzer"
                    Message = $"Test function '{functionName}' should start with a descriptive prefix like 'Should', 'Test', 'When', etc."
                    Code = "NC001"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            // Check for underscores in test names (some teams prefer them)
            if not (functionName.Contains("_")) && functionName.Length > 15 then
                messages.Add({
                    Type = "Naming Convention Analyzer"
                    Message = $"Long test function name '{functionName}'. Consider using underscores for readability: 'Should_ReturnTrue_WhenInputIsValid'."
                    Code = "NC002"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes function names for public API naming conventions
    /// </summary>
    /// <param name="functionName">The function name to analyze</param>
    /// <param name="range">The range of the function</param>
    /// <returns>List of naming convention messages</returns>
    let private analyzePublicFunctionNaming (functionName: string) (range: range) : Message list =
        let messages = ResizeArray<Message>()
        
        // Check if function appears to be public (starts with uppercase)
        if functionName.Length > 0 && System.Char.IsUpper(functionName.[0]) then
            
            // Check for camelCase after first character (F# convention for functions)
            let hasLowerCaseAfterFirst = 
                functionName.Length > 1 && System.Char.IsLower(functionName.[1])
            
            if not hasLowerCaseAfterFirst && not (functionName.Length = 1) then
                messages.Add({
                    Type = "Naming Convention Analyzer"
                    Message = $"Public function '{functionName}' should use camelCase naming (e.g., 'calculateTotal' not 'CalculateTotal')."
                    Code = "NC003"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            // Check for abbreviations or acronyms
            let hasConsecutiveUpperCase = 
                functionName.ToCharArray()
                |> Array.windowed 2
                |> Array.exists (fun pair -> System.Char.IsUpper(pair.[0]) && System.Char.IsUpper(pair.[1]))
            
            if hasConsecutiveUpperCase then
                messages.Add({
                    Type = "Naming Convention Analyzer"
                    Message = $"Function '{functionName}' contains consecutive uppercase letters. Consider camelCase for acronyms (e.g., 'parseXml' not 'parseXML')."
                    Code = "NC004"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            // Check for very short public function names
            if functionName.Length <= 2 then
                messages.Add({
                    Type = "Naming Convention Analyzer"
                    Message = $"Public function name '{functionName}' is very short. Consider a more descriptive name."
                    Code = "NC005"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
            
            // Check for very long function names
            if functionName.Length > 30 then
                messages.Add({
                    Type = "Naming Convention Analyzer"
                    Message = $"Public function name '{functionName}' is very long ({functionName.Length} characters). Consider simplifying."
                    Code = "NC006"
                    Severity = Severity.Warning
                    Range = range
                    Fixes = []
                })
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes type names for naming convention compliance
    /// </summary>
    /// <param name="typeName">The type name to analyze</param>
    /// <param name="range">The range of the type</param>
    /// <returns>List of naming convention messages</returns>
    let private analyzeTypeNaming (typeName: string) (range: range) : Message list =
        let messages = ResizeArray<Message>()
        
        // Types should use PascalCase
        if typeName.Length > 0 && System.Char.IsLower(typeName.[0]) then
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Type name '{typeName}' should start with uppercase letter (PascalCase)."
                Code = "NC007"
                Severity = Severity.Warning
                Range = range
                Fixes = []
            })
        
        // Check for underscores in type names (generally discouraged)
        if typeName.Contains("_") then
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Type name '{typeName}' contains underscores. Consider PascalCase instead (e.g., 'UserAccount' not 'User_Account')."
                Code = "NC008"
                Severity = Severity.Info
                Range = range
                Fixes = []
            })
        
        // Check for common suffixes
        let appropriateSuffixes = ["Request"; "Response"; "Result"; "Exception"; "Attribute"; "Builder"; "Factory"; "Service"]
        let hasKnownSuffix = appropriateSuffixes |> List.exists (fun suffix -> typeName.EndsWith(suffix))
        
        if typeName.Length > 15 && not hasKnownSuffix then
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Long type name '{typeName}' without common suffix. Consider descriptive suffixes like 'Service', 'Result', etc."
                Code = "NC009"
                Severity = Severity.Info
                Range = range
                Fixes = []
            })
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes module names for naming convention compliance
    /// </summary>
    /// <param name="moduleName">The module name to analyze</param>
    /// <param name="range">The range of the module</param>
    /// <returns>List of naming convention messages</returns>
    let private analyzeModuleNaming (moduleName: string) (range: range) : Message list =
        let messages = ResizeArray<Message>()
        
        // Modules should use PascalCase
        if moduleName.Length > 0 && System.Char.IsLower(moduleName.[0]) then
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Module name '{moduleName}' should start with uppercase letter (PascalCase)."
                Code = "NC010"
                Severity = Severity.Warning
                Range = range
                Fixes = []
            })
        
        // Check for very generic module names
        let genericNames = ["Utils"; "Helper"; "Common"; "Shared"; "Misc"; "Utilities"]
        if genericNames |> List.contains moduleName then
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Module name '{moduleName}' is very generic. Consider a more specific, descriptive name."
                Code = "NC011"
                Severity = Severity.Info
                Range = range
                Fixes = []
            })
        
        // Check for plural vs singular (guidance)
        if moduleName.EndsWith("s") && moduleName.Length > 3 then
            let singular = moduleName.Substring(0, moduleName.Length - 1)
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Module name '{moduleName}' is plural. Consider if singular '{singular}' better represents the module's purpose."
                Code = "NC012"
                Severity = Severity.Info
                Range = range
                Fixes = []
            })
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes variable and parameter names for naming conventions
    /// </summary>
    /// <param name="variableName">The variable name to analyze</param>
    /// <param name="range">The range of the variable</param>
    /// <returns>List of naming convention messages</returns>
    let private analyzeVariableNaming (variableName: string) (range: range) : Message list =
        let messages = ResizeArray<Message>()
        
        // Check for single character names (except for common cases)
        if variableName.Length = 1 then
            let commonSingleChars = ["i"; "j"; "k"; "x"; "y"; "z"; "n"; "f"; "g"]
            if not (List.contains variableName commonSingleChars) then
                messages.Add({
                    Type = "Naming Convention Analyzer"
                    Message = $"Single-character variable name '{variableName}' may be unclear. Consider a more descriptive name."
                    Code = "NC013"
                    Severity = Severity.Info
                    Range = range
                    Fixes = []
                })
        
        // Check for numbered variables (often a code smell)
        if System.Text.RegularExpressions.Regex.IsMatch(variableName, @".*\d+$") then
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Variable name '{variableName}' ends with number. Consider more descriptive names instead of numbering."
                Code = "NC014"
                Severity = Severity.Info
                Range = range
                Fixes = []
            })
        
        // Check for Hungarian notation (discouraged in F#)
        let hungarianPrefixes = ["str"; "int"; "bool"; "lst"; "arr"; "dict"]
        let hasHungarianPrefix = hungarianPrefixes |> List.exists (fun prefix -> variableName.StartsWith(prefix) && variableName.Length > prefix.Length)
        
        if hasHungarianPrefix then
            messages.Add({
                Type = "Naming Convention Analyzer"
                Message = $"Variable name '{variableName}' appears to use Hungarian notation. F# type system makes this unnecessary."
                Code = "NC015"
                Severity = Severity.Info
                Range = range
                Fixes = []
            })
        
        messages |> Seq.toList
    
    /// <summary>
    /// Analyzes function bindings for naming convention compliance
    /// </summary>
    /// <param name="binding">The function binding to analyze</param>
    /// <returns>List of naming convention messages</returns>
    let private analyzeFunctionBinding (binding: SynBinding) : Message list =
        match binding with
        | SynBinding(_, _, _, _, _, _, _, pat, _, _, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            match pat with
            | SynPat.Named(SynIdent(ident, _), _, _, _) ->
                let functionName = ident.idText
                let range = ident.idRange
                
                // Apply test naming analysis
                let testMessages = analyzeTestFunctionNaming functionName range
                messages.AddRange testMessages
                
                // Apply public function naming analysis
                let publicMessages = analyzePublicFunctionNaming functionName range
                messages.AddRange publicMessages
                
                // Apply variable naming analysis
                let variableMessages = analyzeVariableNaming functionName range
                messages.AddRange variableMessages
                
            | SynPat.LongIdent(SynLongIdent(ids, _, _), _, _, _, _, _) ->
                // Handle qualified function names
                for ident in ids do
                    let functionName = ident.idText
                    let range = ident.idRange
                    
                    let publicMessages = analyzePublicFunctionNaming functionName range
                    messages.AddRange publicMessages
            
            | _ -> ()
            
            messages |> Seq.toList
    
    /// <summary>
    /// Analyzes type definitions for naming convention compliance
    /// </summary>
    /// <param name="typeDefn">The type definition to analyze</param>
    /// <returns>List of naming convention messages</returns>
    let private analyzeTypeDefinition (typeDefn: SynTypeDefn) : Message list =
        match typeDefn with
        | SynTypeDefn(SynComponentInfo(_, _, _, longId, _, _, _, _), _, _, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            let typeName = longId |> List.map (fun i -> i.idText) |> String.concat "."
            let range = longId |> List.tryHead |> Option.map (fun i -> i.idRange) |> Option.defaultValue Range.Zero
            
            let typeMessages = analyzeTypeNaming typeName range
            messages.AddRange typeMessages
            
            messages |> Seq.toList
    
    /// <summary>
    /// Analyzes module declarations for naming convention patterns
    /// </summary>
    /// <param name="decl">The module declaration to analyze</param>
    /// <returns>List of naming convention messages</returns>
    let rec analyzeModuleDeclaration (decl: SynModuleDecl) : Message list =
        match decl with
        | SynModuleDecl.Let(_, bindings, _) ->
            bindings |> List.collect analyzeFunctionBinding
        
        | SynModuleDecl.Types(typeDefns, _) ->
            typeDefns |> List.collect analyzeTypeDefinition
        
        | SynModuleDecl.NestedModule(SynComponentInfo(_, _, _, longId, _, _, _, _), _, decls, _, _, _) ->
            let messages = ResizeArray<Message>()
            
            let moduleName = longId |> List.map (fun i -> i.idText) |> String.concat "."
            let range = longId |> List.tryHead |> Option.map (fun i -> i.idRange) |> Option.defaultValue Range.Zero
            
            let moduleMessages = analyzeModuleNaming moduleName range
            messages.AddRange moduleMessages
            
            // Recursively analyze nested declarations
            let nestedMessages = decls |> List.collect analyzeModuleDeclaration
            messages.AddRange nestedMessages
            
            messages |> Seq.toList
        
        | _ -> []
    
    /// <summary>
    /// Main naming convention analyzer
    /// </summary>
    [<CliAnalyzer>]
    let namingConventionAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                let messages = ResizeArray<Message>()
                
                try
                    let parseResults = context.ParseFileResults
                    
                    match parseResults.ParseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                        for moduleOrNs in modules do
                            match moduleOrNs with
                            | SynModuleOrNamespace(longId, _, _, decls, _, _, _, _, _) ->
                                // Analyze module/namespace name
                                let moduleName = longId |> List.map (fun i -> i.idText) |> String.concat "."
                                let range = longId |> List.tryHead |> Option.map (fun i -> i.idRange) |> Option.defaultValue Range.Zero
                                
                                if moduleName <> "" then
                                    let moduleMessages = analyzeModuleNaming moduleName range
                                    messages.AddRange moduleMessages
                                
                                // Analyze all declarations within the module
                                let declMessages = decls |> List.collect analyzeModuleDeclaration
                                messages.AddRange declMessages
                    
                    | ParsedInput.SigFile(_) ->
                        // Signature files can also have naming conventions
                        messages.Add({
                            Type = "Naming Convention Analyzer"
                            Message = "Signature file detected. Ensure naming conventions match implementation."
                            Code = "NC016"
                            Severity = Severity.Info
                            Range = Range.Zero
                            Fixes = []
                        })
                
                with
                | ex ->
                    messages.Add({
                        Type = "Naming Convention Analyzer"
                        Message = $"Error analyzing naming conventions: {ex.Message}"
                        Code = "NC999"
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }