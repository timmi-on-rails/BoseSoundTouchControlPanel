using Bridge.Html5;
using Bridge.jQuery2;
using System;

namespace BoseSoundTouchControlPanel
{
	class Program
	{
		static void Get(string function, Action<DocumentInstance> success, HTMLButtonElement buttonToDisable = null)
		{
			if (buttonToDisable != null)
			{
				buttonToDisable.Disabled = true;
			}

			jQuery.Get($"http://192.168.0.54:8090/{function}")
				.Done((a, b, c) =>
				{
					success(c.ResponseXML);
				})
				.Fail(() =>
				{
					Window.Alert("Fehler");
				})
				.Always(() =>
				{
					if (buttonToDisable != null)
					{
						buttonToDisable.Disabled = false;
					}
				});
		}

		static void Post(string function, string data, HTMLButtonElement buttonToDisable = null, Action success = null)
		{
			if (buttonToDisable != null)
			{
				buttonToDisable.Disabled = true;
			}

			jQuery.Post($"http://192.168.0.54:8090/{function}", data)
				.Fail(() =>
				{
					Window.Alert("Fehler");
				})
				.Always(() =>
				{
					if (buttonToDisable != null)
					{
						buttonToDisable.Disabled = false;
					}
				})
				.Done(() =>
				{
					success?.Invoke();
				});
		}

		static void Main(string[] args)
		{
			var h = new HTMLLabelElement
			{
				InnerHTML = "Now Playing: "
			};

			Action up = () =>
			{
				Get("now_playing", (data) =>
				{
					h.InnerHTML = "Now Playing: " + data.FirstChild.ChildNodes[1].TextContent;
					Console.WriteLine(data.FirstChild);
				});
			};

			Document.Body.AppendChild(h);
			up();

			Get("presets", (r) =>
			{
				int i = 1;
				foreach (var child in r.FirstChild.ChildNodes)
				{
					int j = i;
					var name = child.FirstChild.FirstChild.TextContent;

					HTMLButtonElement button = new HTMLButtonElement
					{
						InnerHTML = name,
						OnClick = (o) =>
						{
							Post("key", $"<key state=\"release\" sender=\"Gabbo\">PRESET_{j}</key>", o.Target);
						}
					};

					button.Style.Display = "block";
					Document.Body.AppendChild(button);

					i++;
				}

			});


			HTMLButtonElement button2 = new HTMLButtonElement
			{
				InnerHTML = "Power",
				OnClick = (o) =>
				{
					Post("key", $"<key state=\"press\" sender=\"Gabbo\">POWER</key>", o.Target);
				}
			};

			button2.Style.Display = "block";

			Document.Body.AppendChild(button2);

			HTMLButtonElement button3 = new HTMLButtonElement
			{
				InnerHTML = "Volume Up",
				OnClick = (o) =>
				{
					Post("key", $"<key state=\"press\" sender=\"Gabbo\">VOLUME_UP</key>", o.Target, () =>
					{
						Post("key", $"<key state=\"release\" sender=\"Gabbo\">VOLUME_UP</key>", o.Target);
					});					
				}
			};

			button3.Style.Display = "block";

			Document.Body.AppendChild(button3);

			HTMLButtonElement button4 = new HTMLButtonElement
			{
				InnerHTML = "Volume Down",
				OnClick = (o) =>
				{
					Post("key", $"<key state=\"press\" sender=\"Gabbo\">VOLUME_DOWN</key>", o.Target, () =>
					{
						Post("key", $"<key state=\"release\" sender=\"Gabbo\">VOLUME_DOWN</key>", o.Target);
					});
				}
			};

			button4.Style.Display = "block";

			Document.Body.AppendChild(button4);

			WebSocket Socket = new WebSocket("ws://192.168.0.54:8080", "gabbo");

			Socket.OnOpen += (e) => Console.WriteLine("ws open");
			Socket.OnClose += (e) => Console.WriteLine("ws close");
			Socket.OnMessage += (e2) =>
			{
				Console.WriteLine("ws message: " + e2.Data);
				up();
			};

			Socket.OnError += (e) => Console.WriteLine("ws error");

		}
	}
}
