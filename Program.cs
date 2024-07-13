using BattleGame.TurnBasedGame;
using System.Net;

namespace BattleGame {

	internal static class Program {

		private delegate bool TryParse<T, TOut>(T input, out TOut output);

		private static TOut Ask<TOut>(TryParse<string, TOut> parser, string tip, string err) {
			string input = ConsoleUI.Ask(tip);
			if(parser.Invoke(input, out TOut res)) {
				return res;
			} else {
				ConsoleUI.PrintLine(err);
				return Ask(parser, tip, err);
			}
		}

		public static void Main() {
			ConsoleUI.DefaultFontCol = ConsoleColor.Blue;
			StartRoom();
		}

		private static void StartRoom() {
			string op = ConsoleUI.Ask("%+bEnter 'c' to create a room; enter 'j' to Join a room. > ");
			switch(op) {
			case "c": {
				CreateRoom();
				break;
			}
			case "j": {
				JoinRoom();
				break;
			}
			default: {
				ConsoleUI.PrintLine("%+cPlease enter 'c' or 'j'.");
				StartRoom();
				break;
			}
			}
		}

		private static void CreateRoom() {
			IPEndPoint ip = Ask<IPEndPoint?>(
				IPEndPoint.TryParse,
				"%+bIP Address: ",
				"%+cWrong URL format. Please try again."
			) ?? throw new NullReferenceException();
			string name = ConsoleUI.Ask("%+bYour Name: ");
			Room.Information info = new() {
				name = ConsoleUI.Ask("%+bRoom Name: "),
				description = ConsoleUI.Ask("%+bRoom Description: ")
			};
			int maxPlayer = Ask<int>(
				int.TryParse,
				"%+bMax Player: ",
				"%+cPlease enter an integer."
			);
			RoomServerUI ui = new();
			RoomServer room = new(ui, ip, maxPlayer, new(name), info);
			room.Lauch();
			ui.AskOperations();
		}

		public class RoomServerUI : RoomServer.IUser {

			public RoomServer Master { get; set; }

			public RoomServerUI() {
				Master = null!;
			}

			public void AskOperations() {
				Console.Clear();
				Console.SetCursorPosition(0, Master.MaxPlayer + 6);
				OnUpdated();
				while(AskOperation()) ;
			}

			public bool AskOperation() {
				string[] op = ConsoleUI.Ask("%+b> ").Split();
				switch(op[0]) {
				case "k":
				case "kick": {
					Remove(op);
					return true;
				}
				case "rf":
				case "refresh": {
					OnUpdated();
					return true;
				}
				case "": {
					return true;
				}
				default: {
					ConsoleUI.PrintLine("%+cUnknown command.");
					return true;
				}
				}
			}

			private void Remove(string[] op) {
				if(op.Length < 2) {
					ConsoleUI.PrintLine("%+bkick [INDEX]\nINDEX\tIndex of the player you want to kick.");
					return;
				}
				if(op.Length > 2) {
					ConsoleUI.PrintLine("%+cToo Many Arguments.");
					return;
				}
				if(!int.TryParse(op[1], out int index)) {
					ConsoleUI.PrintLine("%+cArgument should be an integer.");
					return;
				}
				if(index > Master.PlayerList.Count || index < 0) {
					ConsoleUI.PrintLine("%+cInvalid player index.");
					return;
				}
				if(index == 0) {
					ConsoleUI.PrintLine("%+cYou can't kick yourself.");
					return;
				}
				Master.RemovePlayer(index - 1);
			}

			public Joint<int> GetBattlers() {
				Joint<int> battlers = new(
					Ask<int>(
						int.TryParse,
						"%+bBattler 1 index: ",
						"%+cPlease enter an integer."
					),
					Ask<int>(
						int.TryParse,
						"%+bBattler 2 index: ",
						"%+cPlease enter an integer."
					)
				);
				return battlers;
			}

			public bool GetIfContinue() {
				return true;
			}

			public GameRunner.GameType GetGameType() {
				return GameRunner.GameType.RPS;
			}

			public GameRunner.IBattlerUIGetter GetBattlerUI() {
				throw new NotImplementedException();
			}

			public GameRunner.ISpectatorUIGetter GetSpectatorUI() {
				throw new NotImplementedException();
			}

			public void OnUpdated() {
				int curPosLeft = Console.CursorLeft;
				int curPosTop = Console.CursorTop;
				Console.SetCursorPosition(0, 0);
				for(int i = 0; i < Master.MaxPlayer + 6; i++)
					ConsoleUI.PrintLine();
				Console.SetCursorPosition(0, 0);
				ConsoleUI.PrintLine($"%+0%-f{Master.Info.name}");
				ConsoleUI.PrintLine($"%+7{Master.Info.description}");
				ConsoleUI.PrintLine($"%+bPlayer Count: {Master.PlayerList.Count + 1}/{Master.MaxPlayer + 1}");
				ConsoleUI.PrintLine();
				ConsoleUI.PrintLine($"%+e0. {Master.Host.name}");
				for(int i = 0; i < Master.MaxPlayer; i++) {
					if(i < Master.PlayerList.Count) {
						Room.PlayerStat playerStat = Master.PlayerList[i];
						ConsoleUI.PrintLine($"%+{(playerStat.joined ? 'a' : '2')}{i + 1}. {playerStat.player.name}");
					} else {
						ConsoleUI.PrintLine("%+7-");
					}
				}
				ConsoleUI.PrintLine();
				Console.SetCursorPosition(curPosLeft, curPosTop);
			}
		}

		private static void JoinRoom() {
			IPEndPoint ip = Ask<IPEndPoint?>(
				IPEndPoint.TryParse,
				"%+bIP Address: ",
				"%+cWrong URL format. Please try again."
			) ?? throw new NullReferenceException();
			string name = ConsoleUI.Ask("%+bYour Name: ");
			RoomClient room = new(new RoomClientUI(), ip, new(name));
			room.Lauch();
		}

		public class RoomClientUI : RoomClient.IUser {

			public RoomClient Master { get; set; }

			public RoomClientUI() {
				Master = null!;
			}

			public GameRunner.IBattlerUIGetter GetBattlerUI() {
				throw new NotImplementedException();
			}

			public GameRunner.ISpectatorUIGetter GetSpectatorUI() {
				throw new NotImplementedException();
			}

			public void OnUpdated() {
				Console.Clear();
				for(int i = 0; i < Master.MaxPlayer + 6; i++)
					ConsoleUI.PrintLine();
				Console.SetCursorPosition(0, 0);
				ConsoleUI.PrintLine($"%+0%-f{Master.Info.name}");
				ConsoleUI.PrintLine($"%+7{Master.Info.description}");
				ConsoleUI.PrintLine($"%+bPlayer Count: {Master.PlayerList.Count + 1}/{Master.MaxPlayer + 1}");
				ConsoleUI.PrintLine();
				ConsoleUI.PrintLine($"%+e0. {Master.Host.name}");
				for(int i = 0; i < Master.MaxPlayer; i++) {
					if(i < Master.PlayerList.Count) {
						Room.PlayerStat playerStat = Master.PlayerList[i];
						ConsoleUI.PrintLine($"%+{(playerStat.joined ? 'a' : '2')}{i + 1}. {playerStat.player.name}");
					} else {
						ConsoleUI.PrintLine("%+7-");
					}
				}
				ConsoleUI.PrintLine();
			}

			public bool CheckAccept() {
				return true;
			}
		}
	}

}
