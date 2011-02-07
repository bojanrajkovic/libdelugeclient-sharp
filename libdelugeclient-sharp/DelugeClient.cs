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
using System.Diagnostics;
using Hyena.Json;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace CodeRinseRepeat.Deluge
{
	public class DelugeClient
	{
		public Uri ServiceUri { get; private set; }
		private long callId;
		private CookieContainer cookies;

		private static readonly Dictionary<string, string> emptyFilterDict = new Dictionary<string, string> ();
		private static readonly string[] emptyStringArray = new string[0];

		public DelugeClient (Uri serviceUri)
		{
			ServiceUri = serviceUri;
		}

		public DelugeClient (string uri, int port) : this (new Uri (string.Format ("{0}:{1}/json", uri, port))) {
		}

#if DEBUG
		public
#endif
		Dictionary<string, object> DoServiceCall (string method, params object[] parameters) {
			var callId = Interlocked.Increment (ref this.callId);

			var callObject = new Dictionary<string, object> {
				{"id", callId},
				{"method", method},
				{"params", parameters}
			};

#if DEBUG
			Console.Error.Write ("Serializing JSON-RPC call object...");
			Stopwatch s = new Stopwatch ();
			s.Start ();
#endif
			string jsonPayload = new Serializer (callObject).Serialize ();
#if DEBUG
			s.Stop ();
			Console.Error.WriteLine ("done in {0}.", s.Elapsed);
#endif

			var serviceClient = new CookieClient ();

			if (cookies != null) serviceClient.Cookies = cookies;

#if DEBUG
			Console.Error.Write ("Making request...");
			s.Reset ();
			s.Start ();
#endif

			byte[] returnData = serviceClient.UploadData (ServiceUri, Encoding.UTF8.GetBytes (jsonPayload));

#if DEBUG
			s.Stop ();
			Console.Error.WriteLine ("done in {0}.", s.Elapsed);
#endif

			cookies = serviceClient.Cookies;

#if DEBUG
			s.Reset ();
			Console.Error.Write ("Decompressing result...");
			s.Start ();
#endif
			// All this because deluge always returns gzip data, despite what you send for Accept-Encoding.
			var responseJson = new StreamReader (new GZipStream (new MemoryStream (returnData), CompressionMode.Decompress), Encoding.UTF8).ReadToEnd ();
#if DEBUG
			s.Stop ();
			Console.Error.WriteLine ("done in {0}.", s.Elapsed);
#endif

#if DEBUG
			s.Reset ();
			Console.Error.Write ("Deserializing result...");
			s.Start ();
#endif
			var response = new Deserializer (responseJson).Deserialize () as Dictionary<string, object>;
#if DEBUG
			s.Stop ();
			Console.Error.WriteLine ("done in {0}.", s.Elapsed);
#endif

			if ((long) response["id"] != callId)
				throw new ApplicationException (string.Format ("Response ID and original call ID don't match. Expected {0}, received {1}.", callId, response["id"]));

			if (response["error"] != null)
				throw new ApplicationException (string.Format ("Received error message from Deluge. Message: {0}", ((JsonObject) response["error"])["message"]));

			return response;
		}

		private void DoServiceCallAsync (string method, Action<Dictionary<string, object>> callback, params object[] parameters) {
			Task.Factory.StartNew (() => DoServiceCall (method, parameters)).ContinueWith (task => { if (callback != null) callback (task.Result); });
		}

		/*
		 * Interesting methods: { "core.upload_plugin", "core.glob",
		 * "core.remove_torrent", "core.resume_all_torrents", "core.queue_top", "core.set_torrent_options",
		 * "core.set_torrent_prioritize_first_last", "core.get_session_state", "core.set_torrent_move_completed",
		 * "core.set_torrent_file_priorities", "core.get_config", "core.disable_plugin",
		 * "core.test_listen_port", "core.connect_peer", "core.enable_plugin", "core.get_filter_tree",
		 * "core.set_torrent_remove_at_ratio", "core.get_config_values", "core.pause_torrent",
		 * "core.move_storage", "core.force_reannounce", "core.add_torrent_file", "core.get_listen_port",
		 * "core.set_torrent_move_completed_path", "core.set_torrent_stop_at_ratio", "core.rename_folder",
		 * "core.add_torrent_url", "core.get_enabled_plugins", "core.get_libtorrent_version", "core.get_path_size",
		 * "core.set_torrent_max_connections", "core.get_config_value", "core.get_session_status", "core.create_torrent",
		 * "core.add_torrent_magnet", "core.set_torrent_stop_ratio", "core.set_torrent_auto_managed",
		 * "core.pause_all_torrents", "core.rename_files", "core.get_free_space",
		 * "core.queue_bottom", "core.set_torrent_max_upload_speed", "core.resume_torrent",
		 * "core.set_torrent_max_upload_slots", "core.set_config", "core.get_cache_status", "core.queue_down",
		 * "core.get_num_connections", "core.set_torrent_max_download_speed", "core.queue_up", "core.set_torrent_trackers"
		 * }
		 *
		 * Implemented methods: {
		 *  auth.login, system.listMethods, core.get_torrents_status, core.get_available_plugins,
		 *  core.rescan_plugins, core.get_torrent_status
		 * }
		 */

		public Torrent GetTorrentStatus (Torrent t, string[] fields) {
			var result = DoServiceCall ("core.get_torrent_status", t.Hash, fields);
			var torrentObject = (JsonObject) result["result"];

			if (!t.Hash.Equals ((string) torrentObject["hash"]))
				throw new ApplicationException (String.Format ("Hashes don't match, expected {0}, got {1}!", t.Hash, torrentObject["hash"]));

			return t.UpdateFrom (torrentObject);
		}

		public Torrent GetTorrentStatus (Torrent t) {
			return GetTorrentStatus (t, emptyStringArray);
		}

		public void RescanPlugins () {
			DoServiceCall ("core.rescan_plugins");
		}

		public void RescanPluginsAsync () {
			DoServiceCallAsync ("core.rescan_plugins", null);
		}

		public bool Login (string password) {
			var result = DoServiceCall ("auth.login", password);
			return (bool) result["result"];
		}

		public void LoginAsync (string password, Action<bool> callback) {
			Task.Factory.StartNew (() => Login (password)).ContinueWith (task => callback (task.Result));
		}

		public IEnumerable<string> ListMethods () {
			var result = DoServiceCall ("system.listMethods");
			return ((IList<object>) result["result"]).Cast<string> ();
		}

		public void ListMethodsAsync (Action<IEnumerable<string>> callback) {
			Task.Factory.StartNew (() => ListMethods ()).ContinueWith (task => callback (task.Result));
		}

		public IEnumerable<Torrent> GetTorrents () {
			var result = DoServiceCall ("core.get_torrents_status", emptyFilterDict, emptyStringArray);
			var torrentsDict = (Dictionary<string, object>) result["result"];

			var torrents = new List<Torrent> (torrentsDict.Count);

			foreach (var hash in torrentsDict.Keys) {
				JsonObject data = (JsonObject) torrentsDict[hash];
				torrents.Add (Torrent.TorrentFromJsonObject (data));
			}

			return torrents;
		}

		public void GetTorrentsAsync (Action<IEnumerable<Torrent>> callback) {
			Task.Factory.StartNew (() => GetTorrents ()).ContinueWith (task => callback (task.Result));
		}

		public IEnumerable<string> GetAvailablePlugins () {
			var result = DoServiceCall ("core.get_available_plugins");
			return ((IList<object>) result["result"]).Cast<string> ();
		}

		public void GetAvailablePluginsAsync (Action<IEnumerable<string>> callback) {
			Task.Factory.StartNew (() => GetAvailablePlugins ()).ContinueWith (task => callback (task.Result));
		}
	}
}

