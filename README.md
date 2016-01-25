# DefinitelyTyped.NET

DefinitelyTyped.NET is a library that helps you with sharing your entity-model between your .NET-server and your TypeScript-client.

## Getting started
### Installing the nuget-package
As we are still in pre-release, the command for installing the nuget-package is the following:
```
PM> Install-Package DefinitelyTyped.NET -Pre
```
The nuget-package contains an executable. This executable contains a TypeScript-attribute. When executed, it will generate a TypeScript-interface, -enum or -class for every .NET-interface, -enum or -class with the TypeScript-attribute on it.

### Setting up your project
We want to generate and re-generate our TypeScript-types every time we change our .NET-types. 
The Post-Build-event seems the right moment to generate our types. The command you need to run for generating your types is the following:
```
$(SolutionDir)<ProjectName>\bin\DefinitelyTyped.Net.exe -i "$(TargetPath)" -o "$(SolutionDir)<ProjectName>\<Directory>\<GeneratedFileName>.ts"
```
An example of the command would be:
```
$(SolutionDir)My.Awesome.TypeScript.Project\bin\DefinitelyTyped.Net.exe -i "$(TargetPath)" -o "$(SolutionDir)My.Awesome.TypeScript.Project\apps\generated-types.ts"
```
Now you're all set. Every time you build your project, your TypeScript-types will be in sync with your .NET-types.
