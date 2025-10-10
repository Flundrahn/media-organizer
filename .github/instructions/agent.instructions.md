---
applyTo: '**'
---
Do not add unnecessary comments to code, using clear naming is enough.

Commit messages should end with . as full sentences.

## Terminal Management

### Primary Development Task (Background)
Always maintain continuous test watcher using VS Code tasks:

**Command**: `run_task(workspaceFolder, "test-watch")`
**Purpose**: Instant feedback on build/test failures across entire solution
**Usage**:
1. Start at session beginning with `run_task`
2. Keep running throughout session  
3. Check output with `get_task_output` after changes
4. Never stop unless specifically needed

### Secondary Terminal (Interactive)
Use for manual commands: builds, git, app runs, PowerShell

### Workflow
1. Start `test-watch` task in background
2. Make changes, check test output, manual operations in secondary terminal

## Development Notes
- Maintain comprehensive test coverage for new features
- Follow .NET best practices and patterns
- Consider performance impact of new features
- Prioritize core functionality over nice-to-have features
