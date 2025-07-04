# AI Agent Rule Implementation Instructions

## SYSTEM OVERVIEW

You are an AI agent tasked with implementing F# custom analyzers for the FSharp.LintKit framework using FSharp.Analyzers.SDK. Your goal is to generate complete, production-ready analyzer implementations with comprehensive test suites based on human-provided rule specifications.

FSharp.Analyzers.SDK is the official F# analyzer framework that provides:
- Standardized analyzer interfaces and attributes
- AST access through FSharp.Compiler.Syntax
- Integration with development tools and CI/CD pipelines
- Message and diagnostic reporting capabilities

## IMPLEMENTATION REQUIREMENTS

### Core Technologies
- **Framework**: FSharp.Analyzers.SDK
- **Language**: F# with explicit type annotations
- **AST Analysis**: FSharp.Compiler.Syntax
- **Target**: .NET 8+ (adjust TargetFramework based on user environment)

### Code Quality Standards
- **Type Safety**: Use explicit type annotations for all parameters and return values
- **Pattern Matching**: Implement exhaustive pattern matching without wildcard patterns
- **Error Handling**: Include proper exception handling and graceful degradation
- **Performance**: Efficient AST traversal patterns
- **Readability**: Clear, self-documenting code structure

### Project Structure
Generate the following files:
1. **[ProjectName].fsproj** - Project file with appropriate dependencies
2. **[AnalyzerName].fs** - Main analyzer implementation
3. **[ProjectName].Tests.fsproj** - Test project file
4. **[AnalyzerName]Tests.fs** - Comprehensive test suite

## RULE SPECIFICATION FORMAT

Each rule should be specified with:
- **Rule Name**: Clear, descriptive identifier
- **Category**: CodeQuality/Security/Performance/Naming/Style
- **Severity**: Error/Warning/Info/Hint
- **Description**: What the rule detects and why it matters
- **Detection Pattern**: Specific F# code patterns to identify
- **Exclusions**: Patterns that should NOT trigger the rule
- **Message**: User-facing message when rule is triggered
- **Fix Suggestion**: Recommended code changes (if applicable)

## PATTERN REFERENCES

### Essential Resources
Reference these existing implementations for patterns:
- **AST Patterns**: https://github.com/flasksrw/FSharp.LintKit/tree/main/src/LintKit.AnalyzerPatterns
- **Template Structure**: https://github.com/flasksrw/FSharp.LintKit/tree/main/templates/MyCustomAnalyzer

### Key Pattern Categories
- **SynExpr**: All F# expression patterns with complete type matching
- **SynModuleDecl**: Module-level declarations (let, type, open, nested modules)
- **SynPat**: Pattern matching constructs in match expressions and let bindings
- **SynType**: Type expressions and annotations
- **Attributes**: Custom attribute detection and analysis
- **Naming**: Identifier naming convention enforcement

## IMPLEMENTATION WORKFLOW

### Step 1: Project Setup
1. Create project files with appropriate .NET target framework
2. Add FSharp.Analyzers.SDK package reference
3. Set up proper namespace and module structure

### Step 2: Analyzer Implementation
1. Create analyzer module with [<CliAnalyzer>] attribute
2. Implement main analyzer function: `Analyzer<CliContext>`
3. Add helper functions for AST traversal and pattern detection
4. Include proper error handling and logging

### Step 3: Rule Logic
1. Pattern match against relevant AST nodes
2. Apply detection logic based on rule specifications
3. Generate appropriate Message objects with:
   - Type: Clear rule category
   - Message: User-friendly description
   - Code: Unique rule identifier (e.g., "RULE001")
   - Severity: Appropriate severity level
   - Range: Precise location information
   - Fixes: Suggested corrections (if applicable)

### Step 4: Test Suite
Generate comprehensive tests covering:
- **Positive Cases**: Rule correctly detects target patterns
- **Negative Cases**: Rule does not trigger on valid code
- **Boundary Cases**: Edge conditions and complex scenarios
- **Error Handling**: Graceful handling of malformed syntax
- **Performance**: Efficient processing of large files

## TEST REQUIREMENTS

### Test Categories
1. **Basic Functionality**
   - Rule detects intended patterns
   - Rule ignores valid code
   - Message content and codes are correct

2. **Edge Cases**
   - Empty files and modules
   - Syntax errors and malformed code
   - Very large files
   - Nested and complex expressions

3. **Boundary Conditions**
   - Minimum/maximum values for numeric rules
   - String length boundaries
   - Nesting depth limits

4. **Integration Tests**
   - Multiple rules in same file
   - Rule interactions
   - Performance under load

### Test Structure
```fsharp
[<Fact>]
let ``Should detect [pattern] in [context]`` () =
    let code = """
    [F# code example]
    """
    let results = runAnalyzer code
    // Assertions for expected behavior
```

## OUTPUT FORMAT

### Project File Template
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FSharp.Analyzers.SDK" Version="0.31.0" />
  </ItemGroup>
</Project>
```

### Analyzer Structure Template
```fsharp
namespace [ProjectName]

open FSharp.Analyzers.SDK
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

module [AnalyzerName] =
    
    let private analyze[Component] (node: [SynType]) : Message list =
        // Implementation with exhaustive pattern matching
        
    [<CliAnalyzer>]
    let [analyzerName]: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                // Main analysis logic
                return messages |> Seq.toList
            }
```

## QUALITY CHECKLIST

Before delivering implementation, verify:
- [ ] Code compiles without warnings
- [ ] All rules specified are implemented
- [ ] Test coverage includes all specified scenarios
- [ ] Performance is acceptable for large files
- [ ] Error handling prevents crashes
- [ ] Messages are clear and actionable
- [ ] Rule codes are unique and consistent
- [ ] Documentation is complete and accurate

## COMMON PITFALLS TO AVOID

1. **Over-complex Pattern Matching**: Keep patterns simple and focused
2. **Performance Issues**: Avoid deep recursion and expensive operations
3. **Incomplete Error Handling**: Always handle parse errors gracefully
4. **Vague Messages**: Provide specific, actionable feedback
5. **Missing Edge Cases**: Test boundary conditions thoroughly

---

## RULE SPECIFICATIONS

*Human: Fill in your rule specifications below. For each rule, provide all the information specified in the RULE SPECIFICATION FORMAT section above.*

### Rule 1: [Rule Name]
```
Rule Name: 
Category: 
Severity: 
Description: 
Detection Pattern: 
Exclusions: 
Message: 
Fix Suggestion: 
```

### Rule 2: [Rule Name]
```
Rule Name: 
Category: 
Severity: 
Description: 
Detection Pattern: 
Exclusions: 
Message: 
Fix Suggestion: 
```

### Additional Rules:
*Add more rules as needed following the same format*

---

## IMPLEMENTATION INSTRUCTIONS

After filling in the rule specifications above, send this entire document to an AI agent with the instruction:

**"Please implement the F# analyzers based on the rule specifications provided above. Generate complete project files, analyzer implementations, and comprehensive test suites following the guidelines and patterns specified in this document."**

The AI will generate:
1. Complete project structure with appropriate .fsproj files
2. Fully implemented analyzer with proper F# AST pattern matching
3. Comprehensive test suite with multiple test categories
4. Documentation and usage examples

Review the generated code and iterate on the specifications if needed to achieve desired behavior.