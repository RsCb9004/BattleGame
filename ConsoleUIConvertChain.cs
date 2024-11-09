namespace BattleGame {

    public static class ConsoleUIConvertChain {

        public static ConvertChainStart<string> Ask(
            ConsoleColor FontCol,
            ConsoleColor BackCol,
            string text = ""
        )
            => new(() => ConsoleUI.Ask(FontCol, BackCol, text));

        public static ConvertChainStart<string> Ask(string text = "")
            => new(() => ConsoleUI.Ask(text));

        public static ConvertChain<TOut> TryConvert<T, TOut>(
            this ConvertChain<T> chain,
            TryParse<T, TOut> parser,
            string errText
        )
            => chain.TryConvert(parser, () => ConsoleUI.PrintLine(errText));

        public static ConvertChain<T> Check<T>(
            this ConvertChain<T> chain,
            Converter<T, bool> checker,
            string errText
        )
            => chain.Check(checker, () => ConsoleUI.PrintLine(errText));

    }

}
