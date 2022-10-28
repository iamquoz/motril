using System.Net;
using System.Net.Sockets;
using System.Text;

var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

socket.Bind(new IPEndPoint(IPAddress.Any, 50000));

socket.Listen(0);

var handler = await socket.AcceptAsync();

while (true)
{
	var buffer = new byte[65536];
	var bytesReceived = await handler.ReceiveAsync(buffer, SocketFlags.None);
	var request = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

	if (request.Contains("GET")) {
		var page = request.Split(" ").Last().TrimEnd(Environment.NewLine.ToCharArray());
		Console.WriteLine(page);
		if (page == "/index.html")
			buffer = Encoding.UTF8.GetBytes("<html><head></head><body><h1>test</h1></body></html>\n");
		else
			buffer = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\n");
	}
	else
	{
		buffer = Encoding.UTF8.GetBytes("HTTP/1.1 419 Method not supported\n");
	}
	await handler.SendAsync(buffer, SocketFlags.None);
}