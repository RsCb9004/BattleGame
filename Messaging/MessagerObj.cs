using Newtonsoft.Json;

namespace BattleGame.Messaging {

    public static class MessagerObj {

        public static T GetObj<T>(this Listener messager) =>
            JsonConvert.DeserializeObject<T>(messager.GetString()) ??
                throw new NullReferenceException();

        public static async Task<T> GetObjAsync<T>(this Listener messager) =>
            JsonConvert.DeserializeObject<T>(await messager.GetStringAsync()) ??
                throw new NullReferenceException();

        public static void SendObj<T>(this Messager messager, T obj) =>
            messager.SendString(JsonConvert.SerializeObject(obj));

    }

}
