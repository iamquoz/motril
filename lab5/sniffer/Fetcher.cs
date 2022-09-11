using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace sniffer;

internal class Fetcher
{
	private const int BufferSize = 1024 * 1024;
	private readonly Socket Socket;
	private readonly byte[] Buffer;
	
	public Fetcher(IPAddress ip)
	{
		Socket = new Socket(ip.AddressFamily == AddressFamily.InterNetwork ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.IP);

		Buffer = new byte[BufferSize];
		Socket.Bind(new IPEndPoint(ip, 0));
		Socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), null);
		Socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveEvent), null);
	}

	private void ReceiveEvent(IAsyncResult ar)
	{
		int length = Socket.EndReceive(ar);
		var packet = new byte[length];
		

		Array.Copy(Buffer, 0, packet,0, length);

		PacketReceivedEventHandler.Invoke(this, new Packet(packet));
		
		
		Socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveEvent), null);		
	}

	public void Close()
	{
		Socket.Close();
	}

	public delegate void NewPacketReceivedEventHandler(Fetcher fetcher, Packet packet);

	public event NewPacketReceivedEventHandler PacketReceivedEventHandler;
}
