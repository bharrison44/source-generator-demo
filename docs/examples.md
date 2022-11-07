# Example Generators

The following subsections describe each of the example source generators included in the repository.

All example source generators examples are

* configuration to allow debugging of the generator
* able to be used as either a project reference or NuGet package
* an entrypoint project, which use the source generator
* commented `csproj` files explaining configuration

## SimpleGenerator

A small source generator, generating static code. Demonstrates

* basic functionally of a source generator
* the minimum required boilerplate for a source generator

## MapperGenerator

A no-frills AutoMapper style type mapping source generator. Demonstrates

* using a syntax receiver to find specific syntax usings in the analysed code
* getting richer semantic information from the collected syntax
* using semantic information to dynamically generate code

## BoilerplateGenerator

Fills out boilerplate for a partial class to implement some functionality. Demonstrates

* generating partial classes to extend the functionality of the non-generated partial
* generating code that enforces correct usage via compiler (invalid states are unrepresentable principle)

## ApiGenerator

An ASP.NET Core Web API which models and controllers generated from a JSON file. Demonstrates

* using non-code files in source generators
* how NuGet packages need to be used in source generators
* how a source generator can generate multiple files
