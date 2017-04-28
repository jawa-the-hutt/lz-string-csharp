lz-string-csharp
================

C# Class implementation of [lz-string](http://pieroxy.net/blog/pages/lz-string/index.html) (based on https://github.com/pieroxy/lz-string)


## Installation

- Install with NuGet: `Install-Package LZStringCSharp`, or;
- Downloaded from the [Releases](https://github.com/jawa-the-hutt/lz-string-csharp/releases) page


## Please Note

If you plan on using this library for data sent  by the browser (e.g. compressed in JavaScript in the browser, and sent to the server using HTTP), do *not* use `compressToUTF16` and `DecompressFromUTF16`. Safari will mangle up the data by converting to UTF-8, breaking the actual bytes of the request. Instead, you should use `compressToEncodedURIComponent` `DecompressFromEncodedURIComponent`.


## Contributors

lz-string-csharp was created by [jawa-the-hutt](https://github.com/jawa-the-hutt), with several necessary improvements made by [christianrondeau](https://github.com/christianrondeau)
