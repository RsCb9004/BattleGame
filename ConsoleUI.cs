namespace BattleGame {

	public static class ConsoleUI {

		public class WrongFormatException : Exception { }

		public static ConsoleColor DefaultFontCol { get; set; }
		public static ConsoleColor DefaultBackCol { get; set; }

		static ConsoleUI() {
			DefaultFontCol = ConsoleColor.White;
			DefaultBackCol = ConsoleColor.Black;
		}

		public static void Print(string text) {
			ResetColor();
			for(int i = 0; i < text.Length; i++) {
				Console.Write(GetRawText(text, ref i));
			}
			ResetColor();
		}

		public static void PrintLine(string text = "") {
			ResetColor();
			for(int i = 0; i < text.Length; i++) {
				string raw = GetRawText(text, ref i);
				if(!PrintLineRaw(raw)) return;
			}
			int width = GetWidth();
			ResetColor();
			Console.WriteLine(new string(' ', width));
		}

		public static string Ask(
			ConsoleColor FontCol,
			ConsoleColor BackCol,
			string text = ""
		) {
			Print(text);
			Console.ForegroundColor = FontCol;
			Console.BackgroundColor = BackCol;
			string res = Console.ReadLine() ?? "";
			Console.ForegroundColor = DefaultFontCol;
			Console.BackgroundColor = DefaultBackCol;
			return res;
		}

		public static string Ask(string text = "") =>
			Ask(DefaultFontCol, DefaultBackCol, text);

		private static string GetRawText(string text, ref int cur) {
			if(text[cur] == '%') {
				switch(text[++cur]) {
				case '+': {
					Console.ForegroundColor = ToColor(text[++cur]);
					return "";
				}
				case '-': {
					Console.BackgroundColor = ToColor(text[++cur]);
					return "";
				}
				case '=': {
					ResetColor();
					return "";
				}
				case '%': {
					return "%";
				}
				default: {
					throw new WrongFormatException();
				}
				}
			}
			int nxt = text.IndexOf('%', cur);
			if(nxt == -1) nxt = text.Length;
			string res = text[cur..nxt];
			cur = nxt - 1;
			return res;
		}

		private static bool PrintLineRaw(string text) {
			int width, cur, next = 0;
			while(true) {
				width = GetWidth();
				if(width <= 3) {
					Console.WriteLine(new string('.', width));
					return false;
				}
				cur = next;
				next = cur + width / 2 - 1;
				Console.Write(text[cur..int.Min(next, text.Length)]);
				if(next >= text.Length) return true;
			}
		}

		private static void ResetColor() {
			Console.ForegroundColor = DefaultFontCol;
			Console.BackgroundColor = DefaultBackCol;
		}

		private static int GetWidth() {
			return Console.BufferWidth - Console.CursorLeft;
		}

		private static ConsoleColor ToColor(char code) {
			if(char.IsDigit(code))
				return (ConsoleColor)code - '0';
			if(char.IsAsciiHexDigitLower(code))
				return (ConsoleColor)code - 'a' + 10;
			throw new WrongFormatException();
		}
	}
}
