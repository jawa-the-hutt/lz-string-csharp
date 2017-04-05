using System.Collections.Generic;
using lz_string_csharp;
using NUnit.Framework;

namespace lz_string_csharp_tests
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
                CompressedUTF16 = "ᯠ࠱ǌ઀佐᝘ΐრᬢ峆ࠫ爠"
            };

            yield return new LZStringTestCase
            {
                Name = "UTF-8 String",
                Uncompressed = "ユニコード",
                Compressed = "駃⚘操ಃ錌䀀",
                CompressedBase64 = "mcMmmGTNDIPwyJMMQAA=",
                CompressedUTF16 = "䴁䧆ಹ僨ᾦ≬ᢠ "
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

        public struct LZStringTestCase
        {
            public string Name;
            public string Uncompressed;
            public string Compressed;
            public string CompressedBase64;
            public string CompressedUTF16;
            public override string ToString() => Name;
        }
    }
}