using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sniffer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			var hosts = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			var fetcherList = new List<Fetcher>();
			
			foreach (var host in hosts)
			{
				var fetcher = new Fetcher(host);
				fetcher.PacketReceivedEventHandler += new Fetcher.NewPacketReceivedEventHandler(OnNewPacket);
			}
		}

		private void OnNewPacket(Fetcher monitor, Packet p) => this.Dispatcher.Invoke(new Refresh(OnRefresh), p);

		private delegate void Refresh(Packet p);

		private void OnRefresh(Packet p)
		{
			dg.Items.Add(p);
		}

		private void dg_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			var item = (Packet)dg.SelectedItem;

			var content = new Content();
			content.utf8.Text = item.Utf8;
			content.hex.Text = item.Hex;

			content.ShowDialog();
		}
	}
}
