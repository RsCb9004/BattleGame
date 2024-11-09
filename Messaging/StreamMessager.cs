namespace BattleGame.Messaging {

    public static class StreamReadByte {

        public static byte ReadSingleByte(this Stream stream) {
            byte[] bytes = new byte[1];
            stream.Read(bytes, 0, 1);
            return bytes[0];
        }

        public static async Task<byte> ReadSingleByteAsync(this Stream stream) {
            byte[] bytes = new byte[1];
            await stream.ReadAsync(bytes.AsMemory(0, 1));
            return bytes[0];
        }

    }

    public class StreamMessager : Messager {

        public Stream ThisStream { get; }
        private readonly SemaphoreSlim semaphoreWrite;
        private readonly SemaphoreSlim semaphoreRead;

        public StreamMessager(Stream stream) {
            ThisStream = stream;
            semaphoreWrite = new(1);
            semaphoreRead = new(1);
        }

        public static implicit operator StreamMessager(
            Stream stream
        ) =>
            new(stream);

        public override void Send(byte[] message) {
            semaphoreWrite.Wait();
            try {
                ThisStream.WriteByte((byte)message.Length);
                ThisStream.Write(message);
            } finally {
                semaphoreWrite.Release();
            }
        }

        public override byte[] Get() {
            semaphoreRead.Wait();
            try {
                int len = ThisStream.ReadSingleByte();
                byte[] res = new byte[len];
                int cur = 0;
                while(cur < len) {
                    cur += ThisStream.Read(res, cur, len - cur);
                }
                return res;
            } finally {
                semaphoreRead.Release();
            }
        }

        public override async Task<byte[]> GetAsync() {
            await semaphoreRead.WaitAsync();
            try {
                int len = await ThisStream.ReadSingleByteAsync();
                byte[] res = new byte[len];
                int cur = 0;
                while(cur < len) {
                    cur += await ThisStream.ReadAsync(res.AsMemory(cur, len - cur));
                }
                return res;
            } finally {
                semaphoreRead.Release();
            }
        }

    }

}
