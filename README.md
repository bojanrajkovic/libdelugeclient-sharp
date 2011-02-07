What Is It
===========

A Deluge client library, written in C#.

What It's Not
=============

* A Deluge RPC client (it's a JSON-RPC client, so it requires the WebUI).
* A generic JSON-RPC client (though the bits are all there, more or less).

How
===

* CookieClient (a modified WebClient that exposes cookies).
* The Deluge WebUI's JSON-RPC interface (http://deluge-host:webui-port/json).
* JSON serialization and deserialization by the simple, hackable Hyena.Json (http://git.gnome.org/browse/Hyena).

Requirements
============

A .NET 4.0/C# 4.0 runtime/compiler. Mono 2.8.x or later will do.

Why
===

There's no decent Deluge thin client for Mac--I'd rather not deal with the hassle of building Deluge
from source or MacPorts if it isn't even going to net me a proper app bundle. The Windows Python
build isn't exactly great either, and has freezing and quitting issues (as in, it sometimes freezes,
and sometimes it won't quit). A C# library might go toward helping both fronts along.

Using It
========

    using CodeRinseRepeat.Deluge;
    var client = new DelugeClient ("http://deluge-host", webui-port);
    client.Login ("delugePassword");
    var torrents = client.GetTorrents ();
    Console.WriteLine (torrents.First ());

Future
======

* Implement the *actual* Deluge RPC protocol, which I first need to either find thorough documentation on,
  or reverse engineer by sniffing the packets and reading the source as necessary.

* Async calls may need a better name, or a better implementation, or to be spun off into a type of their own. 
  Spawning off new tasks is simple though, and keeps the API clean.

* Make the code better.

License
=======

MIT/X11.
