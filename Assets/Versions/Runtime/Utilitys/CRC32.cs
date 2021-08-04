using System.IO;

namespace VEngine
{
    public class CRC32
    {
        private const uint _initialResidueValue = 0xFFFFFFFF;

        private static readonly object _globalSync = new object();

        private static uint[] _crc32Table;

        private static readonly byte[][] _maskingBitTable =
        {
            new byte[]
            {
                2
            },
            new byte[]
            {
                0, 3
            },
            new byte[]
            {
                0, 1, 4
            },
            new byte[]
            {
                1, 2, 5
            },
            new byte[]
            {
                0, 2, 3, 6
            },
            new byte[]
            {
                1, 3, 4, 7
            },
            new byte[]
            {
                4, 5
            },
            new byte[]
            {
                0, 5, 6
            },
            new byte[]
            {
                1, 6, 7
            },
            new byte[]
            {
                7
            },
            new byte[]
            {
                2
            },
            new byte[]
            {
                3
            },
            new byte[]
            {
                0, 4
            },
            new byte[]
            {
                0, 1, 5
            },
            new byte[]
            {
                1, 2, 6
            },
            new byte[]
            {
                2, 3, 7
            },
            new byte[]
            {
                0, 2, 3, 4
            },
            new byte[]
            {
                0, 1, 3, 4, 5
            },
            new byte[]
            {
                0, 1, 2, 4, 5, 6
            },
            new byte[]
            {
                1, 2, 3, 5, 6, 7
            },
            new byte[]
            {
                3, 4, 6, 7
            },
            new byte[]
            {
                2, 4, 5, 7
            },
            new byte[]
            {
                2, 3, 5, 6
            },
            new byte[]
            {
                3, 4, 6, 7
            },
            new byte[]
            {
                0, 2, 4, 5, 7
            },
            new byte[]
            {
                0, 1, 2, 3, 5, 6
            },
            new byte[]
            {
                0, 1, 2, 3, 4, 6, 7
            },
            new byte[]
            {
                1, 3, 4, 5, 7
            },
            new byte[]
            {
                0, 4, 5, 6
            },
            new byte[]
            {
                0, 1, 5, 6, 7
            },
            new byte[]
            {
                0, 1, 6, 7
            },
            new byte[]
            {
                1, 7
            }
        };

        private uint _residue = _initialResidueValue;

        internal CRC32()
        {
            lock (_globalSync)
            {
                if (_crc32Table == null) PrepareTable();
            }
        }

        internal uint crc => ~_residue;

        internal uint Compute(Stream stream)
        {
            var buffer = new byte[0x1000];

            for (;;)
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                    Accumulate(buffer, 0, bytesRead);
                else
                    break;
            }

            return crc;
        }

        internal void Accumulate(byte[] buffer, int offset, int count)
        {
            for (var i = offset; i < count + offset; i++)
                _residue = ((_residue >> 8) & 0x00FFFFFF)
                           ^
                           _crc32Table[(_residue ^ buffer[i]) & 0x000000FF];
        }

        internal void ClearCrc()
        {
            _residue = _initialResidueValue;
        }

        private static void PrepareTable()
        {
            _crc32Table = new uint[256];

            for (uint tablePosition = 0; tablePosition < _crc32Table.Length; tablePosition++)
            for (byte bitPosition = 0; bitPosition < 32; bitPosition++)
            {
                var bitValue = false;
                foreach (var maskingBit in _maskingBitTable[bitPosition]) bitValue ^= GetBit(maskingBit, tablePosition);

                SetBit(bitPosition, ref _crc32Table[tablePosition], bitValue);
            }
        }

        private static bool GetBit(byte bitOrdinal, uint data)
        {
            return ((data >> bitOrdinal) & 0x1) == 1;
        }

        private static void SetBit(byte bitOrdinal, ref uint data, bool value)
        {
            if (value) data |= (uint) 0x1 << bitOrdinal;
        }
    }
}