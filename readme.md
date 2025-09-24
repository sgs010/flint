Flint is EF Core linter tool. Good to check things after vibe coders. 

This is CLI tool designed for CI/CD pipeline. Flint processes compiled assemblies, no source code is required.

Supported checks:
1. Consider using projection {...} - avoid SELECT * FROM issue.
2. Add Include(...) - avoid N+1 problem.
3. Add AsNoTracking() - reduce memory usage.
4. Consider adding AsSplitQuery() - avoid cartesian explosion.

Usage: flint --input=<path_to_assembly>
