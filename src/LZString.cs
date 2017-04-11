using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LZStringCSharp
{
    public class LZString
    {
        private class ContextCompress
        {
            public Dictionary<string, int> Dictionary { get; set; }
            public Dictionary<string, bool> DictionaryToCreate { get; set; }
            public string C { get; set; }
            public string Wc { get; set; }
            public string W { get; set; }
            public int EnlargeIn { get; set; }
            public int DictSize { get; set; }
            public int NumBits { get; set; }
            public ContextCompressData Data { get; set; }
        }

        private class ContextCompressData
        {
            public string Str { get; set; }
            public int Val { get; set; }
            public int Position { get; set; }
        }

        private class DecompressData
        {
            public string Str { get; set; }
            public int Val { get; set; }
            public int Position { get; set; }
            public int Index { get; set; }
        }

        private static ContextCompressData WriteBit(int value, ContextCompressData data)
        {
            data.Val = (data.Val << 1) | value;

            if (data.Position == 15)
            {
                data.Position = 0;
                data.Str += (char)data.Val;
                data.Val = 0;
            }
            else
                data.Position++;

            return data;
        }

        private static ContextCompressData WriteBits(int numbits, int value, ContextCompressData data)
        {
            for (var i = 0; i < numbits; i++)
            {
                data = WriteBit(value & 1, data);
                value = value >> 1;
            }

            return data;
        }

        private static ContextCompress ProduceW(ContextCompress context)
        {
            if (context.DictionaryToCreate.ContainsKey(context.W))
            {
                if (context.W[0] < 256)
                {
                    context.Data = WriteBits(context.NumBits, 0, context.Data);
                    context.Data = WriteBits(8, context.W[0], context.Data);
                }
                else
                {
                    context.Data = WriteBits(context.NumBits, 1, context.Data);
                    context.Data = WriteBits(16, context.W[0], context.Data);
                }

                context = DecrementEnlargeIn(context);
                context.DictionaryToCreate.Remove(context.W);
            }
            else
            {
                context.Data = WriteBits(context.NumBits, context.Dictionary[context.W], context.Data);
            }

            return context;
        }

        private static ContextCompress DecrementEnlargeIn(ContextCompress context)
        {

            context.EnlargeIn--;
            if (context.EnlargeIn == 0)
            {
                context.EnlargeIn = (int)Math.Pow(2, context.NumBits);
                context.NumBits++;
            }
            return context;
        }

        public static string Compress(string uncompressed)
        {
            ContextCompress context = new ContextCompress();
            ContextCompressData data = new ContextCompressData();

            context.Dictionary = new Dictionary<string, int>();
            context.DictionaryToCreate = new Dictionary<string, bool>();
            context.C = "";
            context.Wc = "";
            context.W = "";
            context.EnlargeIn = 2;
            context.DictSize = 3;
            context.NumBits = 2;

            data.Str = "";
            data.Val = 0;
            data.Position = 0;

            context.Data = data;

            foreach (char c in uncompressed)
            {
                context.C = c.ToString();

                if (!context.Dictionary.ContainsKey(context.C))
                {
                    context.Dictionary[context.C] = context.DictSize++;
                    context.DictionaryToCreate[context.C] = true;
                }

                context.Wc = context.W + context.C;

                if (context.Dictionary.ContainsKey(context.Wc))
                {
                    context.W = context.Wc;
                }
                else
                {
                    context = ProduceW(context);
                    context = DecrementEnlargeIn(context);
                    context.Dictionary[context.Wc] = context.DictSize++;
                    context.W = context.C;
                }
            }

            if (context.W != "")
            {
                context = ProduceW(context);
            }

            // Mark the end of the stream
            context.Data = WriteBits(context.NumBits, 2, context.Data);

            // Flush the last char
            while (true)
            {
                context.Data.Val = (context.Data.Val << 1);
                if (context.Data.Position == 15)
                {
                    context.Data.Str += (char)context.Data.Val;
                    break;
                }
                else
                    context.Data.Position++;
            }

            return context.Data.Str;
        }

        private static int ReadBit(DecompressData data)
        {
            var res = data.Val & data.Position;

            data.Position >>= 1;

            if (data.Position == 0)
            {
                data.Position = 32768;

                // This 'if' check doesn't appear in the orginal lz-string javascript code.
                // Added as a check to make sure we don't exceed the length of data.str
                // The javascript charCodeAt will return a NaN if it exceeds the index but will not error out
                if (data.Index < data.Str.Length)
                {
                    data.Val = data.Str[data.Index++]; // data.val = data.string.charCodeAt(data.index++); <---javascript equivilant
                }
            }

            return res > 0 ? 1 : 0;
        }

        private static int ReadBits(int numBits, DecompressData data)
        {
            int res = 0;
            int maxpower = (int)Math.Pow(2, numBits);
            int power = 1;

            while (power != maxpower)
            {
                res |= ReadBit(data) * power;
                power <<= 1;
            }

            return res;
        }

        public static string Decompress(string compressed)
        {
            if (compressed == null) throw new ArgumentNullException(nameof(compressed));
            DecompressData data = new DecompressData();

            List<string> dictionary = new List<string>();
            int next = 0;
            int enlargeIn = 4;
            int numBits = 3;
            string entry = "";
            StringBuilder result = new StringBuilder();
            int i = 0;
            string w = "";
            string sc = "";
            int c = 0;
            int errorCount = 0;

            data.Str = compressed;
            data.Val = compressed[0];
            data.Position = 32768;
            data.Index = 1;

            for (i = 0; i < 3; i++)
            {
                dictionary.Add(i.ToString());
            }

            next = ReadBits(2, data);

            switch (next)
            {
                case 0:
                    sc = Convert.ToChar(ReadBits(8, data)).ToString();
                    break;
                case 1:
                    sc = Convert.ToChar(ReadBits(16, data)).ToString();
                    break;
                case 2:
                    return "";
            }

            dictionary.Add(sc);

            result.Append(sc);
            w = result.ToString();

            while (true)
            {
                c = ReadBits(numBits, data);
                int cc = c;

                switch (cc)
                {
                    case 0:
                        if (errorCount++ > 10000)
                            throw new Exception("To many errors");

                        sc = Convert.ToChar(ReadBits(8, data)).ToString();
                        dictionary.Add(sc);
                        c = dictionary.Count - 1;
                        enlargeIn--;

                        break;
                    case 1:
                        sc = Convert.ToChar(ReadBits(16, data)).ToString();
                        dictionary.Add(sc);
                        c = dictionary.Count - 1;
                        enlargeIn--;

                        break;
                    case 2:
                        return result.ToString();
                }

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }

                if (dictionary.Count - 1 >= c) // if (dictionary[c] ) <------- original Javascript Equivalant
                {
                    entry = dictionary[c];
                }
                else
                {
                    if (c == dictionary.Count)
                    {
                        entry = w + w[0];
                    }
                    else
                    {
                        return null;
                    }
                }

                result.Append(entry);
                dictionary.Add(w + entry[0]);
                enlargeIn--;
                w = entry;

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }
            }
        }

        public static string CompressToUTF16(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            StringBuilder output = new StringBuilder();
            int status = 0;
            int current = 0;

            if (input == null)
                throw new Exception("Input is Null");

            input = Compress(input);
            if (input.Length == 0)
                return input;

            for (int i = 0; i < input.Length; i++)
            {
                int c = input[i];
                switch (status++)
                {
                    case 0:
                        output.Append((char)((c >> 1) + 32));
                        current = (c & 1) << 14;
                        break;
                    case 1:
                        output.Append((char)((current + (c >> 2)) + 32));
                        current = (c & 3) << 13;
                        break;
                    case 2:
                        output.Append((char)((current + (c >> 3)) + 32));
                        current = (c & 7) << 12;
                        break;
                    case 3:
                        output.Append((char)((current + (c >> 4)) + 32));
                        current = (c & 15) << 11;
                        break;
                    case 4:
                        output.Append((char)((current + (c >> 5)) + 32));
                        current = (c & 31) << 10;
                        break;
                    case 5:
                        output.Append((char)((current + (c >> 6)) + 32));
                        current = (c & 63) << 9;
                        break;
                    case 6:
                        output.Append((char)((current + (c >> 7)) + 32));
                        current = (c & 127) << 8;
                        break;
                    case 7:
                        output.Append((char)((current + (c >> 8)) + 32));
                        current = (c & 255) << 7;
                        break;
                    case 8:
                        output.Append((char)((current + (c >> 9)) + 32));
                        current = (c & 511) << 6;
                        break;
                    case 9:
                        output.Append((char)((current + (c >> 10)) + 32));
                        current = (c & 1023) << 5;
                        break;
                    case 10:
                        output.Append((char)((current + (c >> 11)) + 32));
                        current = (c & 2047) << 4;
                        break;
                    case 11:
                        output .Append((char)((current + (c >> 12)) + 32));
                        current = (c & 4095) << 3;
                        break;
                    case 12:
                        output.Append((char)((current + (c >> 13)) + 32));
                        current = (c & 8191) << 2;
                        break;
                    case 13:
                        output .Append((char)((current + (c >> 14)) + 32));
                        current = (c & 16383) << 1;
                        break;
                    case 14:
                        output.Append((char)((current + (c >> 15)) + 32));
                        output.Append((char)((c & 32767) + 32));
                        status = 0;
                        break;
                }
            }

            output.Append((char)(current + 32));
            return output.ToString();
        }

        public static string DecompressFromUTF16(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            StringBuilder output = new StringBuilder();
            int status = 0;
            int current = 0;
            int i = 0;

            if (input == null)
                throw new Exception("input is Null");

            while (i < input.Length)
            {
                int c = input[i] - 32;

                switch (status++)
                {
                    case 0:
                        current = c << 1;
                        break;
                    case 1:
                        output.Append((char)(current | (c >> 14)));
                        current = (c & 16383) << 2;
                        break;
                    case 2:
                        output.Append((char)(current | (c >> 13)));
                        current = (c & 8191) << 3;
                        break;
                    case 3:
                        output.Append((char)(current | (c >> 12)));
                        current = (c & 4095) << 4;
                        break;
                    case 4:
                        output.Append((char)(current | (c >> 11)));
                        current = (c & 2047) << 5;
                        break;
                    case 5:
                        output.Append((char)(current | (c >> 10)));
                        current = (c & 1023) << 6;
                        break;
                    case 6:
                        output.Append((char)(current | (c >> 9)));
                        current = (c & 511) << 7;
                        break;
                    case 7:
                        output.Append((char)(current | (c >> 8)));
                        current = (c & 255) << 8;
                        break;
                    case 8:
                        output.Append((char)(current | (c >> 7)));
                        current = (c & 127) << 9;
                        break;
                    case 9:
                        output.Append((char)(current | (c >> 6)));
                        current = (c & 63) << 10;
                        break;
                    case 10:
                        output.Append((char)(current | (c >> 5)));
                        current = (c & 31) << 11;
                        break;
                    case 11:
                        output.Append((char)(current | (c >> 4)));
                        current = (c & 15) << 12;
                        break;
                    case 12:
                        output.Append((char)(current | (c >> 3)));
                        current = (c & 7) << 13;
                        break;
                    case 13:
                        output.Append((char)(current | (c >> 2)));
                        current = (c & 3) << 14;
                        break;
                    case 14:
                        output.Append((char)(current | (c >> 1)));
                        current = (c & 1) << 15;
                        break;
                    case 15:
                        output.Append((char)(current | c));
                        status = 0;
                        break;
                    default:
                        throw new InvalidOperationException($"Received invalid status: {status}");
                }

                i++;
            }

            return Decompress(output.ToString());
        }

        public static string CompressToBase64(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            const string keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            StringBuilder output = new StringBuilder();

            // Using the data type 'double' for these so that the .Net double.NaN & double.IsNaN functions can be used
            // later in the function. .Net doesn't have a similar function for regular integers.
            double chr1, chr2, chr3 = 0.0;

            int enc1 = 0;
            int enc2 = 0;
            int enc3 = 0;
            int enc4 = 0;
            int i = 0;

            if (input == null)
                throw new Exception("input is Null");

            input = Compress(input);

            while (i < input.Length * 2)
            {
                if (i % 2 == 0)
                {
                    chr1 = input[i / 2] >> 8;
                    chr2 = input[i / 2] & 255;
                    if (i / 2 + 1 < input.Length)
                        chr3 = input[i / 2 + 1] >> 8;
                    else
                        chr3 = double.NaN;//chr3 = NaN; <------ original Javascript Equivalent
                }
                else
                {
                    chr1 = input[(i - 1) / 2] & 255;
                    if ((i + 1) / 2 < input.Length)
                    {
                        chr2 = input[(i + 1) / 2] >> 8;
                        chr3 = input[(i + 1) / 2] & 255;
                    }
                    else
                    {
                        chr2 = chr3 = double.NaN; // chr2 = chr3 = NaN; <------ original Javascript Equivalent
                    }
                }
                i += 3;

                enc1 = enc2 = enc3 = enc4 = 0; //re-set enc variables to zero so that they do not retain their values from previous iterations in the loop.
                enc1 = (int)(Math.Round(chr1)) >> 2;

                // The next three 'if' statements are there to make sure we are not trying to calculate a value that has been 
                // assigned to 'double.NaN' above. The orginal Javascript functions didn't need these checks due to how
                // Javascript functions.
                // Also, due to the fact that some of the variables are of the data type 'double', we have to do some type
                // conversion to get the 'enc' variables to be the correct value.
                if (!double.IsNaN(chr2))
                {
                    enc2 = (((int)(Math.Round(chr1)) & 3) << 4) | ((int)(Math.Round(chr2)) >> 4);
                }

                if (!double.IsNaN(chr2) && !double.IsNaN(chr3))
                {
                    enc3 = (((int)(Math.Round(chr2)) & 15) << 2) | ((int)(Math.Round(chr3)) >> 6);
                }
                // added per issue #3 logged by ReuvenT
                else
                {
                    enc3 = 0;
                }

                if (!double.IsNaN(chr3))
                {

                    enc4 = (int)(Math.Round(chr3)) & 63;
                }

                if (double.IsNaN(chr2)) //if (isNaN(chr2)) <------ original Javascript Equivalent
                {
                    enc3 = enc4 = 64;
                }
                else if (double.IsNaN(chr3)) //else if (isNaN(chr3)) <------ original Javascript Equivalent
                {
                    enc4 = 64;
                }

                output.Append(keyStr[enc1]);
                output.Append(keyStr[enc2]);
                output.Append(keyStr[enc3]);
                output.Append(keyStr[enc4]);
            }

            return output.ToString();
        }

        public static string DecompressFromBase64(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            const string keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

            StringBuilder output = new StringBuilder();
            // ReSharper disable once InconsistentNaming
            int output_ = 0;
            int ol = 0;
            int chr1, chr2, chr3 = 0;
            int enc1, enc2, enc3, enc4 = 0;
            int i = 0;

            if (input == null)
                throw new Exception("input is Null");

            var regex = new Regex(@"[^A-Za-z0-9-\+\/\=]");
            input = regex.Replace(input, "");

            while (i < input.Length)
            {
                enc1 = keyStr.IndexOf(input[i++]);
                enc2 = keyStr.IndexOf(input[i++]);
                enc3 = keyStr.IndexOf(input[i++]);
                enc4 = keyStr.IndexOf(input[i++]);

                chr1 = (enc1 << 2) | (enc2 >> 4);
                chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                chr3 = ((enc3 & 3) << 6) | enc4;

                if (ol % 2 == 0)
                {
                    output_ = chr1 << 8;

                    if (enc3 != 64)
                    {
                        output.Append((char)(output_ | chr2));
                    }

                    if (enc4 != 64)
                    {
                        output_ = chr3 << 8;
                    }
                }
                else
                {
                    output.Append((char)(output_ | chr1));

                    if (enc3 != 64)
                    {
                        output_ = chr2 << 8;
                    }
                    if (enc4 != 64)
                    {
                        output.Append((char)(output_ | chr3));
                    }
                }
                ol += 3;
            }

            // Send the output out to the main decompress function
            return Decompress(output.ToString());
        }

        public static string CompressToEncodedURIComponent(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return Uri.EscapeDataString(Compress(input));
        }

        public static string DecompressFromEncodedURIComponent(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
           
            return Decompress(Uri.UnescapeDataString(input));
        }
    }
}
