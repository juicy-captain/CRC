using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CRC
{
    class Program
    {
        static void Main(string[] args)
        {
            Crc32 crc32 = new Crc32();
            String hash = String.Empty;

            Console.Write("Enter Path: ");
            string path = Console.ReadLine();

            var dir = new DirectoryInfo(path);
            var files = new List<string>();
            var CRC = new List<string>();
            foreach (FileInfo file in dir.GetFiles()) 
            {                  
                using (FileStream fs = File.Open(file.FullName, FileMode.Open))
                foreach (byte b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToLower();

                files.Add(file.FullName); 
                CRC.Add(file.FullName + " - " + hash);
            }

            foreach (var element in CRC)
            { 
                Console.WriteLine(element);
            }

            path = path + "/CRC.txt";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (FileStream file = File.Create(path))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes("");                 
                file.Write(info, 0, info.Length);
            }

            using (System.IO.StreamWriter file_dump =
            new System.IO.StreamWriter(@path, true))
            {
                foreach (var element in CRC)
                {
                    file_dump.WriteLine(element);
                }

                file_dump.WriteLine();
            }

            Console.WriteLine(" \t \t  Dump Done");
                      
            //Комменты у меня (с) Константин Лебейко

            Console.ReadKey();
        }
        
        public sealed class Crc32 : HashAlgorithm
        {
            public const UInt32 DefaultPolynomial = 0xedb88320u;
            public const UInt32 DefaultSeed = 0xffffffffu;

            static UInt32[] defaultTable;

            readonly UInt32 seed;
            readonly UInt32[] table;
            UInt32 hash;

            public Crc32()
                : this(DefaultPolynomial, DefaultSeed)
            {
            }

            public Crc32(UInt32 polynomial, UInt32 seed)
            {
                table = InitializeTable(polynomial);
                this.seed = hash = seed;
            }

            public override void Initialize()
            {
                hash = seed;
            }

            protected override void HashCore(byte[] array, int ibStart, int cbSize)
            {
                hash = CalculateHash(table, hash, array, ibStart, cbSize);
            }

            protected override byte[] HashFinal()
            {
                var hashBuffer = UInt32ToBigEndianBytes(~hash);
                HashValue = hashBuffer;
                return hashBuffer;
            }

            public override int HashSize { get { return 32; } }

            public static UInt32 Compute(byte[] buffer)
            {
                return Compute(DefaultSeed, buffer);
            }

            public static UInt32 Compute(UInt32 seed, byte[] buffer)
            {
                return Compute(DefaultPolynomial, seed, buffer);
            }

            public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
            {
                return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
            }

            static UInt32[] InitializeTable(UInt32 polynomial)
            {
                if (polynomial == DefaultPolynomial && defaultTable != null)
                    return defaultTable;

                var createTable = new UInt32[256];
                for (var i = 0; i < 256; i++)
                {
                    var entry = (UInt32)i;
                    for (var j = 0; j < 8; j++)
                        if ((entry & 1) == 1)
                            entry = (entry >> 1) ^ polynomial;
                        else
                            entry = entry >> 1;
                    createTable[i] = entry;
                }

                if (polynomial == DefaultPolynomial)
                    defaultTable = createTable;

                return createTable;
            }

            static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size)
            {
                var crc = seed;
                for (var i = start; i < size - start; i++)
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                return crc;
            }

            static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
            {
                var result = BitConverter.GetBytes(uint32);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(result);

                return result;
            }
        }
    }
}
