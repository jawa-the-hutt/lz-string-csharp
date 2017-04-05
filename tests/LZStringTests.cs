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
                Uncompressed = "{ \"key\": \"value\" }",
                Compressed = "㞀⁄ൠꘉ츁렐쀶ղ顀张",
                CompressedBase64 = "N4AgRA1gpgnmBc4BuBDANgVymEBfIE==",
                CompressedUTF16 = "ᯠ࠱ǌ઀佐᝘ΐრᬢ峆ࠫ爠"
            };
        }

        [TestCaseSource(nameof(TestCases))]
        public void Compress(LZStringTestCase test)
        {
            Assert.That(LZString.compress(test.Uncompressed), Is.EqualTo(test.Compressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void Decompress(LZStringTestCase test)
        {
            Assert.That(LZString.decompress(test.Compressed), Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompressToBase64(LZStringTestCase test)
        {
            Assert.That(LZString.compressToBase64(test.Uncompressed), Is.EqualTo(test.CompressedBase64));
        }

        [TestCaseSource(nameof(TestCases))]
        public void DecompressFromBase64(LZStringTestCase test)
        {
            Assert.That(LZString.decompressFromBase64(test.CompressedBase64), Is.EqualTo(test.Uncompressed));
        }

        [TestCaseSource(nameof(TestCases))]
        public void CompressToUTF16(LZStringTestCase test)
        {
            Assert.That(LZString.compressToUTF16(test.Uncompressed), Is.EqualTo(test.CompressedUTF16));
        }

        [TestCaseSource(nameof(TestCases))]
        public void DecompressFromUTF16(LZStringTestCase test)
        {
            Assert.That(LZString.decompressFromUTF16(test.CompressedUTF16), Is.EqualTo(test.Uncompressed));
        }

        public struct LZStringTestCase
        {
            public string Uncompressed;
            public string Compressed;
            public string CompressedBase64;
            public string CompressedUTF16;
        }
    }
}