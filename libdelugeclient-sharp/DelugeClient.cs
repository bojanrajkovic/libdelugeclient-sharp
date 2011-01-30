// 
// DelugeClient.cs
//  
// Author:
//       Bojan Rajkovic <brajkovic@coderinserepeat.com>
// 
// Copyright (c) 2011 Bojan Rajkovic
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hyena.Json;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CodeRinseRepeat.Deluge
{
	public class DelugeClient
	{
		public Uri ServiceUri { get; private set; }
		private WebClient serviceClient;
		private int callId;

		private static readonly DateTime unixTime = new DateTime (1970, 1, 1, 0, 0, 0, 0);

		public DelugeClient (Uri serviceUri)
		{
			ServiceUri = serviceUri;
			serviceClient = new CookieClient ();
		}

		public DelugeClient (string uri, int port) : this (new Uri (string.Format ("{0}:{1}", uri, port))) {
		}

		private Dictionary<string, object> DoServiceCall (string method, params object[] parameters) {
			var callObject = new Dictionary<string, object> {
				{"id", ++callId},
				{"method", method},
				{"params", parameters}
			};

			string jsonPayload = new Serializer (callObject).Serialize ();

			byte[] returnData = serviceClient.UploadData (ServiceUri, Encoding.UTF8.GetBytes (jsonPayload));

			// All this because deluge always returns gzip data, despite what you send for Accept-Encoding.
			var responseReader = new StreamReader (new GZipStream (new MemoryStream (returnData), CompressionMode.Decompress), Encoding.UTF8);

			return new Deserializer (responseReader).Deserialize () as Dictionary<string, object>;
		}

		private void DoServiceCallAsync (string method, Action<Dictionary<string, object>> callback, params object[] parameters) {
			Task.Factory.StartNew (() => DoServiceCall (method, parameters)).ContinueWith (task => callback (task.Result));
		}

		public bool Login (string password) {
			var result = DoServiceCall ("auth.login", password);

			if (result["error"] != null)
				throw new ApplicationException (string.Format ("Received error message from Deluge. Message: {0}", ((Dictionary<string, object>) result["error"])["message"]));
			else return (bool) result["result"];
		}

		public void LoginAsync (string password, Action<bool> callback) {
			Task.Factory.StartNew (() => Login (password)).ContinueWith (task => callback (task.Result));
		}

		public IEnumerable<string> ListMethods () {
			var result = DoServiceCall ("system.listMethods");

			if (result["error"] != null)
				throw new ApplicationException (string.Format ("Received error message from Deluge. Message: {0}", ((Dictionary<string, object>) result["error"])["message"]));
			else return ((List<object>) result["result"]).Cast<string> ();
		}

		public void ListMethodsAsync (Action<IEnumerable<string>> callback) {
			Task.Factory.StartNew (() => ListMethods ()).ContinueWith (task => callback (task.Result));
		}

		public Dictionary<string, object> GetTorrentInfo (string torrentHash) {
			var result = DoServiceCall ("web.get_torrent_info");

			if (result["error"] != null)
				throw new ApplicationException (string.Format ("Received error message from Deluge. Message: {0}", ((Dictionary<string, object>) result["error"])["message"]));
			else return ((Dictionary<string, object>) result["result"]);
		}

		public IEnumerable<Torrent> GetTorrents () {
			var fields = Torrent.Fields.All;

			var result = DoServiceCall ("core.get_torrents_status", new Dictionary<string, string> (), fields);

			if (result["error"] != null)
				throw new ApplicationException (string.Format ("Received error message from Deluge. Message: {0}", ((Dictionary<string, object>) result["error"])["message"]));

			var torrentsDict = (Dictionary<string, object>) result["result"];

			foreach (var hash in torrentsDict.Keys) {
				JsonObject data = (JsonObject) torrentsDict[hash];

				yield return new Torrent {
					ConnectedPeers = (int) data[Torrent.Fields.ConnectedPeers],
					ConnectedSeeds = (int) data[Torrent.Fields.ConnectedSeeds],
					DistributedCopies = Convert.ToDouble (data[Torrent.Fields.DistributedCopies]),
					Downloaded = (int) data[Torrent.Fields.Downloaded],
					DownloadSpeed = Convert.ToDouble (data[Torrent.Fields.DownloadSpeed]),
					ETA = (int) data[Torrent.Fields.ETA],
					Files = GetFiles (torrentsDict, hash),
					Hash = hash,
					IsAutoManaged = (bool) data[Torrent.Fields.IsAutoManaged],
					MaxDownloadSpeed = Convert.ToDouble (data[Torrent.Fields.MaxDownloadSpeed]),
					MaxUploadSpeed = Convert.ToDouble (data[Torrent.Fields.MaxUploadSpeed]),
					Name = (string) data[Torrent.Fields.Name],
					Progress = Convert.ToDouble (data[Torrent.Fields.Progress]),
					Queue = (int) data[Torrent.Fields.Queue],
					Ratio = Convert.ToDouble (data[Torrent.Fields.Ratio]),
					SavePath = (string) data[Torrent.Fields.SavePath],
					State = (State) Enum.Parse (typeof (State), (string) data[Torrent.Fields.State]),
					TimeAdded = unixTime.AddSeconds ((double) data[Torrent.Fields.TimeAdded]),
					TotalPeers = (int) data[Torrent.Fields.TotalPeers],
					TotalSeeds = (int) data[Torrent.Fields.TotalSeeds],
					TotalSize = (int) data[Torrent.Fields.TotalSize],
					TotalUploaded = (int) data[Torrent.Fields.Uploaded],
					TrackerHost = (string) data[Torrent.Fields.TrackerHost],
					Trackers = GetTrackers (torrentsDict, hash),
				};
			}
		}

		private IEnumerable<File> GetFiles (Dictionary<string, object> data, string hash) {
			JsonObject torrentData = (JsonObject) data[hash];
			JsonArray files = (JsonArray) torrentData[Torrent.Fields.Files];
			JsonArray fileProgress = (JsonArray) torrentData[Torrent.Fields.FileProgress];
			JsonArray filePriority = (JsonArray) torrentData[Torrent.Fields.FilePriorities];

			foreach (JsonObject file in files) {
				int index = (int) file[File.Fields.Index];

				int priority = (int) filePriority[index];
				double progress = Convert.ToDouble (fileProgress[index]);

				yield return new File {
					Index = index,
					Offset = (int) file[File.Fields.Offset],
					Path = (string) file[File.Fields.Path],
					Priority = priority,
					Progress = progress,
					Size = (int) file[File.Fields.Size],
				};
			}
		}

		private IEnumerable<Tracker> GetTrackers (Dictionary<string, object> data, string hash) {
			JsonObject torrentData = (JsonObject) data[hash];
			JsonArray trackers = (JsonArray) torrentData[Torrent.Fields.Trackers];

			foreach (JsonObject tracker in trackers) {
				yield return new Tracker {
					Url = (string) tracker[Tracker.Fields.Url],
					Tier = (int) tracker[Tracker.Fields.Tier],
				};
			}
		}
	}
}

