# Gifted Web App (frontend)

A static web app that runs on WASM, written in C# with Blazor.

## Stack

C#, Blazor Webassembly, Javascript, HTML, CSS

## Hosting infrastructure

Azure Static Web App with easy auth enabled


# Gifted Web API Server (backend)

An ASP.NET Core Web API, targeting latest .NET, that powers the Gifted frontend by
storing data and exposing HTTP endpoints.

## Stack

C#, ASP.NET, SQLite

## Hosting infrastructure

Azure Container App instance with ephemeral storage, periodically backed up to azure files

## Endpoints (C# controllers)
* `/api/gift/*` - Get/add/delete a gift for a person
* `/api/person/*` - Get/add/delete a person
* `/api/store/*` - Get/add/delete favorite stores for your people
* `/api/user/*` - Retrieve user info and login/register

## Database
All data is stored in a SQLite file which is periodically backed up to Azure storage.
