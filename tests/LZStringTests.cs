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
                CompressedBase64 = "456A4oGE4LWg6piJ7piF7LiB66CQ7IC21bLpoYDlvKA=",
                CompressedUTF16 = "ᯠ࠱ǌ઀佐᝘ΐრᬢ峆ࠫ爠",
                CompressedEncodedURIComponent = "%E3%9E%80%E2%81%84%E0%B5%A0%EA%98%89%EE%98%85%EC%B8%81%EB%A0%90%EC%80%B6%D5%B2%E9%A1%80%E5%BC%A0"
            };

            yield return new LZStringTestCase
            {
                Name = "UTF-8 String",
                Uncompressed = "ユニコード",
                Compressed = "駃⚘操ಃ錌䀀",
                CompressedBase64 = "6aeD4pqY5pON4LKD74OI6YyM5ICA",
                CompressedUTF16 = "䴁䧆ಹ僨ᾦ≬ᢠ ",
                CompressedEncodedURIComponent = "%E9%A7%83%E2%9A%98%E6%93%8D%E0%B2%83%EF%83%88%E9%8C%8C%E4%80%80"
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
        public void NodeCompatible(LZStringTestCase test)
        {
            Assert.That(RunNodeLzString(test.Uncompressed), Is.EqualTo(test.Uncompressed));
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

        private static string RunNodeLzString(string value)
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, value);
            try
            {
                var output = new StringBuilder();
                var error = new StringBuilder();
                // ReSharper disable once AssignNullToNotNullAttribute
                var workingDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory), "../../"));
                if (!Directory.Exists(Path.Combine(workingDirectory, "node_modules", "lz-string")))
                    Assert.Inconclusive( $"lz-string is not installed. Use `npm install` in `{workingDirectory}` and re-run tests.");
                using (var process = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = workingDirectory,
                        FileName = "cmd.exe",
                        Arguments = $"cmd /C node -e \"var lzString = require('lz-string'); console.log(lzString.compress(fs.readFileSync('{tempFile.Replace("\\", "/")}', {{encoding: 'utf8'}})));\"",
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
                return output.ToString();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}