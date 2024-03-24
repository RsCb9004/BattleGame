namespace RsCb.Console {

	internal struct ConsoleStyle {

		public ConsoleColor fontCol, backCol;

		public ConsoleStyle(
			ConsoleColor _fontCol = ConsoleColor.White,
			ConsoleColor _backCol = ConsoleColor.Black
		) {
			fontCol = _fontCol; backCol = _backCol;
		}
	}

	internal struct ConsoleMessage {

		public string content;
		public ConsoleStyle style;

		public ConsoleMessage(string _content, ConsoleStyle _style = default) {
			content = _content; style = _style;
		}
	}

	internal static class ConsoleIOer {

		public static void Clear() =>
			System.Console.Clear();

		public static void Print(string content, ConsoleColor fontCol, ConsoleColor backCol) {
			System.Console.ForegroundColor = fontCol;
			System.Console.BackgroundColor = backCol;
			System.Console.Write(content);
		}

		public static void Print(string content, ConsoleStyle style = default) =>
			Print(content, style.fontCol, style.backCol);

		public static void Print(ConsoleMessage message) =>
			Print(message.content, message.style);

		public static string Read(ConsoleColor fontCol, ConsoleColor backCol) {
			System.Console.ForegroundColor = fontCol;
			System.Console.BackgroundColor = backCol;
			return System.Console.ReadLine() ?? "";
		}

		public static string Read(ConsoleStyle style = default) =>
			Read(style.fontCol, style.backCol);

		public static string Ask(
			string content, ConsoleColor printFontCol, ConsoleColor printBackCol,
			ConsoleColor readFontCol, ConsoleColor readBackCol
		) {
			Print(content, printFontCol, printBackCol);
			return Read(readFontCol, readBackCol);
		}

		public static string Ask(
			string content, ConsoleStyle printStyle = default,
			ConsoleStyle readStyle = default
		) =>
			Ask(
				content, printStyle.fontCol, printStyle.backCol,
				readStyle.fontCol, readStyle.backCol
			);

		public static string Ask(ConsoleMessage printMessage, ConsoleStyle readStyle) =>
			Ask(printMessage.content, printMessage.style, readStyle);
	}
}
