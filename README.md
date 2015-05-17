LiquidState
===========

Efficient state machines for .NET with both synchronous and asynchronous support.
Heavily inspired by the excellent state machine library [**Stateless**](https://github.com/nblumhardt/stateless) by
**Nicholas Blumhardt.**

[![Build status](https://ci.appveyor.com/api/projects/status/6a1pmx2o3jaje60m/branch/master?svg=true)](https://ci.appveyor.com/project/prasannavl/liquidstate/branch/master) [![NuGet downloads](http://img.shields.io/nuget/dt/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState)
[![NuGet stable version](http://img.shields.io/nuget/v/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState) [![NuGet pre version](http://img.shields.io/nuget/vpre/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState)

**NuGet:**

> Install-Package LiquidState

Supported Platforms:
> PCL profile 259: Supports all platforms including Xamarin.iOS and Xamarin.Android.

######Why LiquidState:

- Fully supports async/await methods everywhere => `OnEntry`, `OnExit`, during trigger, and even trigger conditions.
- Builds a linked object graph internally during configuration making it a much faster and more efficient implementation than regular dictionary based implementations.
- MoveToState, to move freely between states, without triggers.
- `PermitDynamic` to support selection of states dynamically on-the-fly.

For documentation, examples on how to use, and release notes, take a look at the [Wiki](https://github.com/prasannavl/LiquidState/wiki/Table-of-Contents). Please use the issue tracker if you'd like to report problems or discuss features. 
