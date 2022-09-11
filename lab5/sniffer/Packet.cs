using System;
using System.Net;
using System.Text;

namespace sniffer;

internal class Packet
{
	private readonly int LineCount = 30;

	public byte[] RawData { get; private set; }
	public Protocol ProtocolType { get; private set; }
	public IPAddress Source { get; private set; }
	public IPAddress Destination { get; private set; }
	public int SourcePort { get; private set; }
	public int DestinationPort { get; private set; }
	public DateTimeOffset FetchTime { get; private set; }
	public int PacketLength { get; private set; }


	private int HeaderLength { get; set; }

	
	public Packet(byte[] data)
	{
		ArgumentNullException.ThrowIfNull(data);

		FetchTime = DateTimeOffset.Now;
		RawData = data;


		// https://support.huawei.com/enterprise/en/doc/EDOC1000178017/dd76ea1f/ipv4-packet-format
		// https://en.wikipedia.org/wiki/IPv4#Packet_structure
		// https://github.com/manc0/Pepe-Sniffer
		if (Enum.IsDefined(typeof(Protocol), (int)data[9]))
			ProtocolType = (Protocol)data[9];
		else
			ProtocolType = Protocol.Unknown;

		PacketLength = data[2] * 256 + data[3];
		HeaderLength = (data[0] & 0x0f) * 4;

		Source = new IPAddress(BitConverter.ToUInt32(data, 12));
		Destination = new IPAddress(BitConverter.ToUInt32(data, 16));

		if (ProtocolType == Protocol.TCP || ProtocolType == Protocol.UDP)
		{
			SourcePort = data[HeaderLength] * 256 + data[HeaderLength + 1];
			DestinationPort = data[HeaderLength + 2] * 256 + data[HeaderLength + 3];
			if (ProtocolType == Protocol.TCP)
			{
				HeaderLength += 20;
			}
			else if (ProtocolType == Protocol.UDP)
			{
				HeaderLength += 8;
			}
		}
		else
		{
			SourcePort = -1;
			DestinationPort = -1;
		}

	}

	public string Hex => ToString(true);
	public string Utf8 => ToString(false);

	private string ToString(bool useHex)
	{
		switch (useHex)
		{
			case true:
				{
					var sb = new StringBuilder(RawData.Length);
					for (int i = HeaderLength; i < PacketLength; i += LineCount)
					{
						for (int j = i; j < PacketLength && j < i + LineCount; j++)
						{
							sb.Append(RawData[j].ToString("X2") + " ");
						}
						sb.Append(Environment.NewLine);
					}
					return sb.ToString();
				}
			case false:
				{
					var sb = new StringBuilder();

					for (int i = HeaderLength; i < PacketLength; i += LineCount)
					{
						for (int j = i; j < PacketLength && j < i + LineCount; j++)
						{
							if (RawData[j] > 31 && RawData[j] < 128)
								sb.Append((char)RawData[j]);
							else
								sb.Append('.');
						}
						sb.Append(Environment.NewLine);
					}
					return sb.ToString();
				}
		}
	}
}

internal enum Protocol
{
	Unknown = 0,
	ICMP = 1,
	IP = 4,
	TCP = 6,
	UDP = 17,
	IDP = 22
}
