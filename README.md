lz-string-csharp
================

C# Class implementation of LZ-String javascript

Based on the LZ-String javascript found here:  http://pieroxy.net/blog/pages/lz-string/index.html


## Installation

- Add the NuGet package to your project: `Install-Package LZString`


## Please Note:

If you are using this in a web service and are sending in strings that are compressed by the Javascript version of LZ-String, then you will want to use the Javascript function `compressToUTF16`.

If you use just the regular Javascript `compress` function, then depending on the data in the string, it may not decompress correctly on the web service side.  However, if you are using the `compress` function built into lz-string-csharp, then you should be ok to use the regular `decompress` function included.


## Contributors ##

lz-string-csharp was created by [jawa-the-hutt](https://github.com/jawa-the-hutt), with several necessary improvements made by [christianrondeau](https://github.com/christianrondeau)

