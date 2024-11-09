using System.Collections.Concurrent;

namespace BattleGame.Messaging {

    public class MultiTunnelMessager {

        public class Tunnel : Messager {

            private readonly Func<byte[]> get;
            private readonly Func<Task<byte[]>> getAsync;
            private readonly Action<byte[]> send;

            public Tunnel(
                Func<byte[]> get,
                Func<Task<byte[]>> getAsync,
                Action<byte[]> send
            ) {
                this.get = get; this.getAsync = getAsync; this.send = send;
            }

            public override byte[] Get() => get.Invoke();
            public override async Task<byte[]> GetAsync() => await getAsync.Invoke();
            public override void Send(byte[] message) => send.Invoke(message);
        }

        public int TunnelCount { get; }

        public Messager BaseMessager { get; }

        private readonly ConcurrentQueue<byte[]>[] messageQues;
        private readonly Tunnel[] tunnels;

        private readonly SemaphoreSlim semaphoreRead;
        private int time;

        private readonly SemaphoreSlim semaphoreWrite;

        public MultiTunnelMessager(Messager baseMessager, int tunnelCount = 1) {
            BaseMessager = baseMessager; TunnelCount = tunnelCount;
            messageQues = new ConcurrentQueue<byte[]>[TunnelCount];
            tunnels = new Tunnel[TunnelCount];
            semaphoreRead = new(1);
            time = 0;
            semaphoreWrite = new(1);
            for(int i = 0; i < TunnelCount; i++) {
                int index = i;
                messageQues[i] = new();
                tunnels[i] = new(
                    () => GetBytes(index),
                    () => GetBytesAsync(index),
                    (message) => SendBytes(index, message)
                );
            }
        }

        public Tunnel this[int index] => tunnels[index];

        private void Get() {
            byte[] bytes = BaseMessager.Get();
            int index = bytes[0];
            byte[] message = bytes[1..];
            messageQues[index].Enqueue(message);
            time++;
        }

        private byte[] GetBytes(int index) {
            int prevTime = -1;
            while(true) {
                semaphoreRead.Wait();
                try {
                    if(messageQues[index].TryDequeue(out byte[]? bytes))
                        return bytes;
                    if(time == prevTime)
                        Get();
                    prevTime = time;
                } finally {
                    semaphoreRead.Release();
                }
            }
        }

        private async Task GetAsync() {
            byte[] bytes = await BaseMessager.GetAsync();
            int index = bytes[0];
            byte[] message = bytes[1..bytes.Length];
            messageQues[index].Enqueue(message);
            time++;
        }

        private async Task<byte[]> GetBytesAsync(int index) {
            int prevTime = -1;
            while(true) {
                await semaphoreRead.WaitAsync();
                try {
                    if(messageQues[index].TryDequeue(out byte[]? bytes))
                        return bytes;
                    if(time == prevTime)
                        await GetAsync();
                    prevTime = time;
                } finally {
                    semaphoreRead.Release();
                }
            }
        }

        private void SendBytes(int index, byte[] message) {
            semaphoreWrite.Wait();
            try {
                byte[] bytes = new byte[message.Length + 1];
                bytes[0] = (byte)index;
                message.CopyTo(bytes, 1);
                BaseMessager.Send(bytes);
            } finally {
                semaphoreWrite.Release();
            }
        }

    }

}
