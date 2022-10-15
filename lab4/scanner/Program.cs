using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


const int MaxPort = ushort.MaxValue;
const int MinPort = 0;
const int Step = 8192;

var countdown = new CountdownEvent(1);

var ips = new List<IPAddress>();
var tasksPerIp = new List<Task>();
var tasks = new List<Task>();

GetLocalIps();

foreach (IPAddress ip in ips)
	tasks.Add(Task.Run(() => RunScan(MaxPort, MinPort, Step, ip, tasksPerIp)));

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

async Task Connections(Job job, IPAddress ip)
{
	Console.WriteLine($"IP: {ip} | Range: {job.MinPort}-{job.MaxPort}");

	for (var i = job.MinPort; i < job.MaxPort; i++)
	{
		var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		if (await Connect(socket, new IPEndPoint(ip, i)))
		{
			Console.WriteLine($"OPEN: {ip}:{i}");
		}
	}
}

void GetLocalIps()
{
	Console.WriteLine("Local ips: ");
	for (int i = 0; i < 255; i++)
	{
		var sender = new Ping();
		sender.PingCompleted += new PingCompletedEventHandler(PingCompletedHandler);

		countdown.AddCount();
		
		var ip = $"192.168.1.{i}";
		sender.SendAsync(ip, 1000, ip);
	}

	countdown.Signal();
	countdown.Wait();
}

void PingCompletedHandler(object sender, PingCompletedEventArgs e)
{
	var ip = (string)e.UserState;
	if (e.Reply?.Status == IPStatus.Success)
	{
		Console.WriteLine(ip.ToString());
		ips.Add(IPAddress.Parse(ip));
	}

	countdown.Signal();
}

async Task RunScan(int MaxPort, int MinPort, int Step, IPAddress ip, List<Task> tasks)
{
	for (int i = MinPort; i < MaxPort; i += Step)
	{
		var job = new Job { MinPort = i, MaxPort = i + Step };
		tasks.Add(Task.Run(() => Connections(job, ip)));
	}

	await Task.WhenAll(tasks);
}

struct Job
{
	public int MinPort;
	public int MaxPort;
}