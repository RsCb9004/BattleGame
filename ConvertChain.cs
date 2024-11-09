namespace BattleGame {

    public delegate bool TryParse<T, TOut>(T input, out TOut output);

    public abstract class ConvertChain<T> {

        public abstract T GetValue();
        public abstract bool TryGetValue(out T output);

        public ConvertChain<TOut> TryConvert<TOut>(
            TryParse<T, TOut> parser,
            Action? onErr = null
        )
            => new ConvertChainBody<TOut, T>(this, parser, onErr);

        public ConvertChain<TOut> Convert<TOut>(
            Converter<T, TOut> converter
        )
            => new ConvertChainBody<TOut, T>(
                this,
                delegate (T input, out TOut output) {
                    output = converter.Invoke(input);
                    return true;
                },
                null
            );

        public ConvertChain<T> Check(
            Converter<T, bool> checker,
            Action? onErr = null
        )
            => new ConvertChainBody<T, T>(
                this,
                delegate (T input, out T output) {
                    output = input;
                    return checker.Invoke(input);
                },
                onErr
            );

    }

    public class ConvertChainBody<T, TPrev> : ConvertChain<T> {

        private readonly ConvertChain<TPrev> prev;
        private readonly TryParse<TPrev, T> parser;
        private readonly Action? onErr;

        public ConvertChainBody(
            ConvertChain<TPrev> prev,
            TryParse<TPrev, T> parser,
            Action? onErr = null
        ) {
            this.prev = prev;
            this.parser = parser;
            this.onErr = onErr;
        }

        public override T GetValue() {
            var val = prev.GetValue();
            if(parser.Invoke(val, out T res)) {
                return res;
            } else {
                onErr?.Invoke();
                return GetValue();
            }
        }

        public override bool TryGetValue(out T output) {
            output = default!;
            if(!prev.TryGetValue(out TPrev val))
                return false;
            if(!parser.Invoke(val, out output)) {
                onErr?.Invoke();
                return false;
            }
            return true;
        }

    }

    public class ConvertChainStart<T> : ConvertChain<T> {

        private readonly Func<T> getVal;

        public ConvertChainStart(Func<T> getVal) {
            this.getVal = getVal;
        }

        public ConvertChainStart(T val) {
            getVal = () => val;
        }

        public override T GetValue() => getVal.Invoke();

        public override bool TryGetValue(out T output) {
            output = getVal.Invoke();
            return true;
        }

    }

}
