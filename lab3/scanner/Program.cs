using System.Net;
using System.Net.Sockets;


const int MaxPort = ushort.MaxValue;
const int MinPort = 0;
const int Step = 1024;

var host = Dns.GetHostEntry("localhost");
var ip = host.AddressList[1];

var tasks = new List<Task>();


for (int i = MinPort; i < MaxPort; i+=Step)
{
	var job = new Job { MinPort = i, MaxPort = i + Step};
	tasks.Add(Task.Run(() => Connections(job)));
}

await Task.WhenAll(tasks);


async Task<bool> Connect(Socket socket, IPEndPoint ipEP) 
{
	try
	{
		await Task.Run(() => socket.Connect(ipEP));
		return true;
	}
	catch (Exception)
	{
		return false;
	}
	finally 
	{
		if (socket.Connected)
			socket.Disconnect(false);
		
		socket.Close(); 
	}
}

async Task Connections(Job job)
{
	Console.WriteLine($"Range: {job.MinPort}-{job.MaxPort}");
	
	for (var i = job.MinPort; i < job.MaxPort; i++)
	{
		var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		if (await Connect(socket, new IPEndPoint(ip, i)))
		{
			Console.WriteLine($"Port: {i}");
		}
	}
}
struct Job
{
	public int MinPort;
	public int MaxPort;
}