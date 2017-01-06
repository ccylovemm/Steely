using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;

/// <summary>
/// This is an example of a stand-alone server. You don't need Unity in order to compile and run it.
/// Running it as-is will start a Game Server on ports 5127 (TCP) and 5128 (UDP), as well as a Lobby Server on port 5129.
/// </summary>

public class Application : IDisposable
{
	private string mFilename;
    private GameServer mGameServer = null;
    private LobbyServer mLobbyServer = null;

	public delegate bool HandlerRoutine (int type);
	[System.Runtime.InteropServices.DllImport("Kernel32")]
	static extern bool SetConsoleCtrlHandler (HandlerRoutine Handler, bool Add);
    static int Main(string[] args)
    {
        args = new string[]{"-loginPort" , "5127" , "-gatePort" , "5128"};

        if (args == null || args.Length == 0) return 0;

        int loginPort = 0;
        int gatePort = 0;
        for (int i = 0; i < args.Length / 2; i ++)
        {
            string param = args[i];
            string value = args[i + 1];
            if (param == "-loginPort")
            {
                loginPort = int.Parse(value);
            }
            else if (param == "-gatePort")
            {
                gatePort = int.Parse(value);
            }
        }
        Application app = new Application();
        app.Start(loginPort, gatePort);
        return 0;
    }

	public void Start (int tcpPort , int lobbyPort , string fn = "server.dat")
	{
		mFilename = fn;
		List<IPAddress> ips = Tools.localAddresses;
		string text = "\nLocal IPs: " + ips.size;

		for (int i = 0; i < ips.size; ++i)
		{
			text += "\n  " + (i + 1) + ": " + ips[i];
			if (ips[i] == Tools.localAddress) text += " (Primary)";
		}

		Console.WriteLine(text + "\n");
		{
			Tools.Print("External IP: " + Tools.externalAddress);

            mGameServer = new GameServer();
         //   mLobbyServer = new TcpLobbyServer();
         //   mLobbyServer.Start(lobbyPort);
        //    mGameServer.lobbyLink = new LobbyServerLink(mLobbyServer);
            mGameServer.Start(tcpPort);
            mGameServer.Load(mFilename);

			// This approach doesn't work on Windows 7 and higher.
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(delegate(object sender, EventArgs e) { Dispose(); });

			// This approach works only on Windows
			try { SetConsoleCtrlHandler(new HandlerRoutine(OnExit), true); }
			catch (Exception) { }

			for (; ; )
			{
				Thread.Sleep(10000);
			}
			Tools.Print("Shutting down...");
			Dispose();
		}
	}

	public void Dispose ()
	{
		// Stop the game server
		if (mGameServer != null)
		{
			mGameServer.Stop();
			mGameServer = null;
		}

		// Stop the lobby server
		if (mLobbyServer != null)
		{
			mLobbyServer.Stop();
			mLobbyServer = null;
		}
	}

    bool OnExit(int type) { Dispose(); return true; }
}
