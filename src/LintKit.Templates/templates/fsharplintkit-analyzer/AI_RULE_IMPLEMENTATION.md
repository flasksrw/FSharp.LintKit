# AI Agent Rule Implementation Instructions

## Template Copy Workflow

**CRITICAL: DO NOT CREATE NEW FILES FROM SCRATCH**

**MANDATORY WORKFLOW**:
1. **COPY** `TemplateAnalyzer.fs` → `YourRuleName.fs` 
2. **COPY** `TemplateAnalyzerTests.fs` → `YourRuleNameTests.fs`
3. **EDIT** the copied files following the instructions inside each template
4. **ADD** the copied files to `MyCustomAnalyzer.fsproj`
5. **REPEAT** steps 1-4 for each additional rule

**CONCRETE EXAMPLE**:
```bash
# For a "No Hardcoded Strings" rule:
cp TemplateAnalyzer.fs NoHardcodedStringsAnalyzer.fs
cp TemplateAnalyzerTests.fs NoHardcodedStringsAnalyzerTests.fs
```

**Then edit the copied files**:
- Change module name to match the file name (e.g., `NoHardcodedStringsAnalyzer`)
- Follow the detailed editing instructions inside each template file

## Completion Requirements

**Implementation is complete only when ALL of these requirements are satisfied**:
- [ ] Template files were copied (not created from scratch)
- [ ] Module name matches the file name
- [ ] Copied files were added to the project file
- [ ] Code compiles without warnings
- [ ] Tests cover positive and negative cases
- [ ] All test cases pass (green)
- [ ] Error messages are clear and actionable

---

## Rule Specification Section

*Human: Fill in your rule specifications below using the format below.*

**Rule Specification Format**:
- **Rule Name**: Clear, descriptive identifier
- **Category**: CodeQuality/Security/Performance/Naming/Style
- **Severity**: Error/Warning/Info/Hint
- **Description**: What the rule detects and why it matters
- **Detection Pattern**: Specific F# code patterns to identify
- **Exclusions**: Patterns that should NOT trigger the rule
- **Message**: User-facing message when rule is triggered
- **Fix Suggestion**: Recommended code changes (if applicable)

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

---

## Implementation Instructions

After filling in the rule specifications above, send this document to an AI agent with:

**"Please implement the F# analyzers based on the rule specifications provided above."**

**REMINDER: The AI agent must satisfy ALL completion requirements listed above.**

Review the generated code and iterate on the specifications if needed.