using System.Net;
using System.Net.Sockets;
using System.Text;

string response = "HTTP/1.1 200 OK\r\nDate: Sun, 10 Oct 2010 23:26:07 GMT\r\nServer: Apache/2.2.8 (Ubuntu) mod_ssl/2.2.8 OpenSSL/0.9.8g\r\nLast-Modified: Sun, 26 Sep 2010 22:04:35 GMT\r\nETag: \"45b6-834-49130cc1182c0\"\r\nAccept-Ranges: bytes\r\nContent-Length: 12\r\nConnection: close\r\nContent-Type: text/html\r\n\r\nHello world!";
var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

socket.Bind(new IPEndPoint(IPAddress.Any, 50000));

socket.Listen(0);

var handler = await socket.AcceptAsync();

while (true)
{
	var buffer = new byte[65536];
	var bytesReceived = await handler.ReceiveAsync(buffer, SocketFlags.None);
	var request = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
	buffer = Encoding.UTF8.GetBytes(response);

	await handler.SendAsync(buffer, SocketFlags.None);
}
