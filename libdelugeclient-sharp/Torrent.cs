//
// Torrent.cs
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
using System.Collections.Generic;
using System.Linq;

namespace CodeRinseRepeat.Deluge
{
	public class Torrent
	{
		public static class Fields
		{
			public const string Name = "name";
			public const string State = "state";
			public const string SavePath = "save_path";
			public const string MaxDownloadSpeed = "max_download_speed";
			public const string MaxUploadSpeed = "max_upload_speed";
			public const string DownloadSpeed = "download_payload_rate";
			public const string UploadSpeed = "upload_payload_rate";
			public const string ConnectedSeeds = "num_seeds";
			public const string TotalSeeds = "total_seeds";
			public const string ConnectedPeers = "num_peers";
			public const string TotalPeers = "total_peers";
			public const string ETA = "eta";
			public const string Downloaded = "total_done";
			public const string Uploaded = "total_uploaded";
			public const string TotalSize = "total_size";
			public const string Progress = "progress";
			public const string Label = "label";
			public const string Trackers = "trackers";
			public const string Files = "files";
			public const string Index = "index";
			public const string Path = "path";
			public const string Size = "size";
			public const string FileProgress = "file_progress";
			public const string FilePriorities = "file_priorities";
			public const string Queue = "queue";
			public const string Ratio = "ratio";
			public const string DistributedCopies = "distributed_copies";
			public const string IsAutoManaged = "is_auto_managed";
			public const string TimeAdded = "time_added";
			public const string TrackerHost = "tracker_host";

			public static readonly string[] All = new string[] {
				Name, State, SavePath, MaxDownloadSpeed, MaxUploadSpeed, DownloadSpeed, UploadSpeed, ConnectedSeeds,
				TotalSeeds, ConnectedPeers, TotalPeers, ETA, Downloaded, Uploaded, TotalSize, Progress, Label, Trackers,
				Files, Index, Path, Size, FileProgress, FilePriorities, Queue, Ratio, DistributedCopies, IsAutoManaged,
				TimeAdded, TrackerHost
			};

			public static readonly string[] WebUIDefaults = new string[] {
				Queue, Name, TotalSize, State, Progress, ConnectedSeeds, TotalSeeds, ConnectedPeers, TotalPeers,
				DownloadSpeed, UploadSpeed, ETA, Ratio, DistributedCopies, IsAutoManaged, TimeAdded, TrackerHost, SavePath,
				Label
			};
		}

		public override string ToString () {
			return string.Format ("[Torrent: MaxDownloadSpeed={0}, DownloadSpeed={1}, ConnectedPeers={2}, Ratio={3}," +
				"TotalPeers={4}, TotalSize={5}, State={6}, MaxUploadSpeed={7}, ETA={8}, SavePath={9}, Progress={10}," +
				"TimeAdded={11}, TrackerHost={12}, TotalUploaded={13}, Files=[{14}], Downloaded={15}, TotalSeeds={16}," +
				"ConnectedSeeds={17}, Name={18}, Trackers=[{19}], IsAutoManaged={20}, Queue={21}, DistributedCopies={22}]",
				MaxDownloadSpeed, DownloadSpeed, ConnectedPeers, Ratio, TotalPeers, TotalSize, State, MaxUploadSpeed, ETA,
				SavePath, Progress, TimeAdded, TrackerHost, TotalUploaded,
				string.Join (",", Files.Select (f => f.ToString ())), Downloaded, TotalSeeds, ConnectedSeeds,
				Name, string.Join (",", Trackers.Select (t => t.ToString ())), IsAutoManaged, Queue,
				DistributedCopies
			);
		}

		public string Hash { get; internal set; }
		public double MaxDownloadSpeed { get; internal set; }
		public double DownloadSpeed { get; internal set; }
		public int ConnectedPeers { get; internal set; }
		public double Ratio { get; internal set; }
		public int TotalPeers { get; internal set; }
		public int TotalSize { get; internal set; }
		public State State { get; internal set; }
		public double MaxUploadSpeed { get; internal set; }
		public int ETA { get; internal set; }
		public string SavePath { get; internal set; }
		public double Progress { get; internal set; }
		public DateTime TimeAdded { get; internal set; }
		public string TrackerHost { get; internal set; }
		public int TotalUploaded { get; internal set; }
		public IEnumerable<File> Files { get; internal set; }
		public int Downloaded { get; internal set; }
		public int TotalSeeds { get; internal set; }
		public int ConnectedSeeds { get; internal set; }
		public string Name { get; internal set; }
		public IEnumerable<Tracker> Trackers { get; internal set; }
		public bool IsAutoManaged { get; internal set; }
		public int Queue { get; internal set; }
		public double DistributedCopies { get; internal set; }
	}
}

