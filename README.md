# Sancho.XAMLParser

## Simple questions and simple answers 

Q: What is this repo all about?
> A: The idea is to parse XAML files used in Xamarin.Forms.

Q: Couldn't you simply use page.LoadFromXaml() instead?
> A: Where is the fun in that!

Q: Once I parse XAML docs, what then?
> A: Well, you can create real controls and pages on the fly.

Q: Errm, same as LoadFromXaml? Is the library complete, can I throw in any valid XAML?
> A: Not yet! But we are getting there

Q: So what is the point? Xamarin.Forms has built in parser already which is complete and mostly bug free.
> A: Well, by having our own implementation we can do funky stuff. And it won't crash if there are errors in XAML.

Q: Like?
> A: Virtual DOM, incremental DOM, new incompatible features, preprocessor languages like HTML has, optimizations, etc.

Q: Cool, when will it be done?
> A: It is a work in progress :)

## Build instructions

So far the library has been built using Xamarin Studio 6. It is assumed that Visual Studio 2015 will compile it just fine.

## Usage

Sorry for that as the library architecture is still in flux.

## Completeness

Missing:

 - Full markup extensions support
 - Correct DataTemplates implementation
 - Style/OnPlatform are not completely implemented
 - support for custom controls
 - correct XML namespace handling and _x:_ directives
 - binding to self
 - docs and NuGet packages


## License

Library is licensed under MIT.

## Contributions

They are welcome, but talk to me first please.