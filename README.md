LiquidState
====

Efficient state machines for .NET with both synchronous and asynchronous support.
Heavily inspired by the excellent state machine library [**Stateless**](https://github.com/nblumhardt/stateless) by
**Nicholas Blumhardt.**

[![Build status](https://ci.appveyor.com/api/projects/status/6a1pmx2o3jaje60m/branch/master?svg=true)](https://ci.appveyor.com/project/prasannavl/liquidstate/branch/master) [![NuGet downloads](http://img.shields.io/nuget/dt/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState)
[![NuGet stable version](http://img.shields.io/nuget/v/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState) [![NuGet pre version](http://img.shields.io/nuget/vpre/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState)

Installation
----

**NuGet:**

> Install-Package LiquidState

**Supported Platforms:**
> PCL profile 259: Supports all platforms including Xamarin.iOS and Xamarin.Android.

Highlights
----

- Fully supports async/await methods everywhere => `OnEntry`, `OnExit`, during trigger, and even trigger conditions.
- Builds a linked object graph internally during configuration making it a much faster and more efficient implementation than regular dictionary based implementations.
- MoveToState, to move freely between states, without triggers.
- `PermitDynamic` to support selection of states dynamically on-the-fly.
- `Diagnostics` in-built to check for validity of triggers, and currently available triggers.

Documentation
----

For documentation, examples on how to use, and more detailed information, take a look at the [Wiki](https://github.com/prasannavl/LiquidState/wiki/Table-of-Contents).
Documentation is still a work in progress, and any help is appreciated.


Support
----

- **Release Notes:** They're a part of the wiki, [here](https://github.com/prasannavl/LiquidState/wiki/Release-Notes).

- **Bugs & Issues:** Please use the GitHub issue tracker [here](https://github.com/prasannavl/LiquidState/issues) if you'd like to report problems or discuss features. As always, do a preliminary search in the issue tracker before opening new ones - (*Tip:* include pull requests, closed, and open issues: *[Exhaustive search](https://github.com/prasannavl/LiquidState/issues?q=)* ).


Contributions
----

If this project has helped you, the best way to `say thanks` is to contribute back :)

Any commits into the `master` branch will be automatically built and deployed as `pre-release` nuget packages.

- **Wiki and documentation:** Feel free to add new, modify out-dated or remove irrelevant information. Infact, this is a very important part the project currently that currently lags behind - Any contributions are highly appreciated.

- **Non-invasive contributions:** Direct pull requests for bug-fixes and any contributions that do not include conceptual changes, or introduce new features are welcome, and much appreciated :)

- **Invasive contributions:** Any contribution that may introduce new API surface, features, or includes either conceptual or implementational changes are considered invasive. It is **highly recommended** that an issue is opened first (which will be labelled accordingly), so that design notes or at-least a quick review on the direction is discussed, before any large effort is made. There's nothing I hate more than good contributions that go unmerged.
