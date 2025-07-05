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

- **Working Tests (REQUIRED)**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/tests/LintKit.AnalyzerPatterns.Tests/SimpleAnalyzerExampleTests.fs
  - Correct FSharp.Analyzers.SDK.Testing usage
  - Critical pattern: `mkOptionsFromProject |> Async.AwaitTask`
  - Shows proper async test structure and assertions
  - **Use this as your primary template for test structure**

- **AST Patterns (REFERENCE)**: Directory containing pattern matching examples
  - **SynExprPatterns.fs**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/SynExprPatterns.fs - Complete SynExpr pattern matching (expressions, function calls, if-then-else, match, etc.)
  - **SynModuleDeclPatterns.fs**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/SynModuleDeclPatterns.fs - Module declarations (let bindings, type definitions, open statements, nested modules)
  - **SynPatPatterns.fs**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/SynPatPatterns.fs - Pattern matching constructs (match patterns, let binding patterns)
  - **SynTypePatterns.fs**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/SynTypePatterns.fs - Type expressions and annotations
  - **AttributeDetection.fs**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/AttributeDetection.fs - Custom attribute detection and analysis
  - **NamingConventions.fs**: https://raw.githubusercontent.com/flasksrw/FSharp.LintKit/main/src/LintKit.AnalyzerPatterns/NamingConventions.fs - Identifier naming convention enforcement
  - **Consult specific files when you encounter AST pattern matching errors**

- **Template Structure (REFERENCE)**: https://github.com/flasksrw/FSharp.LintKit/tree/main/templates/MyCustomAnalyzer
  - Project file templates and structure
  - Directory organization examples
  - **Use for project setup and organization**

### Key Pattern Categories
- **Working Analyzer**: SimpleAnalyzerExample.fs shows complete implementation with TODO detection
- **Working Tests**: SimpleAnalyzerExampleTests.fs shows correct FSharp.Analyzers.SDK.Testing usage
- **SynExpr**: All F# expression patterns with complete type matching
- **SynModuleDecl**: Module-level declarations (let, type, open, nested modules)
- **SynPat**: Pattern matching constructs in match expressions and let bindings
- **SynType**: Type expressions and annotations
- **Attributes**: Custom attribute detection and analysis
- **Naming**: Identifier naming convention enforcement

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
1. **EDIT** the existing `CustomAnalyzer.fs` file (analyzer module with [<CliAnalyzer>] attribute already exists)
2. **REPLACE** the TODO implementations in the existing `customAnalyzer` function
3. **EDIT** the helper functions `analyzeExpression` and `analyzeModuleDeclaration`
4. The error handling structure is already included

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
**EDIT** the existing `CustomAnalyzerTests.fs` file to add comprehensive tests covering:
- **Positive Cases**: Rule correctly detects target patterns
- **Negative Cases**: Rule does not trigger on valid code
- **Boundary Cases**: Edge conditions and complex scenarios
- **Error Handling**: Graceful handling of malformed syntax
- **Performance**: Efficient processing of large files

## TEST REQUIREMENTS

### Test Implementation Reference
- Study **SimpleAnalyzerExampleTests.fs** for the correct FSharp.Analyzers.SDK.Testing patterns
- Critical pattern: `mkOptionsFromProject |> Async.AwaitTask` for proper async setup
- Follow the established test structure with async blocks and proper assertions

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