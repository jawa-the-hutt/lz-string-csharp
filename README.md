lz-string-csharp
================

C# Class implementation of LZ-String javascript

Based on the LZ-String javascript found here:  http://pieroxy.net/blog/pages/lz-string/index.html

Import the Class file into your project and change your namespace to match.


Important:

If you are using this in a web service and are sending in strings that are compressed by the Javascript version of LZ-String, then you will want to use the Javascript function 'compressToUTF16'.

If you use just the regular Javascript 'compress' function then depending on the data in the string, it will not decompress correctly on the C# side.


However, if you are using the 'compress' function built into this C# version, then you should be ok to use the regular 'decompress' function included.

