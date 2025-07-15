namespace NICDevice
{
    public record struct _4Byte(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        public byte[] bytes = [byte1, byte2, byte3, byte4];
    }
}
