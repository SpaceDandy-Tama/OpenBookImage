using System;

namespace LibObiNet
{
    [Flags]
    public enum ObiFlags : byte
    {
        None = 0,                   // No flags set
        RLE = 0x01,                 // Bit 0: Run Length Encoding flag
        ReservedBit1 = 0x02,        // Bit 1: Reverved
        ReservedBit2 = 0x04,        // Bit 2: Reserved
        ReservedBit3 = 0x08,        // Bit 3: Reserved
        ReservedBit4 = 0x10,        // Bit 4: Reserved
        ReservedBit5 = 0x20,        // Bit 5: Reserved
        ReservedBit6 = 0x40,        // Bit 6: Reserved
        ReservedBit7 = 0x80         // Bit 7: Reserved
    }
}