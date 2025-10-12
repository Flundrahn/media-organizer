# 1 Development

## 1.1 Rules 
- Code must compile before committing.
- Tests affected by your change must pass locally.

## 1.2 Quick commands
- Build: 
```pwsh
dotnet build
```
- Run tests (targeted): 
```pwsh
dotnet test <Project> --filter "<TestClassName>"
```
- Use the 
```pwsh
test-watch
```
VS Code task for fast feedback during development.

## 1.3 General guidelines
- Do not add unnecessary comments to code, using clear naming is enough.
- Do not shorten variable names, e.g. use 'service' not 'svc', use 'result' not 'res', unless very long words or very common shortened names, e.g. customerAgreementRepository could be shortened to customerAgreementRepo.
- Don't stop and ask about small things e.g. adding a using statement to fix a compilation error.

## 1.3 Conventions
- You MUST follow all code-formatting and naming conventions defined in [`.editorconfig`](/.editorconfig).
- Test names: MethodName_IfThisState_ShouldReturnThat.
- Commit messages should be concise and end with a period.
- **Any code you commit SHOULD compile, and new and existing tests related to the change SHOULD pass.**

In addition to the rules enforced by `.editorconfig`, you SHOULD:

- Prefer file-scoped namespace declarations and single-line using directives.
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
- Prefer `?.` if applicable (e.g. `scope?.Dispose()`).
- Use `ObjectDisposedException.ThrowIf` where applicable.
- When adding new unit tests, strongly prefer to add them to existing test code files rather than creating new code files.
- When running tests, if possible use filters and check test run counts, or look at test logs, to ensure they actually ran.
- Do not finish work with any tests commented out or disabled that were not previously commented out or disabled.
- When writing tests, do not emit "Act", "Arrange" or "Assert" comments.

## 2 Iterative Build and Test Strategy

0. Always maintain continuous test-watch using VS Code tasks, check output instead of starting new test runs.

1. Apply the intended changes

2. **Attempt Build.** If the build fails, attempt to fix and retry the step (up to 5 attempts).

3. **Attempt Test.**
    - If a test _build_ fails, attempt to fix and retry the step (up to 5 attempts).
    - If a test _run_ fails,
        - Determine if the problem is in the test or in the source
        - If the problem is in the test, attempt to fix and retry the step (up to 5 attempts).
        - If the problem is in the source, reconsider the full changeset, attempt to fix and repeat the workflow.

4. **Workflow Iteration:**
    - Repeat build and test up to 5 cycles.
    - If issues persist after 5 workflow cycles, report failure.
    - If the same error persists after each fix attempt, do not repeat the same fix. Instead, escalate or report with full logs.

When retrying, attempt different fixes and adjust based on the build/test results.

### 2.1 Success Criteria

- **Build:**
    - Completes without errors.
    - Any non-zero exit code from build commands is considered a failure.

- **Tests:**
    - All tests must pass (zero failures).
    - Any non-zero exit code from test commands is considered a failure.

- **Workflow:**
    - On success: Report completion
    - Otherwise: Report error(s) with logs for diagnostics.
        - Attach relevant log files or error snippets when reporting failures.

## 3 References

- [`.editorconfig`](/.editorconfig)
