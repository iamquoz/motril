using System.Net;
using System.Net.Sockets;
using System.Text;

var host = Dns.GetHostEntry("localhost");
var ip = host.AddressList[0];
var localEndPoint = new IPEndPoint(ip, 10001);

MakeRequest(ip, localEndPoint);

Console.Read();



static void MakeRequest(IPAddress ip, IPEndPoint localEndPoint)
{
	var sender = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

	sender.Connect(localEndPoint);

	Console.WriteLine("Input message: ");
	var msgText = Console.ReadLine();

	var msg = Encoding.UTF8.GetBytes(msgText ?? string.Empty);

	sender.Send(msg);

	var buffer = new byte[1024];
	sender.Receive(buffer);

	Console.WriteLine($"Received {Encoding.UTF8.GetString(buffer)}");

	if (msgText != string.Empty)
		MakeRequest(ip, localEndPoint);

	sender.Shutdown(SocketShutdown.Both);
	sender.Close();
}