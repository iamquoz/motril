using System.Diagnostics;
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

	var output = RunNslookup(request);
	output = output.Remove(output.LastIndexOf(Environment.NewLine));
	buffer = Encoding.UTF8.GetBytes(output);
	await handler.SendAsync(buffer, SocketFlags.None);
}

string RunNslookup(string target)
{
	Process cmd = new Process();
	cmd.StartInfo.FileName = $"nslookup";
	cmd.StartInfo.RedirectStandardInput = true;
	cmd.StartInfo.RedirectStandardOutput = true;
	cmd.StartInfo.CreateNoWindow = true;
	cmd.StartInfo.UseShellExecute = false;
	cmd.Start();

	cmd.StandardInput.WriteLine($"{target}");
	cmd.StandardInput.Flush();
	cmd.StandardInput.Close();

	var output = cmd.StandardOutput.ReadToEnd();
	
	cmd.WaitForExit();
	
	return output;
}