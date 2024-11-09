using System.Text;

namespace BattleGame.Messaging {

    public abstract class Listener {

        public abstract byte[] Get();

        public virtual async Task<byte[]> GetAsync() =>
            await Task.Run(Get);

        public string GetString() =>
            Encoding.UTF8.GetString(Get());

        public async Task<string> GetStringAsync() =>
            Encoding.UTF8.GetString(await GetAsync());

    }

    public abstract class Messager: Listener {

        public abstract void Send(byte[] message);

        public byte[] Exchange(byte[] message) {
            Send(message);
            return Get();
        }

        public async Task<byte[]> ExchangeAsync(byte[] message) {
            Send(message);
            return await GetAsync();
        }

        public T OperateExchange<T>(byte[] message, Func<byte[], byte[], T> operate) =>
            operate.Invoke(message, Exchange(message));

        public async Task<T> OperateExchangeAsync<T>(byte[] message, Func<byte[], byte[], T> operate) =>
            operate.Invoke(message, await ExchangeAsync(message));

        public void SendString(string message) =>
            Send(Encoding.UTF8.GetBytes(message));

        public string ExchangeString(string message) {
            SendString(message);
            return GetString();
        }

        public async Task<string> ExchangeStringAsync(string message) {
            SendString(message);
            return await GetStringAsync();
        }

        public T OperateExchangeString<T>(string message, Func<string, string, T> operate) =>
            operate.Invoke(message, ExchangeString(message));

        public async Task<T> OperateExchangeStringAsync<T>(string message, Func<string, string, T> operate) =>
            operate.Invoke(message, await ExchangeStringAsync(message));

    }

}
