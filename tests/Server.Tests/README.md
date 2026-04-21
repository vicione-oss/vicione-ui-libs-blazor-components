# Tests for `Server` application

[[_TOC_]]

## Introduction

This project contains tests running and testing aspects of the [`Server`](../../samples/Server) application.

## Run tests

- Build project
- Open `PowerShell` console at `bin/Debug/net10.0`
- Run `./playwright install`

  > If the command throws `TypeNotFound` error, make sure to use an up-to-date version of `PowerShell` by calling `dotnet tool update --global PowerShell`.

## Create test using tools available for Playwright

The first step is to run the `Server` project to provide a testable application as described, see section `Get Started` in [README](../../README.md#get-started) in the root directory.

Then, you can use either [`Playwright Codegen`](https://playwright.dev/dotnet/docs/codegen) or [`Playwright MCP`](https://playwright.dev/dotnet/docs/getting-started-mcp) to create tests for the `Server` application.

## Additional resources

- [`Playwright .NET installation`](https://playwright.dev/dotnet/docs/intro#introduction)
