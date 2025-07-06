# 🤖 AI Agent Rule Implementation Instructions

## START HERE - READ README.md FIRST

**Before reading this document, read README.md in this directory** to understand the project structure and your role as an AI agent.

## SYSTEM OVERVIEW - THIS IS FOR YOU (AI AGENT)

**You are an AI agent** tasked with implementing F# custom analyzers for the FSharp.LintKit framework using FSharp.Analyzers.SDK. Your goal is to generate complete, production-ready analyzer implementations with comprehensive test suites based on human-provided rule specifications.

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
**⚠️ CRITICAL: Always examine these resources first when implementing analyzers**

Reference these existing implementations for patterns:
- **Working Example (REQUIRED)**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/SimpleAnalyzerExample.fs
  - Complete TODO detection analyzer implementation
  - Shows proper [<CliAnalyzer>] attribute usage
  - Demonstrates correct FSharp.Analyzers.SDK patterns
  - Contains error handling and async structure examples
  - **Use this as your primary template for analyzer structure**

- **Complete AST Patterns (REQUIRED for thorough analysis)**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/CompleteASTPatterns.fs
  - THE DEFINITIVE consolidated F# AST analysis reference with all integrated knowledge
  - Complete AST pattern matching (SynExpr, SynModuleDecl, SynPat, SynType - all patterns)
  - Integrated identifier extraction, package reference detection, attribute analysis
  - Integrated type annotation guidance and severity level guidelines (Error/Warning/Info/Hint)
  - Uses State<'TState> pattern for extensible analysis with user state
  - **Use this as your primary AST analysis reference - contains all patterns in one place**

- **Working Tests (REQUIRED)**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/tests/LintKit.AnalyzerPatterns.Tests/SimpleAnalyzerExampleTests.fs
  - Correct FSharp.Analyzers.SDK.Testing usage
  - Critical pattern: `mkOptionsFromProject |> Async.AwaitTask`
  - Shows proper async test structure and assertions
  - **Use this as your primary template for test structure**

- **Package Reference Pattern (for external dependencies)**: ConsultCompleteASTPatterns.fs for package detection patterns
  - Package reference detection is integrated into CompleteASTPatterns.fs
  - For testing with external packages, use package records: `{ Name = "xunit"; Version = "2.9.2" }`
  - **Use when test code contains [<Fact>], Assert.Equal, or custom libraries**

- **🎯 SINGLE COMPREHENSIVE REFERENCE**: CompleteASTPatterns.fs consolidates all AST analysis knowledge
  - **All SynExpr patterns** (73 expression types): function calls, if-then-else, match, let bindings, etc.
  - **All SynModuleDecl patterns** (10 module declarations): let bindings, type definitions, open statements, nested modules
  - **All SynPat patterns** (20 pattern constructs): match patterns, let binding patterns, tuple patterns, etc.
  - **All SynType patterns** (23 type expressions): function types, tuple types, generic types, etc.
  - **Integrated specialized knowledge**:
    - Identifier extraction (function names, variable names, module names)
    - Package reference detection (external library usage like Assert.True, Console.WriteLine)
    - Attribute analysis (custom attributes, count validation, specific attribute patterns)
    - Type annotation guidance (presence/absence checking, redundancy detection)
    - Severity level guidelines (Error/Warning/Info/Hint with concrete examples)
  - **⚠️ Use CompleteASTPatterns.fs as your single source of truth for all AST analysis**

- **Template Structure (REFERENCE)**: https://github.com/flasksrw/FSharp.LintKit/tree/main/templates/MyCustomAnalyzer
  - Project file templates and structure
  - Directory organization examples
  - **Use for project setup and organization**

### Key Pattern Categories
- **Working Analyzer**: SimpleAnalyzerExample.fs shows complete implementation with TODO detection
- **Working Tests**: SimpleAnalyzerExampleTests.fs shows correct FSharp.Analyzers.SDK.Testing usage
- **🎯 Complete AST Reference**: CompleteASTPatterns.fs - THE DEFINITIVE consolidated reference containing:
  - All SynExpr patterns (73 expression types) with complete type matching
  - All SynModuleDecl patterns (10 module-level declarations: let, type, open, nested modules)
  - All SynPat patterns (20 pattern matching constructs in match expressions and let bindings)
  - All SynType patterns (23 type expressions and annotations)
  - All specialized analysis techniques (attributes, naming, package detection, severity guidelines) integrated into one comprehensive file

## TEMPLATE PROJECT STRUCTURE

**⚠️ IMPORTANT: This template already contains starter files - DO NOT create new files from scratch**

The template project includes:
- **MyCustomAnalyzer.fsproj** - Project file with FSharp.Analyzers.SDK reference (EDIT THIS)
- **CustomAnalyzer.fs** - Main analyzer implementation template (EDIT THIS)
- **CustomAnalyzerTests.fs** - Test file template (EDIT THIS)
- **AI_RULE_IMPLEMENTATION.md** - This instruction file

### DO NOT CREATE NEW FILES
- **EDIT** the existing `CustomAnalyzer.fs` file instead of creating new analyzer files
- **EDIT** the existing `CustomAnalyzerTests.fs` file instead of creating new test files
- **EDIT** the existing `MyCustomAnalyzer.fsproj` file instead of creating new project files

## IMPLEMENTATION WORKFLOW

### Step 1: Project Setup
1. **EDIT** the existing MyCustomAnalyzer.fsproj file to adjust target framework if needed
2. The FSharp.Analyzers.SDK package reference is already included
3. **EDIT** the namespace in CustomAnalyzer.fs to match your project name

### Step 2: Analyzer Implementation  
**🤖 IMPORTANT (FOR YOU): Create one file per rule using the template**

For each rule you need to implement:
1. **COPY** the `CustomAnalyzer.fs` file to a new file with a descriptive name:
   - Example: `NoHardcodedStringsAnalyzer.fs`
   - Example: `RequireTypeAnnotationsAnalyzer.fs`
   - Example: `NamingConventionAnalyzer.fs`

2. **EDIT** the copied file:
   - Change the module name from `TemplateAnalyzer` to match your rule
   - Change the function name from `templateAnalyzer` to match your rule
   - Update the Type, Message, and Code in the error handling section

3. **IMPLEMENT** only ONE specific rule per file

4. **ADD** the new .fs file to `MyCustomAnalyzer.fsproj`:
   ```xml
   <Compile Include="YourAnalyzerName.fs" />
   ```

5. The error handling structure and async pattern are already included in the template

### Step 3: Rule Logic (For Each Analyzer Function)
1. Focus on ONE specific rule per analyzer function
2. Pattern match against relevant AST nodes for that rule only
3. Apply detection logic based on that specific rule specification
4. Generate appropriate Message objects with:
   - Type: Clear rule category (specific to this rule)
   - Message: User-friendly description for this rule
   - Code: Unique rule identifier (e.g., "HARDCODED001", "TYPEANNOT001")
   - Severity: Appropriate severity level for this rule
   - Range: Precise location information
   - Fixes: Suggested corrections (if applicable)

### Step 4: Test Suite
**🤖 IMPORTANT (FOR YOU): Create test files using the template**

For each analyzer file you created:
1. **COPY** the `CustomAnalyzerTests.fs` file to a new test file:
   - Example: `NoHardcodedStringsAnalyzerTests.fs`
   - Example: `RequireTypeAnnotationsAnalyzerTests.fs`

2. **EDIT** the copied test file:
   - Update the `open` statement to import your specific analyzer module
   - Rename the test module to match your analyzer
   - Change the analyzer function calls to use your analyzer function name
   - Implement tests for your specific rule only

3. **ADD** the test file to `MyCustomAnalyzer.fsproj`:
   ```xml
   <Compile Include="YourAnalyzerNameTests.fs" />
   ```

4. **IMPLEMENT** comprehensive tests covering:
   - **Positive Cases**: Rule correctly detects target patterns
   - **Negative Cases**: Rule does not trigger on valid code
   - **Boundary Cases**: Edge conditions and complex scenarios
   - **Error Handling**: Graceful handling of malformed syntax

## TEST REQUIREMENTS

### Test Implementation Reference
- Study **SimpleAnalyzerExampleTests.fs** for the correct FSharp.Analyzers.SDK.Testing patterns
- Critical pattern: `mkOptionsFromProject |> Async.AwaitTask` for proper async setup
- Follow the established test structure with async blocks and proper assertions

### Package References in Test Code
**🤖 FOR AI AGENTS (THIS MEANS YOU)**: When test code references external packages, add them using Name/Version records:
```fsharp
mkOptionsFromProject
    "net8.0"
    [
        { Name = "xunit"; Version = "2.9.2" }
        { Name = "MyLibrary"; Version = "1.0.0" }
    ]
```
Common packages:
- xUnit: `{ Name = "xunit"; Version = "2.9.2" }` for `[<Fact>]`, `Assert.Equal`
- FSharp.Core: Usually not needed, included by default

### Test Categories
1. **Basic Functionality**
   - Rule detects intended patterns (positive test)
   - Rule ignores valid code (negative test)
   - Message content and codes are correct
   - Multiple instances in same file

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

## OUTPUT FORMAT

### Editing Existing Files
**EDIT** the provided template files rather than creating new ones:

1. **MyCustomAnalyzer.fsproj** - Already contains the correct project structure
2. **CustomAnalyzer.fs** - Contains the basic analyzer template with TODO markers
3. **CustomAnalyzerTests.fs** - Contains the test file template structure

### Analyzer Structure Reference
- Study **SimpleAnalyzerExample.fs** for complete analyzer implementation patterns
- **EDIT** the existing `CustomAnalyzer.fs` following the established structure
- The template already includes [<CliAnalyzer>] attribute and proper error handling structure

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