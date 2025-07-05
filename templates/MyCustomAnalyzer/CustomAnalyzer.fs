/// <summary>
/// ANALYZER TEMPLATE FILE - DO NOT EDIT DIRECTLY
/// 
/// **🤖 FOR AI AGENTS (THIS MEANS YOU)**: COPY this file for each rule:
/// 1. COPY this entire file to create new analyzer files (e.g., NoHardcodedStringsAnalyzer.fs)
/// 2. RENAME the namespace, module, and analyzer function to match your rule
/// 3. IMPLEMENT only ONE specific rule per file
/// 4. ADD the new .fs file to the .fsproj file
/// 5. Reference LintKit.AnalyzerPatterns files for specific AST pattern matching
/// 
/// **EXAMPLE FILE NAMES TO CREATE**:
/// - NoHardcodedStringsAnalyzer.fs
/// - RequireTypeAnnotationsAnalyzer.fs
/// - NamingConventionAnalyzer.fs
/// - SecurityCheckAnalyzer.fs
/// </summary>
namespace MyCustomAnalyzer

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

// 🤖 TEMPLATE MODULE (FOR YOU): 
// When copying this file, rename "TemplateAnalyzer" to match your rule name
// Example: NoHardcodedStringsAnalyzer, RequireTypeAnnotationsAnalyzer, etc.
module TemplateAnalyzer =
    
    /// <summary>
    /// TEMPLATE ANALYZER FUNCTION - COPY AND CUSTOMIZE FOR EACH RULE
    /// 
    /// **🤖 INSTRUCTIONS (FOR YOU)**:
    /// 1. COPY this entire file to create a new analyzer file
    /// 2. RENAME the module and function to match your rule
    /// 3. IMPLEMENT the specific logic for that rule only
    /// 4. Study SimpleAnalyzerExample.fs for the complete implementation pattern
    /// 5. ADD the new .fs file to MyCustomAnalyzer.fsproj
    /// </summary>
    [<CliAnalyzer>]
    let templateAnalyzer: Analyzer<CliContext> =
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
                                // 🤖 IMPLEMENT YOUR SINGLE RULE LOGIC HERE (FOR YOU)
                                // Focus on ONE specific rule only in this file
                                // Reference SimpleAnalyzerExample.fs for patterns
                                
                                // TODO: Replace with your actual rule implementation
                                // This file should implement only ONE rule
                                for decl in decls do
                                    // Example: Process each declaration for your specific rule
                                    // let ruleMessages = checkYourSpecificRule decl
                                    // messages.AddRange ruleMessages
                                    ()
                    
                    | ParsedInput.SigFile(_) ->
                        // 🤖 HANDLE SIGNATURE FILES IF NEEDED (FOR YOU)
                        ()
                
                with
                | ex ->
                    // 🤖 ERROR HANDLING - KEEP THIS PATTERN (FOR YOU)
                    messages.Add({
                        Type = "Template Rule"  // 🤖 CHANGE THIS (FOR YOU): Update to your rule name
                        Message = $"Error during analysis: {ex.Message}"
                        Code = "TEMPLATE999"    // 🤖 CHANGE THIS (FOR YOU): Use your rule code (e.g., HARDCODED001)
                        Severity = Severity.Error
                        Range = Range.Zero
                        Fixes = []
                    })
                
                return messages |> Seq.toList
            }
    
// 🤖 DO NOT ADD MORE ANALYZERS TO THIS FILE (FOR YOU)
// Instead, COPY this entire file to create new analyzer files:
// 
// STEPS TO CREATE A NEW ANALYZER:
// 1. Copy CustomAnalyzer.fs to a new file (e.g., NoHardcodedStringsAnalyzer.fs)
// 2. Rename the namespace if needed
// 3. Rename the module (e.g., NoHardcodedStringsAnalyzer)
// 4. Rename the function (e.g., noHardcodedStringsAnalyzer)
// 5. Implement your specific rule logic
// 6. Add the new .fs file to MyCustomAnalyzer.fsproj
//
// EXAMPLE FILES TO CREATE:
// - NoHardcodedStringsAnalyzer.fs
// - RequireTypeAnnotationsAnalyzer.fs  
// - NamingConventionAnalyzer.fs