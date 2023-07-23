using Sandbox;
using Sandbox.Buttplug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
	public class Buttplug
	{
		public static ButtplugClient Client;
		public Buttplug() 
		{
			Event.Register( this );
			Client = new( "S&Box Buttplug Pong" );
			Client.DeviceAdded += Client_DeviceAdded;
			Client.DeviceRemoved += Client_DeviceRemoved;
			Client.ServerDisconnect += Client_ServerDisconnect;
			Buttplug_Connect();
		}

		private void Client_DeviceRemoved( object? sender, DeviceRemovedEventArgs e )
		{
			Event.Run( "buttplug.deviceremoved", Client, e.Device );
		}

		private void Client_DeviceAdded( object? sender, DeviceAddedEventArgs e )
		{
			Event.Run( "buttplug.deviceadded", Client, e.Device );
		}

		private void Client_ServerDisconnect( object? sender, EventArgs e )
		{
			Event.Run( "buttplug.disconnect", Client );
		}

		[ConVar.Client( "buttplug_url" )]
		public static string ButtplugURL { get; set; } = "ws://localhost:8080";

		[ConCmd.Client( "buttplug_connect" )]
		public static void Buttplug_Connect()
		{
			Log.Info( $"Connecting to buttplug server {ButtplugURL}" );
			GameTask.RunInThreadAsync( async () =>
			{
				await Client.ConnectAsync(ButtplugURL);
				Event.Run( "buttplug.connected", Client );
			} );
		}

		private async void VibrateAll(float strength, int ms)
		{
			// Enable
			foreach ( var device in Client.Devices )
			{
				await device.SendVibrateCmd( strength );
			}
			// Wait
			await GameTask.Delay( ms );
			// Disable
			foreach ( var device in Client.Devices )
			{
				await device.SendVibrateCmd( 0 );
			}
		}

		[Event( "pong.paddlecollide" )]
		private async void OnPaddleCollide( Ball ball, Paddle paddle )
		{
			if (paddle.IsPlayer)
			{
				VibrateAll( 1, 250 );
				//TODO: make strength dependant on tally?
			}
		}

		[Event( "pong.ballhitgoal" )]
		private void OnBallHitGoal( Ball ball, bool isLeftGoal )
		{
			var seconds = isLeftGoal ? 1 : 2;
			VibrateAll( 1, 1000 * seconds );
			//TODO: make strength dependant on tally?
		}
	}
}
