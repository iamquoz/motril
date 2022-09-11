using System.Net;
using System.Net.Sockets;
using System.Text;

var host = Dns.GetHostEntry("localhost");
var ip = host.AddressList[0];
var localEndPoint = new IPEndPoint(ip, 10001);

var listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

try
{
	listener.Bind(localEndPoint);
	listener.Listen(10);

	while (true)
	{
		Console.WriteLine("Waiting for a connection...");

		var handler = listener.Accept();
		var data = new byte[1024];
		var bytesRec = handler.Receive(data);
		var received = Encoding.UTF8.GetString(data);
		
		Console.WriteLine($"Text received: {received}");
		
		var msgByte = Encoding.UTF8.GetBytes($"Message {received} ack'd");
		handler.Send(msgByte);

		handler.Shutdown(SocketShutdown.Both);
		handler.Close();
	}
}
catch (Exception e)
{
	Console.WriteLine(e.ToString());
}
finally
{
	Console.Read();
}