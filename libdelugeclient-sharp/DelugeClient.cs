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

		public Dictionary<string, object> GetTorrents () {
			var fields = Torrent.Fields.All;

			var result = DoServiceCall ("core.get_torrents_status", new Dictionary<string, string> (), fields);

			if (result["error"] != null)
				throw new ApplicationException (string.Format ("Received error message from Deluge. Message: {0}", ((Dictionary<string, object>) result["error"])["message"]));

			return ((Dictionary<string, object>) result["result"]);
		}
	}
}

