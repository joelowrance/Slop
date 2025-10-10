# Cursor AI Rules for VerdaVidaLawnCare

This directory contains Cursor AI assistant rules, templates, and context files to help maintain consistency and provide guidance during development.

## Directory Structure

### `rules/`
Contains detailed coding standards, architecture guidelines, and domain knowledge:
- `error-handling.md` - Error handling patterns and Result<T> implementation
- `coding-standards.md` - C# and general coding conventions
- `architecture.md` - Project structure and design patterns
- `project-overview.md` - High-level project description and goals
- `domain-knowledge.md` - Lawn care business domain information
- `api-controller.md` - REST API controller template
- `entity-model.md` - Entity Framework model template

### `snippets/`
Code snippets for quick insertion:
- `csharp-snippets.json` - C# code snippets for common patterns

### `workflows/`
Predefined development workflows and processes:
- `feature-development.md` - Complete feature development lifecycle
- `api-endpoint-creation.md` - Step-by-step API endpoint creation
- `database-migration.md` - Safe database migration process
- `testing-workflow.md` - Comprehensive testing strategy
- `debugging-workflow.md` - Systematic debugging approach

## Usage

These files help Cursor AI understand:
- Your project's architecture and conventions
- Business domain knowledge
- Code patterns and templates
- Coding standards and best practices

The AI will reference these files when:
- Generating new code
- Refactoring existing code
- Answering questions about the project
- Suggesting improvements

## Customization

Feel free to modify these files to better match your specific needs:
- Update coding standards to match your team's preferences
- Add new templates for common patterns in your project
- Expand domain knowledge as the project grows
- Add new snippets for frequently used code patterns

## Migration from .cursorrules

This project has been migrated from the deprecated `.cursorrules/` directory structure to the current recommended `.cursor/rules/` structure for better organization and maintainability.
