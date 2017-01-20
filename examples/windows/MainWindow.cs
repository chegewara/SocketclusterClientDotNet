using System;
using ScClient;
using SuperSocket.ClientEngine;

public partial class MainWindow : Gtk.Window, BasicListener
{
	Socket socket;
	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();
		urlAddress.Text = "localhost:8000";
	}

	protected void OnDeleteEvent(object sender, Gtk.DeleteEventArgs a)
	{
		Gtk.Application.Quit();
		a.RetVal = true;
	}

	protected void OnConnectBtn(object sender, EventArgs e)
	{
		string url = "ws://" + urlAddress.Text + "/socketcluster/";
		//string url = "ws://localhost:8000/socketcluster/";
		socket = new Socket(url);
		socket.setListerner(this);
		socket.setReconnectStrategy(new ReconnectStrategy().setMaxAttempts(0));
		socket.connect();
		socket.on("echo", (name, data) => {
			OnMessage("echo", data.ToString());
		});
	}

	protected void OnDisconnectBtn(object sender, EventArgs e)
	{
		socket.disconnect();
	}


	// BasicListener implementation
	void BasicListener.onConnected(Socket socket)
	{
		OnMessage("Connected to", urlAddress.Text);
		string txt = "Connected";
		statusbar1.Push(statusbar1.GetContextId(txt), txt);
	}

	void BasicListener.onDisconnected(Socket socket)
	{
		OnMessage("", "Disconnected");
		string txt = "Disconnected";
		statusbar1.Push(statusbar1.GetContextId(txt), txt);
	}

	void BasicListener.onConnectError(Socket socket, ErrorEventArgs e)
	{
		OnMessage("error", e.Exception.Message);
	}

	void BasicListener.onAuthentication(Socket socket, bool status)
	{
		
	}

	void BasicListener.onSetAuthToken(string token, Socket socket)
	{
		
	}

	protected void OnSubscribeBtn(object sender, EventArgs e)
	{
		var channel = socket.createChannel(channelName.Text);
		channel.subscribe();
		channel.onMessage( (name, data) => {
			OnMessage(name, data.ToString());
		});
	}


	int count = 0;
	private void OnMessage(string name, string data)
	{
		var tag = new Gtk.TextTag(null);
		this.textview1.Buffer.TagTable.Add(tag);
		//tag.Weight = Pango.Weight.Bold;
		var iter = this.textview1.Buffer.GetIterAtLine(0);
		this.textview1.Buffer.InsertWithTags(ref iter, name + ": " + data.ToString() + "\n", tag);
		count++;
	}

	protected void OnUnsubscribeBtn(object sender, EventArgs e)
	{
		var channel = socket.getChannelByName(channelName.Text);
		if(channel != null)
			channel.unsubscribe();
	}

	protected void OnPublishBtn(object sender, EventArgs e)
	{
		socket.publish(channelName.Text, message.Text);
	}

	protected void OnEmitBtn(object sender, EventArgs e)
	{
		socket.emit("echo", echo.Text);
	}
}
