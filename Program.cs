using RsCb.Console;
using RsCb.Messager;
using RsCb.TurnBasedGame;
using RsCb.TurnBasedGame.RPS;

internal static class Program {

	private readonly struct ConsoleStyles {
		public static readonly ConsoleStyle
			Read = new(ConsoleColor.Blue),
			Print = new(ConsoleColor.Cyan),
			PrintHighlight = new(ConsoleColor.Black, ConsoleColor.Cyan),
			Good = new(ConsoleColor.Green),
			GoodHighlight = new(ConsoleColor.Black, ConsoleColor.Green),
			Bad = new(ConsoleColor.Red),
			BadHighlight = new(ConsoleColor.Black, ConsoleColor.Red);
	}

	private readonly struct Messages {
		public static readonly ConsoleMessage
			Error = new("An error occurred. Further information:\n", ConsoleStyles.BadHighlight),
			AskMode = new("Mode (c: client, s: server): ", ConsoleStyles.Print),
			InvalidMode = new("Invalid input.\n", ConsoleStyles.Bad),
			AskIpClient = new("Server's ip address: ", ConsoleStyles.Print),
			AskPortClient = new("Port id: ", ConsoleStyles.Print),
			AskIpServer = new("Ip address: ", ConsoleStyles.Print),
			AskPortServer = new("Port id: ", ConsoleStyles.Print),
			OnBind = new("Binded. Waiting for client to join in...\n", ConsoleStyles.Print),
			OnConnect = new("Connected!\n", ConsoleStyles.GoodHighlight),
			Onstart = new("Game Start!\n", ConsoleStyles.PrintHighlight),
			OnNewRound = new("Round {0} | Score {1}:{2}\n", ConsoleStyles.Print),
			AskGesture = new("Your Gesture (r: rock, p: paper, s: scissors): ", ConsoleStyles.Print),
			InvalidGesture = new("Invalid Gesture.\n", ConsoleStyles.Bad),
			WaitGesture = new("Please wait for opponent's gesture...\n", ConsoleStyles.Print),
			ShowGesture = new("Opponent's Gesture: ", ConsoleStyles.Print),
			Tie = new("Tie.\n", ConsoleStyles.Print),
			Win = new("You win!\n", ConsoleStyles.Good),
			Lose = new("You lose!\n", ConsoleStyles.Bad);
	}

	private static IMessager? messager;

	private static void Connect() {
		string mode;
		mode = ConsoleIOer.Ask(Messages.AskMode, ConsoleStyles.Read);
		try {
			switch(mode) {
			case "c":
				ConnectAsClient();
				break;
			case "s":
				ConnectAsServer();
				break;
			default:
				ConsoleIOer.Print(Messages.InvalidMode);
				Connect();
				return;
			}
		} catch(Exception e) {
			ConsoleIOer.Print(Messages.Error);
			ConsoleIOer.Print(e.Message, ConsoleStyles.Bad);
			Connect();
			return;
		}
	}

	private static void ConnectAsClient() {
		string ip = ConsoleIOer.Ask(Messages.AskIpClient, ConsoleStyles.Read);
		int port = int.Parse(ConsoleIOer.Ask(Messages.AskPortClient, ConsoleStyles.Read));
		messager = new ClientTcpMessager(ip, port);
	}

	private static void ConnectAsServer() {
		string ip = ConsoleIOer.Ask(Messages.AskIpServer, ConsoleStyles.Read);
		int port = int.Parse(ConsoleIOer.Ask(Messages.AskPortServer, ConsoleStyles.Read));
		messager = new ServerTcpMessager(ip, port, delegate () {
			ConsoleIOer.Print(Messages.OnBind);
		});
	}

	private static RPSGame.Gesture GetGesture() {
		string gest = ConsoleIOer.Ask(Messages.AskGesture, ConsoleStyles.Read);
		switch(gest) {
		case "r":
			return RPSGame.Gesture.Rock;
		case "p":
			return RPSGame.Gesture.Paper;
		case "s":
			return RPSGame.Gesture.Scissors;
		default:
			ConsoleIOer.Print(Messages.InvalidGesture);
			return GetGesture();
		}
	}

	private static void Main() {
		Connect();
		ConsoleIOer.Print(Messages.OnConnect);
		RPSGame game = new();
		GameRunner<RPSGame.Gesture, bool, bool> gameRunner =
			new(game, GetGesture, messager!);
		gameRunner.Actions.onStart = delegate () {
			ConsoleIOer.Print(Messages.Onstart);
		};
		gameRunner.Actions.onNewRound = delegate () {
			ConsoleMessage message = Messages.OnNewRound;
			message.content = string.Format(
				message.content,
				game.Round, game.State.here, game.State.there
			);
			ConsoleIOer.Print(message);
		};
		gameRunner.Actions.onSendOperation = delegate () {
			ConsoleIOer.Print(Messages.WaitGesture);
		};
		gameRunner.Actions.onGetOperation = delegate (RPSGame.Gesture gesture) {
			ConsoleIOer.Print(Messages.ShowGesture);
			ConsoleIOer.Print(gesture.ToString() + "\n", ConsoleStyles.PrintHighlight);
		};
		gameRunner.Actions.onOperate = delegate () {
			switch(game.CurResult) {
			case RPSGame.RoundResult.Tie:
				ConsoleIOer.Print(Messages.Tie);
				break;
			case RPSGame.RoundResult.Win:
				ConsoleIOer.Print(Messages.Win);
				break;
			case RPSGame.RoundResult.Lose:
				ConsoleIOer.Print(Messages.Lose);
				break;
			}
		};
		gameRunner.Run();
	}
}
