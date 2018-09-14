using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace LZStringCSharp.Tests
{
    public class LZStringTests
    {
        public static IEnumerable<LZStringTestCase> TestCases()
        {
            yield return new LZStringTestCase
            {
                Name = "Json",
                Uncompressed = "{ \"key\": \"value\" }",
                Compressed = "㞀⁄ൠꘉ츁렐쀶ղ顀张",
                CompressedBase64 = "N4AgRA1gpgnmBc4BuBDANgVymEBfIA==",
                CompressedUTF16 = "ᯠ࠱ǌ઀佐᝘ΐრᬢ峆ࠫ爠",
                CompressedEncodedURIComponent = "N4AgRA1gpgnmBc4BuBDANgVymEBfIA"
            };

            yield return new LZStringTestCase
            {
                Name = "UTF-8 String",
                Uncompressed = "ユニコード",
                Compressed = "駃⚘操ಃ錌䀀",
                CompressedBase64 = "mcMmmGTNDIPwyJMMQ===",
                CompressedUTF16 = "䴁䧆ಹ僨ᾦ≬ᢠ",
                CompressedEncodedURIComponent = "mcMmmGTNDIPwyJMMQ"
            };

            yield return new LZStringTestCase
            {
                Name = "Edge Case 1",
                Uncompressed = "000",
                Compressed = "̊\0",
                CompressedBase64 = "Awo=",
                CompressedUTF16 = "ƥ ",
                CompressedEncodedURIComponent = "Awo"
            };
        }

        [TestCaseSource(nameof(TestCases))]
        public void Compress(LZStringTestCase test)
        {
            Assert.That(LZString.Compress(test.Uncompressed), Is.EqualTo(test.Compressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void Decompress(LZStringTestCase test)
        {
            Assert.That(LZString.Decompress(test.Compressed), Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompressToBase64(LZStringTestCase test)
        {
            Assert.That(LZString.CompressToBase64(test.Uncompressed), Is.EqualTo(test.CompressedBase64));
        }

        [TestCaseSource(nameof(TestCases))]
        public void DecompressFromBase64(LZStringTestCase test)
        {
            Assert.That(LZString.DecompressFromBase64(test.CompressedBase64), Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompressToUTF16(LZStringTestCase test)
        {
            Assert.That(LZString.CompressToUTF16(test.Uncompressed), Is.EqualTo(test.CompressedUTF16));
        }

        [TestCaseSource(nameof(TestCases))]
        public void DecompressFromUTF16(LZStringTestCase test)
        {
            Assert.That(LZString.DecompressFromUTF16(test.CompressedUTF16), Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompressToEncodedURIComponent(LZStringTestCase test)
        {
            Assert.That(LZString.CompressToEncodedURIComponent(test.Uncompressed), Is.EqualTo(test.CompressedEncodedURIComponent));
        }

        [TestCaseSource(nameof(TestCases))]
        public void DecompressFromEncodedURIComponent(LZStringTestCase test)
        {
            Assert.That(LZString.DecompressFromEncodedURIComponent(test.CompressedEncodedURIComponent), Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompatibilityDecompressEncodedURIComponentFromNode(LZStringTestCase test)
        {
            var compress = RunNodeLzString("compressToEncodedURIComponent", test.Uncompressed);
            var uncompress = "";
            try
            {
                uncompress = LZString.DecompressFromEncodedURIComponent(compress);
            }
            catch (FormatException exc)
            {
                Assert.Fail($"Invalid EncodedURIComponent string: '{compress}'{Environment.NewLine}{exc.Message}");
            }
            Console.WriteLine("lz-string compression result:");
            Console.WriteLine(compress);
            Assert.That(uncompress, Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompatibilityCompressEncodedURIComponentFromCSharp(LZStringTestCase test)
        {
            var compress = LZString.CompressToEncodedURIComponent(test.Uncompressed);
            var uncompress = RunNodeLzString("decompressFromEncodedURIComponent", compress);
            Console.WriteLine("c# compression result:");
            Console.WriteLine(compress);
            Assert.That(uncompress, Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompatibilityDecompressBase64FromNode(LZStringTestCase test)
        {
            var compress = RunNodeLzString("compressToBase64", test.Uncompressed);
            var uncompress = "";
            try
            {
                uncompress = LZString.DecompressFromBase64(compress);
            }
            catch (FormatException exc)
            {
                Assert.Fail($"Invalid Base64 string: '{compress}'{Environment.NewLine}{exc.Message}");
            }
            Console.WriteLine("lz-string compression result:");
            Console.WriteLine(compress);
            Assert.That(uncompress, Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompatibilityCompressBase64FromCSharp(LZStringTestCase test)
        {
            var compress = LZString.CompressToBase64(test.Uncompressed);
            var uncompress = RunNodeLzString("decompressFromBase64", compress);
            Console.WriteLine("c# compression result:");
            Console.WriteLine(compress);
            Assert.That(uncompress, Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompatibilityDecompressUTF16FromNode(LZStringTestCase test)
        {
            var compress = RunNodeLzString("compressToUTF16", test.Uncompressed);
            var uncompress = LZString.DecompressFromUTF16(compress);
            Console.WriteLine("lz-string compression result:");
            Console.WriteLine(compress);
            Assert.That(uncompress, Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompatibilityCompressUTF16FromCSharp(LZStringTestCase test)
        {
            var compress = LZString.CompressToUTF16(test.Uncompressed);
            var uncompress = RunNodeLzString("decompressFromUTF16", compress);
            Console.WriteLine("c# compression result:");
            Console.WriteLine(compress);
            Assert.That(uncompress, Is.EqualTo(test.Uncompressed));
        }

        [Test]
        [Explicit]
        public void CompressToBase64Performance()
        {
            var value = new string('x', 65536);
            LZString.CompressToBase64(value); // Warmup

            var timer = new Stopwatch();
            timer.Start();
            const int times = 1024;

            for (var i = 0; i < times; i++)
                LZString.CompressToBase64(value);

            timer.Stop();
            Assert.Pass($"Did {times} compressions in {timer.Elapsed.TotalSeconds}s. Average: {timer.ElapsedMilliseconds / times}ms");
        }
        
        [Test]
        [Explicit]
        public void DecompressFromBase64Performance()
        {
            var value = new string('x', 65536);
            value = LZString.CompressToBase64(value);
            LZString.DecompressFromBase64(value); // Warmup

            var timer = new Stopwatch();
            timer.Start();
            const int times = 1024;

            for (var i = 0; i < times; i++)
                LZString.DecompressFromBase64(value);

            timer.Stop();
            Assert.Pass($"Did {times} decompressions in {timer.Elapsed.TotalSeconds}s. Average: {timer.ElapsedMilliseconds / times}ms");
        }

        public struct LZStringTestCase
        {
            public string Name;
            public string Uncompressed;
            public string Compressed;
            public string CompressedBase64;
            public string CompressedUTF16;
            public string CompressedEncodedURIComponent;
            public override string ToString() => Name;
        }

        private static string RunNodeLzString(string fn, string value)
        {
            var inputTempFile = Path.GetTempFileName();
            var outputTempFile = Path.GetTempFileName();
            File.WriteAllText(inputTempFile, value);
            try
            {
                var output = new StringBuilder();
                var error = new StringBuilder();
                // ReSharper disable once AssignNullToNotNullAttribute
                var workingDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory), "../../../"));
                if (!Directory.Exists(Path.Combine(workingDirectory, "node_modules", "lz-string")))
                    Assert.Inconclusive( $"lz-string is not installed. Use `npm install` in `{workingDirectory}` and re-run tests.");
                using (var process = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = workingDirectory,
                        FileName = "cmd.exe",
                        Arguments = $"cmd /C node -e \"var lzString = require('lz-string'); fs.writeFileSync('{outputTempFile.Replace('\\', '/')}', (lzString.{fn}(fs.readFileSync('{inputTempFile.Replace('\\', '/')}', {{encoding: 'utf8'}}))));\"",
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                })
                {
                    process.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
                    process.ErrorDataReceived += (sender, args) => error.AppendLine(args.Data);
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    if (!process.WaitForExit(10 * 1000))
                        Assert.Fail($"Node timeout: {output}{Environment.NewLine}{error}");
                }

                if(error.Length > 4) // Two line breaks
                    Assert.Fail($"Node error: {error}");
                var result = File.ReadAllText(outputTempFile);
                if(string.IsNullOrEmpty(result))
                    Assert.Fail($"lz-string did not write output. Full output: '{output}'");
                return result;
            }
            finally
            {
                if (File.Exists(inputTempFile))
                    File.Delete(inputTempFile);
                if (File.Exists(outputTempFile))
                    File.Delete(outputTempFile);
            }
        }
    }
}