//
// Peer.cs
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

namespace CodeRinseRepeat.Deluge
{
	public class Peer
	{
		public static class Fields
		{
			public const string DownloadSpeed = "down_speed";
			public const string IP = "ip";
			public const string UploadSpeed = "up_speed";
			public const string Client = "client";
			public const string Country = "country";
			public const string Progress = "progress";
			public const string Seed = "seed";
		}

		public double DownloadSpeed { get; internal set; }
		public string IP { get; internal set; }
		public double UploadSpeed { get; internal set; }
		public string Client { get; internal set; }
		public string Country { get; internal set; }
		public double Progress { get; internal set; }
		public int Seed { get; internal set; }

		public override string ToString () {
			return string.Format ("[Peer: DownloadSpeed={0}, IP={1}, UploadSpeed={2}, Client={3}, Country={4}, Progress={5}," +
				"Seed={6}]", DownloadSpeed, IP, UploadSpeed, Client, Country, Progress, Seed
			);
		}

		public override bool Equals (object obj) {
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(Peer))
				return false;
			Peer other = (Peer) obj;
			return IP == other.IP;
		}


		public override int GetHashCode () {
			unchecked {
				return IP.GetHashCode ();
			}
		}
	}
}

